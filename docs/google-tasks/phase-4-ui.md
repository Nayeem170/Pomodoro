# Phase 4 — UI

## Goal

Build the multi-list task panel with list tabs, per-list sync strip, extended task edit fields, and settings integration per the mock design.

## Prerequisites

- Phase 2 complete (data model, `TaskListRef`, multi-list `ITaskService`)
- Phase 1 complete (Google task data available for read-only display)

## Steps

### 1. Create `TaskListTabs` component

**File:** `Components/Tasks/TaskListTabs.razor` (new)

Renders tabs: `Pomodoro` (coral) | visible Google lists (list color) | `Schedule` (yellow).

- Tabs show: color dot, list name, task count badge
- Active tab: highlighted with list color
- Inject `ITaskService` for `TaskLists`, `CurrentListId`, `SelectListAsync`
- On tab click: call `SelectListAsync(listId)`, update `AppState.CurrentListId`
- Hide Google tabs when not connected (not due to transient network error — review round 1)
- Schedule tab is a local filtered view (review round 2) — unaffected by Google sync

### 2. Update `IndexPagePresenterService` / `IndexPageState` (review round 3 medium gap)

**File:** `Services/IndexPagePresenterService.cs`

Current state pipeline assumes one flat task list. Add:
```
class IndexPageState
{
    List<TaskItem> Tasks              // filtered to active tab
    Guid? CurrentTaskId
    string? CurrentListId             // new
    IReadOnlyList<TaskListRef> TaskLists  // new — for tab rendering
}
```

`UpdateState` must:
1. Read `CurrentListId` from `AppState`
2. Get `TaskLists` from `ITaskService`
3. Filter `Tasks` to the active list (`GetTasksForListAsync`)
4. Pass filtered tasks + list data to `IndexBase`

### 3. Update `Index.razor` / `IndexBase`

**File:** `Pages/Index.razor` + `Pages/IndexBase.cs` or `.razor.cs`

- Add `TaskListTabs` above `TaskList` component
- Pass `IndexPageState.TaskLists` and `IndexPageState.CurrentListId` to `TaskListTabs`
- Pass filtered `IndexPageState.Tasks` to `TaskList`

### 4. Per-list sync strip

**File:** `Components/Tasks/TaskListSyncStrip.razor` (new)

When on a Google list tab, show: `Google Tasks · {list name} · 2-way sync` (Phase 3) or `Google Tasks · {list name} · read-only` (first cut).

- Inject `IGoogleTasksSyncService` for sync state
- Show stale indicator when offline (review round 1)

### 5. Checkbox color

**File:** `Components/Tasks/TaskItemComponent.razor`

- Google tasks: checkbox color = list color (blue/green/etc.)
- Local Pomodoro tasks: checkbox color = coral (`var(--pomodoro-color)`)
- Pass `TaskListRef.Color` or a computed class from the parent

### 6. Split `CloudSyncSettings` into sub-components (review round 3 medium gap)

**File:** `Components/Settings/CloudSyncSettings.razor`

Currently 186 lines. Split into:
- `GoogleConnectionSettings.razor` — Connect/Sync/Disconnect + email display
- `GoogleListToggles.razor` — "Show beside Pomodoro" list toggles (color dot, name, count, on/off)

This follows the existing codebase pattern (`TimerDurationSettings`, `AutomationSettings`, `SoundNotificationSettings`).

`CloudSyncSettings.razor` becomes the container rendering both sub-components.

### 7. Extend `TaskEditPanel`

**File:** `Components/Tasks/TaskEditPanel.razor` + `.razor.cs`

Add fields (per mock):
- **Priority** dropdown: High / Medium / None (local only — not synced to Google for Phase 3)
- **Due date** picker: `<input type="date">` bound to `TaskItem.DueDate`
- **Notes** textarea: bound to `TaskItem.Notes`
- **Subtasks**: deferred to Phase 3 (flat Google tasks with `parent`+`position`)

For first cut (read-only Google): priority/due/notes fields are **disabled** for Google tasks (no writes to Google yet).

### 8. Fix `ConsentService` / notification actions for list context (review round 3 high gap)

**File:** `Services/ConsentService.cs:203` (`StartSessionAsync`)

Auto-start reuses `CurrentTaskId` — no list context. Fix:
- Carry `CurrentListId` through auto-start flow
- Switch tab to match current task's list

**File:** `Pages/Index.razor.Events.cs:102` (notification action)

Same issue — notification action uses `AppState.CurrentTaskId` directly. Fix:
- Navigate to correct tab when starting pomodoro from notification

### 9. Fix `CurrentTaskIndicator` for list-aware filtering (review round 3 low gap)

**File:** `Components/Timer/CurrentTaskIndicator.razor`

Currently receives all tasks via `Index.razor:46-48`. With filtered lists, if current task is in a different tab:
- Either receive the current `TaskItem` as a separate parameter (preferred)
- Or look up independently of filtered list

### 10. Schedule tab interaction with Google tasks (review round 3 low gap — decision needed)

Schedule shows only Pomodoro-list tasks with `ScheduledDate`/`Repeat`. Google tasks with `DueDate` do NOT appear in Schedule (to keep Schedule as a pure local planning view).

Decision: Schedule tab filters `IsLocal == true` and `IsScheduled || IsRecurring`.

## Change sites

| File | Change |
|------|--------|
| `Components/Tasks/TaskListTabs.razor` (new) | List tabs component |
| `Components/Tasks/TaskListSyncStrip.razor` (new) | Per-list sync strip |
| `Services/IndexPagePresenterService.cs` | Add list data to `IndexPageState` |
| `Services/IndexPagePresenterService.cs` (`IndexPageState`) | Add `CurrentListId`, `TaskLists` |
| `Pages/Index.razor` | Add `TaskListTabs`, pass filtered tasks |
| `Pages/IndexBase.cs` | Handle list state |
| `Components/Tasks/TaskItemComponent.razor` | List-color checkboxes |
| `Components/Settings/GoogleConnectionSettings.razor` (new) | Split from CloudSyncSettings |
| `Components/Settings/GoogleListToggles.razor` (new) | Split from CloudSyncSettings |
| `Components/Settings/CloudSyncSettings.razor` | Container for sub-components |
| `Components/Tasks/TaskEditPanel.razor` | Add priority, due date, notes |
| `Services/ConsentService.cs:203` | Add list context to auto-start |
| `Pages/Index.razor.Events.cs:102` | Add list context to notification action |
| `Components/Timer/CurrentTaskIndicator.razor` | List-aware current task display |

## Tests

- Component: `TaskListTabs` renders Pomodoro + Google list tabs; selects tab on click
- Component: `TaskListTabs` hides Google tabs when disconnected
- Component: `TaskListSyncStrip` shows list name and sync status
- Component: `TaskEditPanel` renders priority/due/notes for local tasks; disabled for Google tasks (first cut)
- Component: `GoogleConnectionSettings` shows Connect/Disconnect + email
- Component: `GoogleListToggles` shows list toggles with color dots
- E2E: connect → list tabs appear → select Google tab → tasks display

## Risks

- `CloudSyncSettings` split changes existing component structure — ensure no regression in Drive sync UI
- `CurrentTaskIndicator` may break if current task is in a different filtered list
- `ConsentService` auto-start with list context requires careful state coordination
