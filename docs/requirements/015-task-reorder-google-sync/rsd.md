# RSD: Manual task ordering (Phase 2 - Google-synced groups)

## Status

`Accepted - operator sign-off 2026-08-18`

Issue: #155. Precedes TDS; no implementation until accepted.

Operator authority decision (2026-08-16, recorded verbatim in
intent): SortOrder becomes authoritative everywhere; GooglePosition
is demoted to a sync artifact.

## Background

Phase 1 (RSD 014) rejects reorders on any group containing a Google
task: without writing Google's `previous` parameter, a locally
written order would be silently overwritten by the next pull
(skip-duplicates merge keeps local rows, but Google children render
by GooglePosition - two ordering fields over one group with no
reconciliation).

That rejection also froze the structural debt that caused phase 1's
worst review defect: an over-broad sibling-group predicate existed
because there were two parenting-and-ordering schemes
(local ParentTaskId + SortOrder vs Google GoogleParentTaskId +
GooglePosition). Keeping both schemes (option B) preserves that bug
class permanently and can never order a mixed local+Google sibling
group. Collapsing to one comparator removes the class.

## Requirements (EARS)

WHEN tasks are ordered for display THE SYSTEM SHALL order every
sibling group - local, Google-synced, or mixed - by a single
comparator: SortOrder ascending, with the legacy CreatedAt tiebreak
(children oldest-first, roots newest-first). The GooglePosition
ordering path in BuildTree is removed.

WHEN a pull imports a Google task that does not exist locally (new
TaskKey) THE SYSTEM SHALL derive its SortOrder from the ordinal rank
of its position among the imported siblings of its group (first
position = 1000, then 2000, ... at Constants.Tasks.SortGap).

WHEN a pull updates an existing task (TaskKey match) THE SYSTEM SHALL
NOT overwrite the local SortOrder. GooglePosition is still recorded
on the task as a sync artifact (last-known Google-side position) and
takes no part in display ordering.

WHEN the user reorders a group containing Google tasks THE SYSTEM
SHALL persist the new SortOrder locally AND push the position change
to Google Tasks by calling move with the `previous` parameter set to
the Google task id of the sibling preceding the moved task.

WHEN a reorder moves a task to the first position of its Google group
THE SYSTEM SHALL send the move request without a `previous` id. This
is an explicit null-previous code path, not a fallthrough: Google's
semantics are "place after the given id; omitted id = first
position" (googleTasks.js moveTask, `?previous=` query param).

WHEN pushing a reorder to Google fails (offline, HTTP error) THE
SYSTEM SHALL retain the local SortOrder, SHALL keep the push pending
via the existing dirty/sync machinery for retry on the next sync, and
SHALL surface the failure through the existing sync error display.
Local order is never rolled back to match a failed push.

WHEN the task list renders THE SYSTEM SHALL build the sibling-group
lookups (TaskGrouping.BuildLookups) once per render pass instead of
once per row per call, eliminating the per-render O(n^2) scan noted
at phase 1 acceptance.

## Rationale: one ordering field

- Pull-derives-on-insert is consistent with the existing
  skip-duplicates-by-TaskKey merge: a local reorder awaiting push can
  never be clobbered by pull, and a fresh device importing a
  Google-authored order reproduces it exactly.
- GooglePosition survives as a sync artifact only: the value the last
  pull saw, used to detect and push position changes, never consulted
  for display.
- The two child dictionaries in BuildTree (childrenByLocalParent by
  SortOrder/CreatedAt, childrenByGoogleParent by GooglePosition)
  collapse into one dictionary and one comparator.

## Out of scope

- Keyboard reorder (phase 3, #154 - in flight on
  feat/task-keyboard-reorder; unaffected by this authority decision).
- Drag-to-reparent / make-child - later phase.
- Conflict resolution beyond the deterministic CreatedAt tiebreak:
  two devices reordering the same Google group concurrently converge
  to the last push that Google accepts; no merge UI.
- Google list-level (parent list) reordering.

## TDS

Written after this RSD is signed off: MoveTaskAsync signature
widening (IGoogleTasksService + GoogleTasksService + JS interop),
ImportService insert-path SortOrder derivation, ReorderTaskAsync
Google-group unlock (drop the group.Any(IsGoogleTask) rejection),
push queue integration, lookup hoist, migration of existing
GooglePosition-ordered groups on next pull (derive-on-insert covers
new devices; existing local Google rows keep SortOrder 0 +
CreatedAt tiebreak until first reorder - analyzed in TDS).
