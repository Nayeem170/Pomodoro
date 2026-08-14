# RSD: Task Ancestry Breadcrumb

## Status

`Decisions resolved — 2026-08-12 (operator)`

All four open questions resolved by the operator (2026-08-12). "Spec
first" satisfied: RSD reviewed, decisions recorded in Open Questions.
Delivery split into two PRs, both targeting `develop`:

- **PR 1 (Group A)** — NEW dual-edge path builder on `TaskService` (do
  NOT extend `GetTaskDepth` — `TaskService.cs:366-379` breaks at line
  374 and roots Google-parented tasks). Wires clock (`Index.razor:75`),
  active-task indicator (`CurrentTaskIndicator.razor:10`), sticky bar
  (`StickyTimerBar.razor:31`), timer (`TimerDisplay.razor.cs:35`). No
  model change, no migration, no export/import. Lands the format in
  front of the product owner cheaply. Ships first.
- **PR 2 (Group B)** — `TaskPath` on `ActivityRecord` +
  `TimerCompletedEventArgs`, derive-leaf, `TaskPath ?? TaskName`
  fallback, `ActivityKey`/export schema, import dedup + legacy
  back-compat.

Gate run per PR.

## Background

Today, everywhere a task is referenced by name, the app shows only the
task's own (leaf) name. When the selected task is a subtask, the user
loses its ancestry context: a session logged against task `d` (child of
`b`, child of root `a`) is displayed as just `d` in the clock, the
today's-sessions list, and history.

```
a -> b, c
b -> d
```

Product owner wants the full ancestry shown as a breadcrumb
(`a › b › d`) everywhere the task name is used as context — except
inside the task tree itself, where the hierarchy is already shown
visually by indentation.

## Current State (verified against `fix/bugfix` working tree)

> **Baseline warning.** Verified against the `fix/bugfix` working tree
> (commits `develop..fix/bugfix`, now 24 commits as of `2557c4c`),
> NOT against `develop`. Relative to `develop`:
> `GetTaskDepth` (`TaskService.cs:366-379`) and `FollowsParentRepeat`
> do NOT exist on `develop`, and `TaskService.cs` is 1321 lines on
> `develop` vs 1504 in the verified tree. PR 1 must therefore branch
> off `fix/bugfix` (stacked, merges after `fix/bugfix`) or off
> `develop` only AFTER `fix/bugfix` merges — otherwise every
> `GetTaskDepth` citation below refers to code absent from the base.
> The dual-edge resolver requirement (REQ-4) stands regardless of base:
> `IsSubtask` (`TaskItem.cs:103`) checks both `ParentTaskId` and
> `GoogleParentTaskId` on every base that has Google-parent support.

### Display sites — two distinct data sources

**Group A — rendered from the live task list** (ancestry computable at
render time by walking parent edges to root):

| Site | File:line | Current source |
|---|---|---|
| Clock/task text under timer | `Index.razor:75` via `GetCurrentTaskName()` (`Index.razor.cs:351-355`) | `AppState.Tasks.FirstOrDefault(...).Name` |
| Active-task indicator | `CurrentTaskIndicator.razor:10` | live `Tasks` list `.Name` |
| Sticky timer bar | `StickyTimerBar.razor:31` | `CurrentTaskName` string param |
| Desktop/mobile timer | `Index.razor:69,116` -> `TimerDisplay.razor.cs:35` | `CurrentTaskName` param (3rd consumer of `GetCurrentTaskName`) |
| Task tree | `TaskList.razor` / `TaskItemComponent.razor:43` | `.Name` — **out of scope, stays bare** |

Group A is answer-independent: a path helper over the live tree works
regardless of how the snapshot/history questions resolve.

**Group B — rendered from a frozen `ActivityRecord` snapshot** (name
captured at log time; the task may since be moved/renamed/deleted, so
ancestry cannot be reliably re-resolved later):

| Site | File:line | Current source |
|---|---|---|
| "Today's Sessions" log | `SessionLog.razor:23` | `session.TaskName` |
| History timeline | `ActivityTimeline.razor:47` | `activity.TaskName` |
| Weekly recent activity | `WeeklyRecentActivity.razor:31` | `activity.TaskName` |
| History activity item | `ActivityItem.razor.cs:46` -> `ActivityItemFormatter.GetTaskName` | `Activity.TaskName` |
| Time-distribution chart | `WeeklyTimeDistribution.razor.cs:86,120` | `TaskName` used as **aggregation (GroupBy) key**, not just display |

### Snapshot data model (Group B) — TaskName only, no ancestry

- `ActivityRecord` (`Models/ActivityRecord.cs:9-10`) stores `TaskName`
  + `TaskId` only.
- `TimerCompletedEventArgs` (`Models/TimerCompletedEventArgs.cs:3-10`)
  carries `TaskName` + `TaskId`.
- The logging site is `ActivityService.HandleTimerCompletedAsync`
  (`ActivityService.cs:299-313`): builds `ActivityRecord` from the
  event args, copying `TaskName = args.TaskName` (line 306).

### Path construction — existing helpers are insufficient

- `TaskItem.IsSubtask` (`TaskItem.cs:103`):
  `ParentTaskId.HasValue || !string.IsNullOrEmpty(GoogleParentTaskId)`
  — a task can be a subtask via a Google parent edge with no local
  Guid parent.
- `TaskService.GetTaskDepth` (`TaskService.cs:366-379`) walks
  `ParentTaskId` **only** and breaks at line 374 when it is null. A
  Google-parented task with no local Guid mapping is silently treated
  as a root (depth 0). The path builder **cannot reuse** this helper —
  it must resolve both edges, like `BuildTree` in
  `TaskList.razor.cs:172-230` already does (it builds a
  `googleIdToTask` map and walks both `childrenByLocalParent` and
  `childrenByGoogleParent`).
- `Constants.Tasks.MaxSubtaskDepth = 4` (`Constants.Session.cs:75`)
  bounds the tree to root + 4 levels = **at most 5 path segments**.

### Export/import schema

- `ExportKeys.cs:5-19` — `ActivityKey` includes `TaskName` as a dedup
  key (used by `ImportService.cs:130,230,282` to detect
  already-imported activities). Adding `TaskPath` is an export-schema
  change; import must stay backward-compatible with exports that carry
  only `TaskName`.

### Migration

- Existing `ActivityRecord`s in localStorage have no `TaskPath`.
  Deserialization yields null. Without a fallback, history would render
  blank on upgrade.

## Requirements (EARS)

### Display scope

- **REQ-1**: THE SYSTEM SHALL display the full ancestry path
  (root › ... › leaf) for the current/selected task at every Group A
  site: clock task text (`Index.razor:75`), active-task indicator
  (`CurrentTaskIndicator.razor:10`), sticky timer bar
  (`StickyTimerBar.razor:31`), and the timer display name
  (`TimerDisplay.razor.cs:35`).
- **REQ-2**: THE SYSTEM SHALL display the full ancestry path at every
  Group B site: today's-sessions log (`SessionLog.razor:23`), history
  timeline (`ActivityTimeline.razor:47`), weekly recent activity
  (`WeeklyRecentActivity.razor:31`), and the history activity item
  (`ActivityItem.razor.cs:46`).
- **REQ-3**: THE SYSTEM SHALL display only the task's own (leaf) name
  inside the task tree (`TaskList` / `TaskItemComponent`); ancestry is
  not shown there.

### Path construction

- **REQ-4**: WHEN computing the ancestry path, THE SYSTEM SHALL resolve
  both the local parent edge (`ParentTaskId`) and, when that is absent,
  the Google parent edge (`GoogleParentTaskId`) via a
  Google-id-to-local-id map, so a Google-parented task with no local
  Guid parent is not silently treated as a root. (Closes the gap in
  `GetTaskDepth`, `TaskService.cs:366-379`.)
- **REQ-5**: THE SYSTEM SHALL guard the path walk against cycles using
  a visited set (same pattern as `GetTaskDepth:370`).
- **REQ-6**: THE rendered path SHALL be bounded by
  `MaxSubtaskDepth + 1` segments (at most 5).

### Format and truncation

- **REQ-7**: THE SYSTEM SHALL join path segments with the single
  constant `Constants.TaskUI.PathSeparator = " › "` (right chevron
  glyph). Rationale: emoji already ship as string consts
  (`Constants.Session.cs:31-34`); `>` is typed in real task names
  (e.g. "build > test"); `/` and `-` collide with task content; a
  CSS/SVG chevron breaks `text-overflow: ellipsis`
  (`CurrentTaskIndicator.razor:10`) and cannot be snapshotted into a
  stored `TaskPath` (Group B). One const, both groups use it.
- **REQ-13**: For accessibility, THE SYSTEM SHALL pair the visual
  separator with a visually-hidden (`.sr`) span reading "under" so
  screen readers announce `a under b under d` rather than the glyph
  name.
- **REQ-8**: WHEN the rendered path exceeds the width of a narrow
  container (clock, sticky bar), THE SYSTEM SHALL drop leading
  ancestors, keeping the leaf always visible (e.g.
  `… › parent › leaf`). The full path is shown where space allows
  (history rows).

### Snapshot model (Group B)

- **REQ-9**: WHEN a session is logged (complete or partial), THE SYSTEM
  SHALL store a single `TaskPath` snapshot — the ancestry path string
  at log time — on the `ActivityRecord`, replacing the separate
  `TaskName` field. The leaf name SHALL be derived as the last segment
  wherever a leaf is needed. (`TimerCompletedEventArgs` gains
  `TaskPath`; `ActivityService.HandleTimerCompletedAsync:301-310`
  copies it.)
- **REQ-10**: WHEN an existing `ActivityRecord` without `TaskPath` is
  loaded (pre-upgrade localStorage), THE SYSTEM SHALL fall back to the
  legacy `TaskName` value so history does not go blank on upgrade. The
  fallback must apply at every read site (a single
  `EffectiveTaskPath`/derived accessor is preferred over scattering
  `?? TaskName` checks).
- **REQ-11**: THE export/import schema SHALL carry `TaskPath`, and
  SHALL import legacy exports that carry only `TaskName` by treating
  the legacy value as a leaf-only path. The `ActivityKey` dedup
  (`ExportKeys.cs:5-19`) SHALL use `TaskPath` (falling back to
  `TaskName`) so re-imports of the same activity still dedupe.

### Chart aggregation

- **REQ-12**: THE weekly time-distribution chart
  (`WeeklyTimeDistribution.razor.cs:86,120`) SHALL aggregate focus
  time by **task leaf name** (current behavior preserved), not by full
  path, unless Open Question 2 decides otherwise. Two sessions of the
  same task (identical path) still bucket together.

## Affected Files (inventory)

| File | Change |
|---|---|
| **New**: `TaskService.GetTaskPath(taskId)` (or static helper) | Dual-edge ancestry resolver (local + Google parent), cycle-guarded, returns segments root->leaf. Cannot reuse `GetTaskDepth`. |
| `src/Pomodoro.Web/Pages/Index.razor.cs:351-355` | `GetCurrentTaskName` -> `GetCurrentTaskPath` (or add path getter); feeds the 3 Group A consumers |
| `src/Pomodoro.Web/Components/Timer/CurrentTaskIndicator.razor:10` | Render path |
| `src/Pomodoro.Web/Components/Timer/StickyTimerBar.razor:31` | Render path |
| `src/Pomodoro.Web/Components/Timer/TimerDisplay.razor(.cs)` | Receive/render path param |
| `src/Pomodoro.Web/Components/Timer/SessionLog.razor:23` | Render `TaskPath` (full, space allows) |
| `src/Pomodoro.Web/Components/History/ActivityTimeline.razor:47` | Render path |
| `src/Pomodoro.Web/Components/History/WeeklyRecentActivity.razor:31` | Render path |
| `src/Pomodoro.Web/Components/History/ActivityItem.razor.cs:46` + `Services/Formatters/ActivityItemFormatter.cs` | `GetTaskName` -> leaf-derivation from path |
| `src/Pomodoro.Web/Components/History/WeeklyTimeDistribution.razor.cs:86,120` | GroupBy leaf (REQ-12) |
| `src/Pomodoro.Web/Models/ActivityRecord.cs` | Add `TaskPath`; derive leaf; keep `TaskName` for back-compat or migrate with fallback (REQ-9/10) |
| `src/Pomodoro.Web/Models/TimerCompletedEventArgs.cs` | Add `TaskPath` |
| `src/Pomodoro.Web/Services/ActivityService.cs:299-313` | Populate `TaskPath` when building record (needs task-list access to compute path at log time) |
| The site that constructs `TimerCompletedEventArgs` in `TimerService` | Populate `TaskPath` (alternative to computing in ActivityService) |
| `src/Pomodoro.Web/Services/ExportKeys.cs:5-19` | `ActivityKey.TaskPath` (+ fallback); import dedup |
| `src/Pomodoro.Web/Services/ImportService.cs:130,230,252,282` | Schema + legacy-`TaskName` back-compat |
| `src/Pomodoro.Web/Constants/*` | Separator constant + truncation rules |
| `tests/Pomodoro.Web.Tests/` | Path builder (dual-edge, Google-parent, cycle), formatter leaf derivation, migration fallback, import back-compat, chart aggregation-by-leaf. 99.5% coverage gate. |

## Testable Scenarios (Gate 1 mapping)

| REQ | Scenario | Test location |
|---|---|---|
| REQ-1 | Clock + indicator + sticky bar show `a › b › d` for a 3-deep selected subtask | bUnit / unit |
| REQ-3 | Task tree still shows bare `d` | bUnit `TaskItemComponent` |
| REQ-4 | Google-parented task (no local Guid parent) resolves full path, not silently root | `TaskServiceTests` path helper |
| REQ-4 | Path of a 5-deep task renders all 5 segments | unit |
| REQ-5 | Cycle in parent links does not loop (visited guard) | unit |
| REQ-8 | Narrow container truncates leading ancestors, leaf always visible | unit/formatter |
| REQ-9 | Logged activity stores `TaskPath` = full path at log time | `ActivityServiceTests` |
| REQ-10 | Pre-upgrade record (TaskPath null, TaskName set) renders its name, not blank | migration/unit |
| REQ-11 | Legacy export (TaskName only) imports as leaf-only path; dedup still works | `ExportServiceTests` / `ImportService` tests |
| REQ-12 | Chart buckets two same-path sessions together; two same-leaf-different-path sessions also bucket together (by leaf) | `WeeklyTimeDistribution` / `TimeDistributionChartTests` |

## Open Questions

1. **Separator — RESOLVED: ` › ` glyph.** Captured as `REQ-7` +
   `Constants.TaskUI.PathSeparator`. Accessibility handled by `REQ-13`
   (`.sr` "under" span).

2. **Chart bucket key — RESOLVED: leaf.** `WeeklyTimeDistribution`
   GroupBy stays leaf-equivalent (`REQ-12`). A display feature must not
   silently re-bucket analytics; full paths would blow up the legend.
   With `TaskPath` as the stored field, leaf = last segment, so the
   grouping key is identical to today. Same-leaf-under-different-parents
   already merges today — pre-existing, logged as a follow-up RSD, not
   fixed here.

3. **Group A fast-track — RESOLVED: yes, split.** See Status: PR 1
   (Group A) ships first, PR 2 (Group B) follows. All risk lives in B.

4. **"session" exclusion — RESOLVED: show it.** The excluded "session"
   is the session-type label (Focus / Short Break / Long Break —
   `Constants.Session.cs:19-22`, and the `Focus |` prefix at
   `StickyTimerBar.razor:31`), which never displayed a task name. The
   "Today's Sessions" panel and history sites ARE in scope (`REQ-2`).
   `SessionLog.razor:23` and `ActivityTimeline.razor:47` render the same
   `ActivityRecord` field; splitting them would be arbitrary. Confirm
   with product owner as a one-liner — does not block PR 1.

5. **Where to compute the snapshot path — DEFERRED to TDS.** Either
   `TimerService` (when building `TimerCompletedEventArgs`) or
   `ActivityService` (when building the record); depends which has
   task-list access at that point. PR 2 concern, not PR 1. Note: like
   every `GetTaskDepth` citation in this RSD, this question lands only
   after `fix/bugfix` merges — on `develop` there is no depth helper at
   all (see Current State baseline warning).

## Sign-off

- [x] Operator resolved Open Question 1 (separator: ` › `)
- [x] Operator resolved Open Question 2 (chart: leaf)
- [x] Operator resolved Open Question 3 (Group A fast-track: yes)
- [x] Operator resolved Open Question 4 ("session" = session-type label;
      SessionLog/history in scope)
- [ ] Product owner one-line confirmation of OQ-4 reading (does not
      block PR 1)
- [ ] RSD signed off -> PR 1 (Group A) proceeds
