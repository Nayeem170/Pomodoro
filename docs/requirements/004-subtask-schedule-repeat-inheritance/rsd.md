# RSD: Subtask Schedule/Repeat Inheritance from Parent

## Status

`Signed off — 2026-08-06`

All open questions resolved. Scope narrowed during investigation:
- REQ-3 already partially implemented (`ScheduleDayRow.razor` renders immediate
  children); gap is deep nesting only
- Open Question 2 resolved: History/stats unaffected (queries Activity records,
  not TaskItem scheduling)
- REQ-6 locked to per-occurrence (extend `MaterializeSingleAsync` to clone subtree)

## Background

User request (verbatim, compressed): "task move upward only / all of inner task should
have the same schedule and repeat logic / as we can set repeat and schedule logic to the
parent ticket only."

Investigated against the codebase (`TaskItem.cs`, `TaskEditPanel.razor.cs`,
`Index.razor.cs`, `TaskItemComponent.razor.cs`). Findings:

- **"Move upward only" is accurate and not a bug** — the only reparent action wired to UI
  is subtask→root promotion (`TaskItemComponent.razor.cs:70` `CanMoveToRoot`,
  `TaskService.ReparentTaskAsync` is only ever called with `newParentId: null`). This is
  kept as-is; it's the escape hatch for giving a subtask independent scheduling again
  (promote it to root first).
- **Subtasks currently have no schedule/repeat at all**, not "their own that happens to
  match the parent." `TaskEditPanel.razor.cs:81-87` explicitly nulls `Repeat` and
  `ScheduledDate` on every subtask save. The subtask edit UI (`TaskEditPanel.razor`
  `IsSubtask` branch) never renders Repeat/Schedule controls in the first place.
- **Subtasks are excluded from the schedule view entirely** — `Index.razor.cs:368`:
  `AppState.Tasks.Where(t => !t.IsDeleted && !t.IsSubtask)` filters subtasks out of the
  candidate list *before* `OccursOn` ever runs. A subtask under a recurring parent
  currently never shows up on any scheduled day.
- **Only root tasks can hold a rule** — `IsSubtask => ParentTaskId.HasValue || ...`
  (`TaskItem.cs:102`) is true for every non-root task regardless of depth. Since subtasks
  can nest up to `Constants.Session.MaxSubtaskDepth = 4` levels, and only a true root
  (`ParentTaskId == null`) can ever have a `Repeat`/`ScheduledDate`, resolving "the
  governing schedule" for a deeply-nested subtask means walking to the root ancestor,
  not just the immediate parent (which may itself be a subtask with nothing set).

Net: this isn't "fix a broken inheritance" — it's "add inheritance that doesn't exist
yet," because subtasks are currently invisible to the whole schedule/repeat system.

## Requirements (EARS)

- **REQ-1**: THE SYSTEM SHALL resolve a subtask's effective `Repeat`/`ScheduledDate` by
  walking its `ParentTaskId` chain to the root ancestor (`ParentTaskId == null`) and
  using that root's `Repeat`/`ScheduledDate`, regardless of nesting depth (up to
  `MaxSubtaskDepth`).
- **REQ-2**: THE SYSTEM SHALL include subtasks in schedule-window generation
  (`Index.razor.cs` `BuildScheduleWindow`, currently line 368) on any date the resolved
  root ancestor's rule fires — subtasks do not get independently-evaluated `OccursOn`
  results; they only appear when their root appears.
- **REQ-3**: THE SYSTEM SHALL render subtasks nested under their root task's row in the
  schedule/agenda day view (`ScheduleAgenda.razor`/`ScheduleDayRow.razor`), matching the
  existing nested-rendering pattern already used in the flat task list
  (`TaskItemComponent.razor` `Depth`), rather than as flat sibling rows.
- **REQ-4**: THE SYSTEM SHALL continue to hide Repeat/Schedule edit controls on subtask
  rows (`TaskEditPanel.razor` `IsSubtask` branch) — no UI change needed, this already
  matches the requirement. The existing wipe-on-save (`TaskEditPanel.razor.cs:81-87`)
  becomes a defensive no-op rather than the sole enforcement mechanism, since inheritance
  is now computed at read time, not stored per-subtask.
- **REQ-5**: THE SYSTEM SHALL leave the subtask→root promotion path unchanged
  (`ReparentTaskAsync(taskId, null)`). Promoting a subtask to root is the only way to
  give it independent scheduling, consistent with "move upward only."
- **REQ-6 (decision needed — see Open Question 1)**: THE SYSTEM SHALL track subtask
  completion for a recurring root either (a) per-occurrence-date (each day's instance of
  the root gets its own fresh completion state for every subtask, mirroring however the
  root's own repeat-instance completion already works via `RepeatSeriesId`/
  `OccurrenceDate`), or (b) as a single persistent completion state shared across all
  occurrences (completing a subtask once completes it for every future occurrence too).

## Open Questions

All resolved during sign-off (2026-08-06):

1. **REQ-6 — completion semantics**: per-occurrence (option a). Extend
   `MaterializeSingleAsync` to clone the subtask tree when materializing a root
   occurrence. Each day's instance gets fresh subtask checkboxes.
2. **History/stats page**: unaffected. `History.razor` queries Activity records
   (focus sessions), not `TaskItem` scheduling fields.
3. **Root-resolution helper**: lives on `TaskService`, takes
   `(TaskItem task, IReadOnlyList<TaskItem> all)`, walks `ParentTaskId` chain to
   root. Used by `ScheduleDayRow` for descendant-tree resolution.

## Investigation findings (narrowed scope)

- **`ScheduleDayRow.razor:26-30` already renders immediate children** under each
  schedule item via `_allSubtasks.Where(s => s.ParentTaskId == taskId)`. REQ-3's
  nested rendering is already implemented for depth-1 subtasks. The actual gap is
  deep nesting (grandchildren invisible) and per-occurrence completion reset.
- **`MaterializeSingleAsync` (`TaskService.cs:296`) creates a shallow clone** of the
  root template only. Subtasks still point to the template's `Id`, not the
  materialized instance. Extending this to clone the full descendant tree gives
  each occurrence independent subtask state.

## Sign-off

- [x] Product owner (Nayeem) approves REQ-1/2/3/4/5 as scoped
- [x] Product owner decides REQ-6: per-occurrence (option a)
- [x] History/stats page checked for Open Question 2 impact (unaffected)
- [x] RSD signed off -> proceed to implementation
