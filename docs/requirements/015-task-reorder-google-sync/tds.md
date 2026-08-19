# TDS: Manual task ordering (Phase 2 - Google-synced groups)

## Status

`Reviewed - accepted 2026-08-20` (design review, two iterations).
Issue: #155. Implements RSD 015 (accepted, operator sign-off
2026-08-18).

Anchors are file:line at develop `fe92cfd` and move during
implementation; decisions, not line numbers, are binding.

## Verified current state

| Claim (from RSD) | Anchor | Confirmed |
|---|---|---|
| `moveTask` JS interop lacks `previous` | googleTasks.js:78-93 | yes |
| `MoveTaskAsync(listId, taskId, parentTaskId, targetListId)` | IGoogleTasksService.cs:11, GoogleTasksService.cs:117-127 | yes |
| Reorder rejects any group with a Google member | TaskService.cs:501 (`group.Any(t => t.IsGoogleTask)`) | yes |
| UI gate mirrors the rejection | TaskList.razor.cs:140 (`group.All(t => !t.IsGoogleTask)`) | yes |
| BuildTree orders Google children by GooglePosition ordinal | TaskList.razor.cs:221-226, 234-236, 254-257 | yes |
| Pull never writes SortOrder (update path) | TaskService.cs:1197-1226 (absent from field list) | yes |
| Pull insert defaults SortOrder 0 | TaskService.cs:1394-1414 (`MapGoogleTaskToTaskItem`), insert at 1230-1232 | yes |
| Pull resolves local ParentTaskId for Google children | TaskService.cs:1416-1442 (`ResolveGoogleParentIdsAsync`) | yes |
| IsReorderableFor rebuilds lookups per row (O(n^2) per render) | TaskList.razor.cs:139 -> TaskGrouping.cs:27-29 (`BuildLookups` per call) | yes |
| Dirty machinery: push wrapper clears IsLocalDirty on success, sets it on failure; pull skips dirty rows from overwrite | TaskService.cs:1652-1696 (`PushGooglePatchAsync`), 1161-1166 | yes |
| Pull clears IsLocalDirty when content matches (name/status/notes/due only, no position term) | TaskService.cs:1168-1193 | yes |
| Move/insert responses carry Position; recorded on callers | TaskService.cs:277 (move), 243 (insert) | yes |
| `SortGap = 1000` | Constants.Session.cs:77 | yes |

## Design decisions

### D1 - JS interop: `previous` parameter

`googleTasks.js` `moveTask` gains a sixth parameter `previousTaskId`.
It appends `previous=<id>` to the move query exactly like the existing
`parent` param (falsy -> omitted, googleTasks.js:82). Omitted `previous`
is Google's "first position" semantics; there is no explicit null
encoding on the wire. `GoogleTasksService.MoveTaskAsync` (117-127)
appends the matching argument to the `InvokeAsync` positional call.

### D2 - Service signature widening

`Task => Task<GoogleTask?> MoveTaskAsync(string listId, string taskId,
string? parentTaskId = null, string? targetListId = null,
string? previousTaskId = null)` on both `IGoogleTasksService` and
`GoogleTasksService`. Trailing optional parameter: the three existing
call sites (TaskService.cs:272, 418, 425) compile unchanged and
behave identically. `ExecuteWithRetryAsync` (existing retry wrapper)
retains transport-level retries.

### D3 - Unlock reorder for Google groups

- TaskService.cs:501 drops `group.Any(t => t.IsGoogleTask)`, keeping
  `group.Count < 2`.
- TaskList.razor.cs:140 `IsReorderableFor` drops
  `group.All(t => !t.IsGoogleTask)`, keeping `group.Count > 1`.

The sibling-group predicate itself (TaskGrouping.GetSiblingGroup) is
unchanged in this decision; D7 collapses its dual-scheme keying.

### D4 - Push after local persist

`ReorderTaskAsync` keeps writing local `SortOrder` first
(PersistSortOrderAsync, 557-562), then pushes to Google when the
dragged task `IsGoogleTask`:

- `previous` = the Google task id of the sibling that now precedes the
  dragged task in the persisted final order; null (omitted) when the
  dragged task is first. This is an explicit branch, not a fallthrough.
- Only the dragged task is pushed. The renumber-all path (529-541,
  gap exhaustion) rewrites local SortOrder for the whole group but
  preserves relative order, so one move reproduces it on Google.
- The push runs through a new `PushGoogleMoveAsync` wrapper that
  mirrors `PushGooglePatchAsync` (1652-1696) exactly: on success it
  records `GooglePosition` from `moved.Position` plus the returned
  ETag and clears `IsLocalDirty` (1663-1673 pattern); on non-auth
  failure it logs a warning and explicitly sets `IsLocalDirty = true`
  (1684-1693 pattern). `SaveTaskAsync` (1354-1357) never touches the
  flag - the flag lifecycle belongs to the push wrapper.
- Push failure (offline, HTTP error): local SortOrder stays written,
  IsLocalDirty is set by the wrapper's catch, GooglePosition is not
  updated, and `ReorderTaskAsync` still returns true (the local
  reorder succeeded; the push is retried per D5).

Error surfacing: Google Tasks push failures today are log-only
(1684-1686); the visible sync error surfaces are the Index task-op
banner (Index.razor.Tasks.cs:33-41) and the Drive-sync toasts
(CloudSyncSettings.razor:146-162, Drive-only). The reorder entry point
adds the Index banner surface for a failed move push, following the
existing task-op catch pattern at Index.razor.Tasks.cs:41.

### D5 - Retry on next sync (no model change)

Durable retry reuses existing persisted flags; no new TaskItem field.

Enabling change first: the pull reconciliation's match predicate
(`localMatchesRemote`, 1168-1172) compares content only (name, status,
notes, due). A position-only reorder matches content, so
reconciliation would clear `IsLocalDirty` at 1182 and defeat retry.
For Google tasks the predicate additionally requires that the local
SortOrder rank agrees with the just-received position rank within the
sibling group; on rank mismatch the row is treated as not matching
(skipped, flag retained) - exactly the existing dirty-skip behavior.

Then, at the end of a successful pull cycle: for each sibling group
containing Google tasks with `IsLocalDirty` set, compare the ordinal
rank implied by local `SortOrder` against the rank implied by the
just-received response positions (not the stale local GooglePosition
artifact). On mismatch, re-push the local order for that group (one
`MoveTaskAsync` per displaced task, via the D4 wrapper) and, on
success, record the returned positions and clear `IsLocalDirty` for
the pushed members.

Sequencing: within the post-pull phase, D5's comparison runs before
D9's migration, so a divergent dirty group is re-pushed against live
Google positions rather than partially migrated underneath.

Why this shape: `IsLocalDirty` already persists in IndexedDB (survives
reload), pull already skips dirty rows from overwrite (1161-1166), and
a failed push leaves exactly the state (dirty + stale GooglePosition)
that the comparison detects. Convergence between two devices remains
last-push-wins per the RSD; no merge UI.

### D6 - Pull insert-path SortOrder derivation

New imports (1228-1232) derive SortOrder at insert: group the pulled
tasks of one list by `Parent` id, then within each group sort by the
`GooglePosition` value with `StringComparer.Ordinal` (the same
comparator the display path uses today at TaskList.razor.cs:225) and
assign `(rank + 1) * Constants.Tasks.SortGap` (1000, 2000, ...).

Position is Google's authoritative order key; the raw response
sequence is not relied on for ordering.

Update path (1197-1226) continues to leave SortOrder untouched;
`GooglePosition` is recorded as the sync artifact per the RSD.

Edge - a mixed group where locals already carry SortOrder: derived
values start at SortGap and may interleave with existing local values;
display falls to the SortOrder comparator, which is total (CreatedAt
tiebreak), so no group is ever unorderable. Renumber-on-next-reorder
(505-511, 529-541) normalizes gaps.

### D7 - BuildTree collapses to one comparator

`childrenByGoogleParent` (TaskList.razor.cs:221-226) is deleted.
One dictionary keyed by `ParentTaskId`, ordered
`SortOrder asc, CreatedAt asc` (children oldest-first) - identical to
the surviving local dictionary at 217-220. Roots already use
`SortOrder asc, CreatedAt desc` (240-243) and stay.

Feasible because `ResolveGoogleParentIdsAsync` (1416-1442) runs at the
end of every pull and stamps the resolved local id into `ParentTaskId`
for every Google child. `ChildCountFor` (229-238) and `Walk` (251-257)
drop their Google-dictionary branches. `HasKnownParent`
(TaskGrouping.cs:23-25) keeps consulting both parent schemes for
orphan detection (a Google child whose parent is not yet imported has
only `GoogleParentTaskId` until the parent arrives).

`GetOrderedSiblingGroup` (TaskGrouping.cs:40-54) collapses the same
way: its Google-parent branch (51-53, ordered by GooglePosition
ordinal) is replaced by `OrderChildrenForDisplay` (SortOrder asc,
CreatedAt asc), matching the local-parent branch at 49. After D3
unlocks Google groups, `ReorderTaskAsync` (503) calls this method, so
the single comparator must hold here too. `GetSiblingGroup` (27-37)
keeps its dual-scheme membership keying for orphan detection.

### D8 - Lookup hoist (per-render, not per-row)

`TaskGrouping.BuildLookups` runs once per render pass:

- `BuildTree` already calls it once (216).
- `IsReorderableFor` (137-140) stops calling `GetSiblingGroup` (which
  rebuilds lookups per call, TaskGrouping.cs:27-29) and consumes a
  cached `Lookups` built alongside `AllNodes` in the same render pass.

This removes the O(n^2) per-render scan noted at phase 1 acceptance.

### D9 - Migration of existing Google rows

Existing local Google rows carry SortOrder 0; derive-on-insert (D6)
only covers new TaskKeys. After D7 removes the GooglePosition ordering
path, those groups would fall to the CreatedAt tiebreak, which does
not reproduce Google's order.

Migration, in the pull cycle after D5's comparison step: for each
Google sibling group where every member has `SortOrder == 0`, sort the
group by `GooglePosition` ordinal (same comparator as D6) and assign
SortOrder `(rank + 1) * SortGap` and persist. Idempotent (a migrated
group is no longer all-zero), never touches user-reordered groups
(non-zero), and converges fresh devices via D6.

## Failure modes

| Failure | Behavior |
|---|---|
| Move push offline/HTTP error | Local order kept; wrapper sets IsLocalDirty; logged + Index banner surfaced; retry per D5 |
| Position-only divergence pulled from Google | Extended match predicate (D5) keeps IsLocalDirty set; end-of-pull re-push or overwrite per authority rules |
| App reload between failed push and next sync | IsLocalDirty persisted; D5 detects on next pull |
| Two devices reorder same group | Last push Google accepts wins; loser converges on next pull (dirty rows are skipped, then re-pushed or overwritten per authority rules) |
| Gap exhaustion after many inserts | Existing renumber-all path (529-541) handles it |
| Parent not yet imported (transient) | HasKnownParent keeps GoogleParentTaskId awareness (D7); child renders as root until parent arrives, same as today |

## Test plan mapping

| Area | File | Covers |
|---|---|---|
| MoveTaskAsync previous param | GoogleTasksServiceTests | D1, D2 (JS arg order, null omission) |
| Reorder unlock + push | TaskServiceTests (reorder) | D3, D4 (prev id computed from final order, first-position null path, response recorded) |
| Push failure semantics | TaskServiceTests | D4 (no rollback, wrapper sets dirty, banner surfaced) |
| Reconciliation keeps dirty on position divergence | TaskServiceTests (sync/pull) | D5 (extended match predicate) |
| Pull insert derivation | TaskServiceTests (sync/pull) | D6 (position-ordinal sort, not response order) |
| Divergent-dirty re-push | TaskServiceTests (sync/pull) | D5 |
| All-zero group migration | TaskServiceTests (sync/pull) | D9 |
| BuildTree single comparator | TaskList component tests + TaskGrouping tests | D7 (mixed group ordering, GetOrderedSiblingGroup) |
| IsReorderableFor hoist | TaskList component tests | D8 (correct gates, one lookup build) |
| Drag reorder on Google group | tests/e2e/pages/drag-reorder.spec.ts (extend) | D3, D4 end to end |

## Out of scope

Unchanged from RSD 015: keyboard reorder (#154, phase 3),
drag-to-reparent, conflict-resolution UI, list-level reordering.

## DoD

1. `moveTask` JS interop accepts and forwards `previousTaskId`; null
   omits the param (wire-level).
2. `MoveTaskAsync` signature widened on interface + impl; existing
   call sites compile unchanged.
3. `ReorderTaskAsync` and `IsReorderableFor` no longer reject Google
   groups.
4. Reorder of a Google task persists SortOrder, pushes `previous`
   through the PushGoogleMoveAsync wrapper, records returned Position
   and ETag, clears IsLocalDirty on success; failure keeps local
   order, the wrapper sets IsLocalDirty, and the failure is logged and
   surfaced via the Index task-op error banner.
5. Pull assigns SortOrder on insert by position-ordinal rank (not
   response order); update path never writes SortOrder; the
   reconciliation match predicate keeps IsLocalDirty set on
   position-rank divergence.
6. End-of-pull re-pushes divergent dirty Google groups; all-zero
   Google groups migrate from GooglePosition rank once.
7. BuildTree has one child dictionary ordered by SortOrder/CreatedAt;
   GetOrderedSiblingGroup uses the same comparator; no GooglePosition
   ordering path remains in display or reorder code.
8. Sibling lookups build once per render pass; no per-row lookup
   rebuild.
9. Unit + component tests for every row of the test plan mapping land
   with the change; e2e extends drag-reorder.spec.ts with a
   Google-group reorder case.
