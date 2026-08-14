# RSD: Manual task ordering (Phase 1 - drag reorder among siblings)

## Status

`Accepted (retroactive) - 2026-08-15`

Branch: `feat/task-drag-reorder`

Reviewed plan: `.agentic-tasks/feat-task-drag-reorder/plan.md`
(iterations 1-2, reviewer accepted).

## Background

Tasks rendered in incidental order: local children by CreatedAt,
Google children by GooglePosition, roots unordered (repository
return order). Users could not rearrange tasks. Investigation
found: IndexedDB stores whole TaskItem objects (no migration), and
default SortOrder 0 with a CreatedAt tiebreak reproduces today's
order exactly - no backfill.

## Requirements (EARS)

WHEN tasks are ordered for display THE SYSTEM SHALL sort local
children by SortOrder then CreatedAt (oldest first, legacy child
order), roots by SortOrder then CreatedAt descending (newest first,
legacy root order - E2E-enforced in task-ordering.spec.ts), and
Google children by GooglePosition unchanged.

WHEN all SortOrder values in a sibling group are 0 (never
reordered) THE SYSTEM SHALL display them in legacy order (children
oldest-first, roots newest-first), identical to pre-feature
behavior, until the first reorder in that group.

WHEN the user drags a task over the top or bottom 25% of a
reorderable sibling row THE SYSTEM SHALL show an accent line at
that edge and insert the dragged task before/after the target on
drop.

WHEN the user drags over the middle 50% of a row THE SYSTEM SHALL
perform no reorder.

WHEN the drag source or target group contains any Google task THE
SYSTEM SHALL reject the reorder: rows show the no-drop outline, no
zones render, and the service returns false without writes.
(Phase 2 will wire Google's `previous` param; until then reorders
on Google groups would be silently overwritten by the next pull -
silent data loss of intent.)

WHEN a group that has never been reordered receives its first
reorder THE SYSTEM SHALL assign SortOrder 1000, 2000, ... in the
current display order once (lazy normalize).

WHEN an insert would land between two siblings whose SortOrder gap
is less than 2 THE SYSTEM SHALL renumber the whole group at gap
1000; OTHERWISE the dragged task receives the midpoint (or
neighbor +/- 1000 at group edges).

WHEN a reorder leaves the dragged task in its current position
THE SYSTEM SHALL persist nothing.

WHEN the user is inline-editing a task name THE SYSTEM SHALL
disable dragging on that row.

## Sync and persistence contract

- SortOrder serializes with the whole TaskItem (IndexedDB put,
  Drive sync envelope, export/import). WithUpdates copies it
  explicitly; omission would silently zero it on every update.
- Pull merge is skip-duplicates by TaskKey: pull never overwrites
  existing tasks, so a local reorder cannot be clobbered by pull.
- Two devices reordering offline may produce duplicate SortOrder
  after sync; the CreatedAt tiebreak gives a deterministic stable
  order. No crash.

## Visible behavior change (accepted)

Before a group's first reorder, display order is byte-identical to
legacy behavior (verified: roots newest-first per E2E
task-ordering.spec.ts, children oldest-first per BuildTree). After a
group's first reorder the group switches to explicit SortOrder
ordering; from then on the user controls the order.

## Out of scope

- Google Tasks sibling position writes (`previous` param) - phase 2.
- Drag-to-reparent / make-child (mid-zone) / horizontal
  drag-to-depth - phase 3.
- Touch input: HTML5 DnD does not fire on touch; manual only.
