# Phase 2 — Data Model

## Goal

Extend `TaskItem` with Google fields, create `TaskListRef` for list tabs, add the sidecar IndexedDB store for pomodoro metadata, update `ITaskService` for multi-list support, and fix the timer write path.

## Prerequisites

- Phase 1 complete (API client available)
- Existing: `TaskItem.cs`, `ITaskService.cs`, `ITaskRepository.cs`, `AppState.cs`, `TaskService.cs`

## Steps

### 1. Extend `TaskItem`

**File:** `Models/TaskItem.cs`

Add nullable fields:
```
string? GoogleTaskId
string? GoogleListId
string? ETag
DateTime? UpdatedAt       // UTC — set on every Google sync pull
string? Notes
DateTime? DueDate          // date-only (Google API ignores time component)
Priority Priority          // enum: None, Med, High — local only, not synced to Google
List<Subtask>? Subtasks    // Phase 3
```

Computed properties:
```
bool IsGoogleTask => GoogleTaskId.HasValue
```

Google IDs should be marked `[JsonIgnore]` or stripped in export to prevent leaking into non-Google devices (review round 1 resolution).

### 2. Add `Priority` enum

**File:** `Models/Priority.cs` (new)
```
enum Priority { None, Med, High }
```

### 3. Add `Subtask` model

**File:** `Models/Subtask.cs` (new, deferred to Phase 3)
```
record Subtask(string Id, string Title, bool IsCompleted);
```

### 4. Add `TaskItem.WithUpdates()` helper

**File:** `Models/TaskItem.cs`

Add a `WithUpdates` method that copies all mutable fields from another instance. This replaces the 4+1 manual copy sites:

**Change sites (5 total):**
| Method | File:Line |
|--------|-----------|
| `UpdateTaskAsync` | `TaskService.cs:~108` |
| `DeleteTaskAsync` | `TaskService.cs:~141` |
| `CompleteTaskAsync` | `TaskService.cs:~179` |
| `UncompleteTaskAsync` | `TaskService.cs:~234` |
| `ImportTasksAsync` | `ImportService.cs:~184-195` (review round 3 gap) |

All 5 must route through `WithUpdates()` to prevent silently dropping new fields.

### 5. Create `TaskListRef`

**File:** `Models/TaskListRef.cs` (new)
```
record TaskListRef(string Id, string Title, string Color, int Count, bool IsVisible, bool IsLocal);
```

- `IsLocal = true` for the Pomodoro list and Schedule tab
- `Color` = `"var(--pomodoro-color)"` for Pomodoro, Google list color for Google tasks, yellow for Schedule
- `Count` = number of active tasks in the list

### 6. Create `GoogleTasksSettings`

**File:** `Models/GoogleTasksSettings.cs` (new)
```
record GoogleTasksSettings(Dictionary<string, ListSetting> Lists);

record ListSetting(bool IsVisible, string Color, DateTime? LastSync);
```

Persisted via the settings repository. Not in `SyncStateRecord` (review round 1 resolution).

### 7. Create `IPomodoroMetaRepository` + sidecar store

**File:** `Services/Repositories/IPomodoroMetaRepository.cs` (new)
```
interface IPomodoroMetaRepository
{
    Task<PomodoroMeta?> GetAsync(string googleTaskId);
    Task SaveAsync(PomodoroMeta meta);
    Task DeleteAsync(string googleTaskId);
    Task<IReadOnlyList<PomodoroMeta>> GetAllAsync();
    Task ClearAllAsync();
}
```

**File:** `Models/PomodoroMeta.cs` (new)
```
record PomodoroMeta(string GoogleTaskId, int PomodoroCount, int TotalFocusMinutes, Priority Priority);
```

**IndexedDB migration:** bump `Constants.Storage.DatabaseVersion` 1→2; add `pomo_meta` object store (keyed by `googleTaskId`, additive only — do not drop existing v1 stores).

**File:** `wwwroot/js/indexedDbInterop.js:37` — add `pomo_meta` in `onupgradeneeded`.

**File:** `Constants/Constants.Storage.cs` — bump `DatabaseVersion` to `2`.

### 8. Fix timer write path for Google tasks (review round 3 critical gap)

**File:** `Services/TaskService.cs:276-281` (`AddTimeToTaskAsync`)

Current: writes `PomodoroCount`/`TotalFocusMinutes` directly to `ITaskRepository`.

Branch needed:
```
if (task.GoogleTaskId.HasValue)
    await _pomodoroMetaRepo.SaveAsync(...);  // sidecar
else
    // existing path — write to TaskItem via ITaskRepository
```

Also affects `HandleTimerCompletedAsync` (`:320`) — the sidecar write must be handled here.

**File:** `Services/DailyStatsService.cs:54-58` (`RecordPomodoroCompletion`) — records local Guid. Low priority to fix (cosmetic, not portable for Google tasks).

### 9. Extend `ITaskService` for multi-list (review round 3 high gap)

**File:** `Services/ITaskService.cs`

Current interface is entirely single-list. Add:
```
IReadOnlyList<TaskListRef> TaskLists { get; }
TaskListRef? CurrentList { get; }
string? CurrentListId { get; }
Task<IReadOnlyList<TaskItem>> GetTasksForListAsync(string listId);
Task SelectListAsync(string listId);
Task AddTaskAsync(string name, string? listId = null);
```

### 10. Add `CurrentListId` to `AppState` and `AppStateRecord` (review round 3 high gap)

**File:** `Models/AppState.cs`
- Add `string? CurrentListId` property

**File:** `Services/TaskService.cs:436-440` (`AppStateRecord`)
- Add `string? CurrentListId` to the persistence record
- Restore on `InitializeAsync` (`:41-48`)
- Persist on tab switch

### 11. Extend `ITaskRepository` with list-based queries (review round 3 medium gap)

**File:** `Services/Repositories/ITaskRepository.cs`

Add:
```
Task<IReadOnlyList<TaskItem>> GetByGoogleListIdAsync(string listId);
Task<TaskItem?> GetByGoogleTaskIdAsync(string googleTaskId);
```

Add corresponding IndexedDB indexes on `googleListId` and `googleTaskId` in `onupgradeneeded`.

### 12. Fix `ExportService` / `ImportService` for sidecar (review round 3 critical gap)

**Export:**
- Bump `ExportData.Version` from 1 to 2
- Add `PomodoroMeta?` field to both Drive envelope and local JSON export
- The anonymous type in `ExportService.ExportToJsonAsync` (`:36-43`) should be replaced with a proper `ExportData` record class (reuse existing from `ImportService.cs:274-281` or extract)

**Import:**
- Handle version 2 data (sidecar present)
- On non-Google devices: orphan sidecar entries (no matching `GoogleTaskId` on any task) → discard silently

**Sidecar cross-device via Drive envelope (review round 2 high):**
- Include sidecar in `SyncEnvelope.Data` payload (`CloudSyncService.cs:303`)
- Bump `SyncEnvelope.Version` (`CloudSyncService.cs:469`)
- Add back-compat read in `PullAsync` (`:337`) for old versions without sidecar

## Change sites

| File | Change |
|------|--------|
| `Models/TaskItem.cs` | Add Google fields, `Priority`, `WithUpdates()`, computed `IsGoogleTask` |
| `Models/Priority.cs` (new) | Enum |
| `Models/Subtask.cs` (new) | Record (Phase 3) |
| `Models/TaskListRef.cs` (new) | Record |
| `Models/GoogleTasksSettings.cs` (new) | Record |
| `Models/PomodoroMeta.cs` (new) | Record |
| `Models/AppState.cs` | Add `CurrentListId` |
| `Services/ITaskService.cs` | Add multi-list members |
| `Services/TaskService.cs` | Fix 5 copy sites, add timer sidecar branch, add `CurrentListId` |
| `Services/ImportService.cs:184-195` | Route through `WithUpdates()` |
| `Services/ExportService.cs` | Version bump, add sidecar, extract proper export type |
| `Services/Repositories/ITaskRepository.cs` | Add list-based query methods |
| `Services/Repositories/IPomodoroMetaRepository.cs` (new) | Sidecar repo interface |
| `Services/Repositories/PomodoroMetaRepository.cs` (new) | IndexedDB implementation |
| `Constants/Constants.Storage.cs` | Bump `DatabaseVersion` to 2 |
| `wwwroot/js/indexedDbInterop.js:37` | Add `pomo_meta` store + indexes |
| `Services/ServiceRegistrationService.cs` | Register sidecar repo |
| `tests/.../Helpers/TestHelper.cs` | Add sidecar repo mock |

## Tests

- Unit: `TaskItem.WithUpdates()` correctly copies all fields (including new ones)
- Unit: `IsGoogleTask` computed property
- Unit: `AddTimeToTaskAsync` branches: Google task → sidecar write, local task → TaskItem write
- Unit: `ITaskRepository.GetByGoogleListIdAsync` returns filtered results
- Unit: `IPomodoroMetaRepository` CRUD operations
- Unit: `ExportData` version 2 includes sidecar; version 1 import still works (back-compat)
- Unit: import with orphan sidecar entries on non-Google device → silently discarded
- Unit: `AppStateRecord` persists/restores `CurrentListId`

## Risks

- `ImportService.cs:184-195` is a 5th copy site easily missed — must be included in `WithUpdates` migration
- Timer sidecar branch is the architectural crux — if missed, Google tasks accumulate pomodoro data in the wrong store
