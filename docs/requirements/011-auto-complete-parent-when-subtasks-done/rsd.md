# RSD: Auto-Complete Parent Task When All Subtasks Are Done

## Status

`Draft — 2026-08-12`

Branch: `fix/auto-complete-parent-when-subtasks-done`

No dependency on `004` or `009`. Self-contained change to
`TaskService.CompleteTaskAsync` / `UncompleteTaskAsync`.

## Background

The app already enforces one half of the parent/subtask completion
invariant: `CompleteTaskAsync` (`TaskService.cs:615`) refuses to complete a
task that still has incomplete subtasks, throwing
`Constants.Messages.CompleteSubtasksFirst`
(`Constants.Messages.cs:90`) — "Complete all subtasks before completing
this task".

The other half is missing. When the user completes the *last* incomplete
subtask, nothing propagates upward: the parent stays incomplete even though
every one of its children is done. The user must then manually complete the
parent — an action that is now guaranteed to succeed, since the guard's
precondition is already satisfied. That manual step is pure ceremony.

Product owner wants the parent to complete itself in that moment.

## Current State (verified against codebase)

- `CompleteTaskAsync` (`TaskService.cs:615-663`) operates on exactly one
  task. It: (1) collects incomplete subtask ids, (2) throws if any exist,
  (3) stamps `Repeat.LastCompletedDate` when the task is recurring, (4)
  saves + updates `_appState` with `IsCompleted = true` and `CompletedAt =
  DateTime.UtcNow`, (5) notifies, (6) pushes a Google patch
  (`status: "completed"`) or calls `MarkDirty()`.
- The subtask lookup at `TaskService.cs:620-628` matches children by
  **either** identifier:
  `t.ParentTaskId == taskId || (existingTask.GoogleTaskId is non-empty &&
  t.GoogleParentTaskId == existingTask.GoogleTaskId)`, and excludes
  `IsDeleted` and already-`IsCompleted` rows. This dual match exists because
  a Google-sourced subtask may not yet have its local `ParentTaskId`
  resolved (see `5715e6a fix: resolve GoogleParentTaskId to local
  ParentTaskId after sync`).
- `UncompleteTaskAsync` (`TaskService.cs:665-692`) has **no** guard at all
  and no upward propagation. It clears `IsCompleted`/`CompletedAt`, saves,
  notifies, pushes `status: "needsAction"` or marks dirty. It does not clear
  `Repeat.LastCompletedDate`.
- `IsDescendantOf` (`TaskService.cs:573-586`) already walks the parent chain
  upward via `_appState.FindTaskById`, with a `HashSet<Guid> seen` cycle
  break. It is the existing precedent for safe ancestor traversal.
- Both operations are called from exactly one place each:
  `HandleTaskComplete` (`Index.razor.Tasks.cs:88`) and
  `HandleTaskUncomplete` (`Index.razor.Tasks.cs:140`), both wrapped in
  `TryExecuteAsync` with a single `StateHasChanged` afterwards. No caller
  change is required — propagation belongs in the service.

## Decisions (locked)

**D1 — Reciprocity on uncomplete: YES.** Uncompleting a subtask
auto-uncompletes any completed ancestor. Without it, the app can sit in the
exact state the `CompleteSubtasksFirst` guard exists to prevent — a
completed parent holding an incomplete child — reachable in two clicks.
Completion-only would fix one direction and open a hole in the other.

**D2 — Cascade depth: full ancestor chain.** Propagation continues upward
through grandparent, great-grandparent, etc., stopping at the first ancestor
that still has an incomplete child (complete direction) or that is already
incomplete (uncomplete direction). Stopping at the immediate parent would
leave the same inconsistency one level up.

## Requirements (EARS)

### Upward completion

- **REQ-1**: WHEN a task is completed and that task has a parent, THE
  SYSTEM SHALL evaluate whether the parent has any remaining incomplete,
  non-deleted subtasks.
- **REQ-2**: WHEN the parent has no remaining incomplete, non-deleted
  subtasks and the parent is not already completed, THE SYSTEM SHALL
  complete the parent, applying the same state transition as a manual
  completion (`IsCompleted = true`, `CompletedAt = DateTime.UtcNow`,
  persistence, `NotifyStateChanged`, and Google `status: "completed"` push
  when the parent is Google-backed).
- **REQ-3**: WHEN the parent is auto-completed per REQ-2, THE SYSTEM SHALL
  repeat REQ-1/REQ-2 against that parent's own parent, continuing upward
  until an ancestor has a remaining incomplete subtask, an ancestor is
  already completed, or the root is reached.
- **REQ-4**: WHEN the parent still has at least one incomplete,
  non-deleted subtask, THE SYSTEM SHALL leave the parent unchanged and
  terminate the upward walk.
- **REQ-5**: THE SYSTEM SHALL identify subtasks using the same dual-match
  predicate already used at `TaskService.cs:620-628` (local `ParentTaskId`
  **or** `GoogleParentTaskId` against the parent's `GoogleTaskId`), so
  Google-sourced subtasks whose local parent link is not yet resolved are
  not silently ignored.

### Upward uncompletion (D1)

- **REQ-6**: WHEN a task is uncompleted and that task has a parent that is
  currently completed, THE SYSTEM SHALL uncomplete the parent, applying the
  same state transition as a manual uncompletion (`IsCompleted = false`,
  `CompletedAt = null`, persistence, `NotifyStateChanged`, and Google
  `status: "needsAction"` push when the parent is Google-backed).
- **REQ-7**: WHEN the parent is auto-uncompleted per REQ-6, THE SYSTEM
  SHALL repeat REQ-6 against that parent's own parent, continuing upward
  until an ancestor is already incomplete or the root is reached.
- **REQ-8**: THE SYSTEM SHALL NOT uncomplete sibling subtasks or any
  descendant of the uncompleted task — propagation is strictly upward.

### Recurrence

- **REQ-9**: WHEN an auto-completed ancestor is recurring
  (`IsRecurring && Repeat is { IsActive: true }`), THE SYSTEM SHALL stamp
  `Repeat.LastCompletedDate = DateTime.Now.Date` on it, identically to a
  manual completion — auto-completion is not a second-class completion and
  must not desynchronise the recurrence cursor.

### Safety

- **REQ-10**: THE SYSTEM SHALL guard the upward walk against cyclic parent
  chains using a visited-id set, matching the `seen` guard in
  `IsDescendantOf` (`TaskService.cs:573-586`), so malformed data cannot
  produce an infinite loop.
- **REQ-11**: THE SYSTEM SHALL NOT throw `CompleteSubtasksFirst` as a
  result of auto-completion — the ancestor is only completed once its
  subtask set is verified empty of incomplete members, so the guard's
  precondition holds by construction.

## Design Note (non-binding)

REQ-1 through REQ-3 fall out of a recursive tail call: `CompleteTaskAsync`
already marks the target complete in `_appState` *before* it returns, so a
recursive `CompleteTaskAsync(parentId)` appended after
`NotifyStateChanged()` re-runs the existing subtask query with correct
state and self-terminates via the existing guard-precondition check.
Same shape for `UncompleteTaskAsync`. REQ-10's visited set is the one piece
that must be threaded through (private overload taking a `HashSet<Guid>`,
public signature unchanged).

`ITaskService` signatures are unchanged. No UI change, no new constants.

## Affected Files (inventory)

| File | Change |
|---|---|
| `src/Pomodoro.Web/Services/TaskService.cs` | `CompleteTaskAsync` gains upward propagation (REQ-1..5, 9, 10); `UncompleteTaskAsync` gains upward propagation (REQ-6..8, 10) |
| `src/Pomodoro.Web/Services/ITaskService.cs` | No change — public signatures unchanged |
| `src/Pomodoro.Web/Pages/Index.razor.Tasks.cs` | No change — `HandleTaskComplete`/`HandleTaskUncomplete` already re-render after the service call |
| `tests/Pomodoro.Web.Tests/Services/TaskServiceTests.cs` | New coverage per scenario table below |
| `tests/e2e/` | Optional E2E: complete last subtask, assert parent row renders completed |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1/2 | Parent with 2 subtasks; complete both; parent becomes `IsCompleted` with non-null `CompletedAt` | `TaskServiceTests` |
| REQ-3 | 3-level chain (grandparent → parent → subtask); completing the only subtask completes both ancestors | `TaskServiceTests` |
| REQ-4 | Parent with 2 subtasks; complete only one; parent stays incomplete | `TaskServiceTests` |
| REQ-4 | Soft-deleted sibling (`IsDeleted = true`) does not block parent auto-completion | `TaskServiceTests` |
| REQ-5 | Subtask linked only via `GoogleParentTaskId` (null local `ParentTaskId`) still counts toward the parent's set | `TaskServiceTests` |
| REQ-2 | Google-backed parent auto-completion pushes a `status: "completed"` patch | `TaskServiceTests` (existing Google mock pattern) |
| REQ-6 | Parent auto-completed, then one subtask uncompleted → parent returns to incomplete, `CompletedAt` null | `TaskServiceTests` |
| REQ-7 | 3-level chain fully complete; uncomplete the leaf → both ancestors uncomplete | `TaskServiceTests` |
| REQ-8 | Uncompleting a subtask leaves completed siblings and its own children untouched | `TaskServiceTests` |
| REQ-9 | Recurring parent auto-completed → `Repeat.LastCompletedDate` set to today | `TaskServiceTests` |
| REQ-10 | Task whose `ParentTaskId` chain is cyclic terminates instead of hanging | `TaskServiceTests` |
| REQ-11 | Completing the last subtask does not surface `CompleteSubtasksFirst` | `TaskServiceTests` |
| REQ-2 | E2E: complete last subtask in UI, parent row shows completed without a page action | `tests/e2e/` |

## Open Questions

1. **`LastCompletedDate` on auto-uncomplete (pre-existing gap, likely
   out of scope).** Manual `UncompleteTaskAsync` does not clear
   `Repeat.LastCompletedDate` today. Auto-uncompleting a recurring ancestor
   inherits that behaviour: the recurrence cursor stays advanced for a task
   that is no longer complete. Recommend leaving as-is here and filing a
   separate ticket rather than silently patching (per project rule: no
   out-of-scope fixes).
2. **Selected task.** If the auto-completed ancestor is the currently
   selected timer task, nothing clears the selection — same as manual
   completion today. Assumed acceptable; confirm.
3. **Undo affordance.** Completing one subtask can now flip several rows.
   No undo toast exists for completion (only for delete). Assumed
   acceptable since REQ-6/7 make it reversible by uncompleting the same
   subtask; confirm.

## Sign-off

- [x] Product owner delegated D1 (reciprocal uncomplete) and D2 (cascade
      depth) — recorded as locked decisions above
- [x] Open Question 1 confirmed: out of scope (auto-uncomplete mirrors manual;
      no `LastCompletedDate` clearing). Separate ticket to be filed.
- [x] Open Questions 2 and 3 confirmed: acceptable, out of scope (no selection
      clearing, no undo toast — both consistent with manual completion today).
- [x] RSD signed off -> proceed to implementation (2026-08-12).

Design Note clarification (non-binding note reconciled with binding REQ-10/11):
the upward propagation uses a private overload that check-and-returns (not the
public method's check-and-throw), so auto-completion never throws
`CompleteSubtasksFirst` (REQ-11) and threads a `HashSet<Guid>` visited set
(REQ-10). Public signatures unchanged.
