# RSD: Highlight Newly Added Task Row

## Status

`Draft — 2026-08-14`

Branch: `feat-task-add-highlight`

No dependency on `009` or `010`/`011`. Self-contained UI addition to the
task list add flow.

## Background

Today, adding a root task or a subtask inserts a new row into `TaskList`
with no visual acknowledgement beyond the row simply appearing. In a long
or scrolled list, a newly added subtask (inserted at the end of its
parent's children) may render off-screen, so the user gets no feedback at
all that the add succeeded. Product owner wants a brief highlight flash on
the new row, paired with scrolling it into view if it's off-screen.

## Current State (verified against codebase)

- `TaskList.razor` renders each node with `@key="node.Task.Id"`
  (`TaskList.razor:8,41`), so a newly added task is a freshly-mounted DOM
  node — a one-shot highlight keyed to that mount is cheap and reliable.
- Root add: `TaskListBase.HandleAddTask()` (`TaskList.razor.cs:236-260`)
  invokes `OnTaskAdd` → `IndexBase.HandleTaskAdd`
  (`Index.razor.Tasks.cs:29-66`), which calls `TaskService.AddTaskAsync`
  and then reads `TaskService.CurrentTaskId` (line 37) — `AddTaskAsync`
  (`TaskService.cs:144-168,170-179`) already sets `_appState.CurrentTaskId
  = task.Id` synchronously before returning. This is a reliable existing
  signal for "which task was just added" on the root path. No signature
  change needed here.
- Subtask add: `TaskItemBase.HandleAddSubtask()`
  (`TaskItemComponent.razor.cs:270-278`) invokes `OnAddSubtask` →
  `IndexBase.HandleAddSubtask` (`Index.razor.Tasks.cs:173-181`), which
  calls `TaskService.AddSubtaskAsync(request.Name, request.ParentTaskId)`
  (`TaskService.cs:213-252`). This method returns `Task` (void) and does
  **not** set `CurrentTaskId` or expose the new subtask's `Id` anywhere.
  It also has two silent-failure early returns (invalid name at line
  216-217, missing parent at line 219-220). **A signature change is
  required**: `AddSubtaskAsync` must return `Task<Guid?>` — the new
  subtask's `Id` on success, `null` on either early-return failure path.
  This is the one interface-level change in scope (`ITaskService.cs:39`),
  with a single call site to update (`Index.razor.Tasks.cs:177`).
- `TaskItemComponent` (`TaskItemComponent.razor` / `TaskItemBase.cs`) has
  an existing per-item class hook, `GetTaskClass()`
  (`TaskItemBase.cs:123-130`), joined into `class="task-row @GetTaskClass()"`
  (`TaskItemComponent.razor:4`). This is the natural place to append a new
  conditional class for the highlight.
- `TaskItemBase` already has precedent for a one-shot post-render action
  gated by a flag cleared on first use: `_shouldFocusInlineEdit` /
  `OnAfterRenderAsync` (`TaskItemBase.cs:323-330`), which focuses
  `_inlineEditInput` once and clears the flag. The highlight-and-scroll
  behavior follows the same shape.
- No scroll-into-view JS interop exists anywhere in the codebase (0
  matches for `scrollIntoView` repo-wide). Existing interop modules
  (`wwwroot/js/infiniteScroll.js` etc.) follow a `window.<name>Interop =
  { method(...) {...} }` convention invoked via
  `IJSRuntime.InvokeAsync<T>("<name>Interop.method", ...)`. Neither
  `TaskList.razor.cs` nor `TaskItemComponent.razor.cs` currently injects
  `IJSRuntime` — this will be newly added to `TaskItemComponent.razor.cs`.
- No one-shot CSS flash animation exists. Existing keyframes (`sd-pulse`,
  `ls-draw-*` in `wwwroot/css/app.css:722-731,1927-1958`) are all
  `infinite`-looping and driven by a static class being present — not a
  precedent for a timed one-shot. This RSD introduces that pattern.
- No `prefers-reduced-motion` media query exists anywhere in the
  codebase (0 matches). This RSD introduces the first one, scoped to the
  new highlight animation only — not a general audit of existing
  `infinite` animations (out of scope, see Open Questions).
- `TaskItem.Id` is `Guid` (`Models/TaskItem.cs:56`). Subtask linkage is
  `ParentTaskId`/`GoogleParentTaskId` (`TaskItem.cs:108,111`) — irrelevant
  to this feature beyond confirming `Id` is the correct highlight key.

## Decisions (locked)

**D1 — Scope: root task add and subtask add only.** Per product owner.
Imported/synced/restored tasks do not trigger the highlight — those are
not user-initiated "I just typed this and hit add" moments, and a bulk
Google sync could otherwise flash dozens of rows at once.

**D2 — Visual: single flash-fade, ~1.1s, theme-accent tint, one-shot.**
The row background transitions from an accent tint to transparent once,
using the existing theme accent color (Deep Forest green — reuse the
existing CSS variable, not a new hardcoded color). Respects
`prefers-reduced-motion: reduce` — reduced-motion users get the class
applied and removed with no animated transition (effectively no visible
flash), consistent with treating motion as strictly decorative.

**D3 — Auto-scroll paired with the flash.** If the new row is outside the
visible scroll area of the task list, it is scrolled into view
(`block: 'nearest'`, smooth behavior) so the flash is guaranteed visible.
No scroll occurs if the row is already visible.

**D4 — Signal mechanism: return the new Id, not list-diffing.** Root add
reuses the existing `TaskService.CurrentTaskId` signal (no change).
Subtask add requires `AddSubtaskAsync` to return `Task<Guid?>` (D-locked
above). Diffing the `Tasks` list before/after was considered and
rejected — it's fragile against concurrent Google sync insertions and
more code than threading one Id through an existing call chain.

## Requirements (EARS)

### Trigger

- **REQ-1**: WHEN a root task is successfully added via `HandleTaskAdd`,
  THE SYSTEM SHALL mark the resulting task's `Id` (from
  `TaskService.CurrentTaskId`) as the highlight target for the next
  render of `TaskList`.
- **REQ-2**: WHEN a subtask is successfully added via `HandleAddSubtask`
  (i.e. `AddSubtaskAsync` returns a non-null `Guid`), THE SYSTEM SHALL
  mark that returned `Id` as the highlight target for the next render of
  `TaskList`.
- **REQ-3**: WHEN `AddSubtaskAsync` returns `null` (invalid name or
  missing parent), THE SYSTEM SHALL NOT set a highlight target and SHALL
  NOT throw — existing silent-failure behavior is preserved.

### Rendering

- **REQ-4**: WHEN `TaskList` renders a task node whose `Id` equals the
  current highlight target, THE SYSTEM SHALL apply an additional CSS
  class (`task-row--new`) to that row's `TaskItemComponent`.
- **REQ-5**: WHEN a row is rendered with `task-row--new`, THE SYSTEM
  SHALL play a one-shot background flash-to-transparent animation over
  ~1.1s using the theme accent color, then leave the row in its normal
  unhighlighted state.
- **REQ-6**: THE SYSTEM SHALL clear the highlight target (so the class is
  not reapplied on subsequent unrelated re-renders, e.g. from selecting a
  different task) once the flash has played, mirroring the
  `_shouldFocusInlineEdit`-style one-shot flag pattern at
  `TaskItemBase.cs:323-330`.
- **REQ-7**: WHEN the user's OS/browser reports
  `prefers-reduced-motion: reduce`, THE SYSTEM SHALL still apply and
  clear `task-row--new` per REQ-4/REQ-6, but SHALL suppress the animated
  transition (no visible flash).

### Scrolling

- **REQ-8**: WHEN a row is rendered with `task-row--new` and that row's
  bounding box is not fully within the scrollable task list container's
  visible viewport, THE SYSTEM SHALL scroll the row into view with smooth
  behavior before or concurrent with the flash.
- **REQ-9**: WHEN the row is already fully visible, THE SYSTEM SHALL NOT
  perform any scroll action.

### Non-goals

- **REQ-10**: THE SYSTEM SHALL NOT apply the highlight to tasks created
  by Google Tasks sync, import, restore-from-trash, or duplication flows
  — only the two explicit add entry points in D1.
- **REQ-11**: THE SYSTEM SHALL NOT persist highlight state across a page
  reload or navigation away and back — it is a transient, in-session
  render cue only.

## Design Note (non-binding)

- `ITaskService.AddSubtaskAsync` signature changes from `Task
  AddSubtaskAsync(string name, Guid parentTaskId)` to `Task<Guid?>
  AddSubtaskAsync(string name, Guid parentTaskId)`
  (`ITaskService.cs:39`, `TaskService.cs:213-252` — return `subtask.Id`
  at the success path, `null` at both early returns).
- `IndexBase` gains a `Guid? _highlightTaskId` field (or similar), set in
  `HandleTaskAdd` (from `TaskService.CurrentTaskId`) and `HandleAddSubtask`
  (from the new return value), passed down to `TaskList` as a new
  `[Parameter] Guid? HighlightTaskId`.
- `TaskList.razor.cs` passes `HighlightTaskId` through to each
  `TaskItemComponent` as a new `[Parameter] bool IsNewlyAdded` (computed
  as `HighlightTaskId == node.Task.Id`), consistent with how `IsSelected`
  is already threaded per-row.
- `TaskItemBase.OnAfterRenderAsync` triggers the scroll interop call and a
  one-shot clear-the-flag callback (`OnHighlightConsumed` or similar
  `EventCallback`) back up to `IndexBase` to null out `_highlightTaskId`,
  following the same up-then-down flow used elsewhere in this component
  tree (no direct parent-mutation from the child).
- New JS interop module `wwwroot/js/taskScrollInterop.js`:
  `window.taskScrollInterop = { scrollIntoViewIfNeeded(element) {
  element.scrollIntoView({ behavior: 'smooth', block: 'nearest' }); } }`,
  registered in the script includes alongside existing interop modules,
  invoked via `IJSRuntime.InvokeVoidAsync("taskScrollInterop.scrollIntoViewIfNeeded",
  elementReference)` from `TaskItemComponent.razor.cs` using an
  `ElementReference` on the row (mirrors `_inlineEditInput` pattern).
  Visibility/"is it already in view" check can be done JS-side (compare
  `getBoundingClientRect()` against the scroll container) or left to
  `scrollIntoView`'s native no-op-if-visible behavior — implementer's
  call at design/TDS stage, REQ-9 just requires no jank when already
  visible.
- New CSS: `@keyframes task-flash` background-color animation (accent →
  transparent) on `.task-row--new`, ~1.1s, plus a
  `@media (prefers-reduced-motion: reduce) { .task-row--new { animation:
  none; } }` override, in `app.css` near the existing keyframes for
  convention consistency, or in a component-scoped `.razor.css` if the
  project prefers scoped styles for new component-local rules — confirm
  at TDS.

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/Services/ITaskService.cs` | `AddSubtaskAsync` signature: `Task` → `Task<Guid?>` |
| `src/Pomodoro.Web/Services/TaskService.cs` | `AddSubtaskAsync` returns `subtask.Id` / `null` |
| `src/Pomodoro.Web/Pages/Index.razor.Tasks.cs` | `HandleTaskAdd`/`HandleAddSubtask` set highlight target; new handler to clear it |
| `src/Pomodoro.Web/Components/Tasks/TaskList.razor(.cs)` | New `HighlightTaskId` parameter, threaded to `TaskItemComponent` |
| `src/Pomodoro.Web/Components/Tasks/TaskItemComponent.razor(.cs)` | New `IsNewlyAdded` parameter, `task-row--new` class, `IJSRuntime` injection, scroll-on-render, one-shot clear callback |
| `src/Pomodoro.Web/wwwroot/js/taskScrollInterop.js` | New file — scroll-into-view interop |
| `src/Pomodoro.Web/wwwroot/index.html` (or `_Host`/`App.razor`, wherever other interop scripts are registered) | Register new script |
| `src/Pomodoro.Web/wwwroot/css/app.css` (or new scoped css) | `task-flash` keyframes, `task-row--new` class, `prefers-reduced-motion` override |
| `tests/Pomodoro.Web.Tests/Services/TaskServiceTests.cs` | Update existing `AddSubtaskAsync` tests for new return type; add null-return cases |
| `tests/Pomodoro.Web.Tests/Components/Tasks/TaskListTests.cs` / `TaskItemComponentTests.cs` | New coverage per scenario table |
| `tests/e2e/` | Optional E2E: add task, assert flash class appears then clears; add off-screen subtask, assert scroll |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Add root task → `CurrentTaskId` becomes the highlight target passed to `TaskList` | `IndexTasksTests` or equivalent |
| REQ-2 | Add subtask with valid name/parent → returned `Guid` becomes highlight target | `TaskServiceTests`, `IndexTasksTests` |
| REQ-3 | `AddSubtaskAsync` with blank/too-long name → returns `null`, no highlight set, no exception | `TaskServiceTests` |
| REQ-3 | `AddSubtaskAsync` with non-existent `parentTaskId` → returns `null` | `TaskServiceTests` |
| REQ-4 | `TaskList` renders node matching `HighlightTaskId` → `TaskItemComponent` receives `IsNewlyAdded = true` | `TaskListTests` |
| REQ-4 | Non-matching nodes render `IsNewlyAdded = false` | `TaskListTests` |
| REQ-5/REQ-6 | Row with `IsNewlyAdded = true` applies `task-row--new`, then clears after one-shot render pass | `TaskItemComponentTests` |
| REQ-6 | Selecting a different task after add does not reapply `task-row--new` to the old row | `TaskItemComponentTests` |
| REQ-7 | Reduced-motion CSS override present (`prefers-reduced-motion: reduce` disables animation) | CSS/manual or Playwright `emulateMedia` |
| REQ-8 | Off-screen new row triggers `scrollIntoViewIfNeeded` interop call | `TaskItemComponentTests` (mock `IJSRuntime`) |
| REQ-9 | On-screen new row: interop call is a no-op / not disruptive (exact assertion per TDS) | `TaskItemComponentTests` |
| REQ-10 | Google sync-inserted task does not receive `IsNewlyAdded` | `TaskListTests` / `IndexTasksTests` |
| REQ-2/8 | E2E: add subtask at bottom of long list → row flashes and scrolls into view | `tests/e2e/` |

## Open Questions

1. **CSS location: `app.css` vs. component-scoped `.razor.css`.** Existing
   keyframes live in `app.css`; scoped CSS isn't used elsewhere for
   animations in this codebase per the investigation. Recommend `app.css`
   for consistency with `sd-pulse`/`ls-draw-*`. Confirm at TDS.
2. **Reduced-motion audit of existing animations — explicitly out of
   scope.** `sd-pulse` and `ls-draw-*` have no reduced-motion handling
   today; this RSD only adds the guard for the new `task-flash` animation,
   per project rule against out-of-scope fixes. Recommend filing a
   separate ticket if a full audit is wanted.
3. **Highlight during active inline-edit or drag/reorder of the same
   row.** Unlikely to occur (a row can't be mid-edit at the instant it's
   created), but not explicitly excluded. Assumed non-issue; confirm.

## Sign-off

- [ ] Product owner confirms D1-D4 (scope: root+subtask only; ~1.1s
      accent flash with reduced-motion support; paired auto-scroll;
      `AddSubtaskAsync` return-type change as the signal mechanism)
- [ ] Open Question 1 (CSS location) confirmed or deferred to TDS
- [ ] Open Question 2 (no retroactive reduced-motion audit) confirmed
- [ ] Open Question 3 (concurrent edit/drag edge case) confirmed
- [ ] RSD signed off -> proceed to design/TDS
