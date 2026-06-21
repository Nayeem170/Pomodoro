# Phase 2 — Data Model

## Goal

Extend `TaskItem` with Google fields, create `TaskListRef` for list tabs, add the sidecar IndexedDB store for pomodoro metadata, update `ITaskService` for multi-list support, fix the timer write path, and implement read-only pull (GoogleTask→TaskItem map + upsert on connect/sync).

**Scope clarification:** This phase includes the pull-half of sync (read-only). Phase 3 adds push/reconcile/delete. Without pull, multi-list UI shows empty Google lists — shelves with nothing stocked.

## Prerequisites

- Phase 1 complete (API client available)

## Implementation Order

### Batch 1 — New Models (no existing code touched)

#### 1A. `Models/Priority.cs` (new)
```
enum Priority { None, Med, High }
```

#### 1B. `Models/PomodoroMeta.cs` (new)
```
record PomodoroMeta(string GoogleTaskId, int PomodoroCount, int TotalFocusMinutes, Priority Priority)
```

#### 1C. `Models/TaskListRef.cs` (new)
```
record TaskListRef(string Id, string Title, string Color, int Count, bool IsVisible, bool IsLocal)
```
- `IsLocal = true` for the Pomodoro list and Schedule tab
- `Color` = `"var(--pomodoro-color)"` for Pomodoro, Google list color for Google tasks, yellow for Schedule
- `Count` = number of active tasks in the list

#### 1D. `Models/GoogleTasksSettings.cs` (new)
```
record GoogleTasksSettings(Dictionary<string, ListSetting> Lists);

record ListSetting(bool IsVisible, string Color, DateTime? LastSync)
```
Persisted via settings repository. Not in `SyncStateRecord`.

---

### Batch 2 — Extend `TaskItem`

#### 2A. Add Google fields to `Models/TaskItem.cs`

Add nullable fields after existing fields:
```
string? GoogleTaskId
string? GoogleListId
string? ETag
DateTime? UpdatedAt       // UTC — set on every Google sync pull
string? Notes
DateTime? DueDate          // date-only (Google API ignores time component)
Priority Priority          // enum: local only, not synced to Google
```

Computed property:
```
bool IsGoogleTask => !string.IsNullOrEmpty(GoogleTaskId)
```

**No `[JsonIgnore]` on any field.** Google IDs must persist in IndexedDB (Blazor uses System.Text.Json for all IIndexedDbService calls — `[JsonIgnore]` would drop them from storage). Google fields are scrubbed in the export projection only (Batch 8B).

#### 2B. Add `TaskItem.WithUpdates()` helper

Replaces all 5 manual copy sites. Copies all mutable fields from a source `TaskItem`:

| Method | File:Line | Fields changed |
|--------|-----------|---------------|
| `UpdateTaskAsync` | `TaskService.cs:120-133` | Name |
| `DeleteTaskAsync` | `TaskService.cs:146-159` | IsDeleted, DeletedAt |
| `CompleteTaskAsync` | `TaskService.cs:208-221` | IsCompleted |
| `UncompleteTaskAsync` | `TaskService.cs:234-247` | IsCompleted |
| `ImportTasksAsync` | `ImportService.cs:184-195` | Full copy, new Id |

Pattern:
```csharp
public TaskItem WithUpdates(Action<TaskItem>? mutate = null)
{
    var copy = new TaskItem
    {
        Id = Id,
        Name = Name,
        CreatedAt = CreatedAt,
        IsCompleted = IsCompleted,
        TotalFocusMinutes = TotalFocusMinutes,
        PomodoroCount = PomodoroCount,
        LastWorkedOn = LastWorkedOn,
        IsDeleted = IsDeleted,
        DeletedAt = DeletedAt,
        Repeat = Repeat,
        ScheduledDate = ScheduledDate,
        GoogleTaskId = GoogleTaskId,
        GoogleListId = GoogleListId,
        ETag = ETag,
        UpdatedAt = UpdatedAt,
        Notes = Notes,
        DueDate = DueDate,
        Priority = Priority
    };
    mutate?.Invoke(copy);
    return copy;
}
```

Each copy site becomes:
```csharp
var taskToSave = existingTask.WithUpdates(t => t.Name = name);
```

---

### Batch 3 — IndexedDB Migration

#### 3A. Bump `Constants.Storage.DatabaseVersion` to `2` in `Constants.Storage.cs`

#### 3B. Migration block in `indexedDbInterop.js` (`onupgradeneeded`)

**Do NOT put new indexes inside the existing `if (!db.objectStoreNames.contains(tasksStore))` block.** That block only runs on fresh install — existing v1 users upgrading to v2 already have the store, so it's skipped and new indexes are never created.

After the existing per-store creation blocks, add a version-gated migration block:
```js
// V2 migration: add indexes to existing tasks store + new pomo_meta store
if (event.oldVersion < 2) {
    const tx = event.target.transaction;
    const ts = tx.objectStore(storage.tasksStore);
    if (!ts.indexNames.contains(indexes.googleListId)) {
        ts.createIndex(indexes.googleListId, indexes.googleListId, { unique: false });
    }
    if (!ts.indexNames.contains(indexes.googleTaskId)) {
        ts.createIndex(indexes.googleTaskId, indexes.googleTaskId, { unique: false });
    }
    if (!db.objectStoreNames.contains(storage.pomoMetaStore)) {
        db.createObjectStore(storage.pomoMetaStore, { keyPath: indexes.googleTaskId });
    }
}
```

For fresh installs (oldVersion === 0), both the existing creation blocks AND this migration block run. The `indexNames.contains` guard prevents duplicate index errors.

Add to `pomodoroConstants.storage` in `Constants.cs`:
- `pomoMetaStore: "pomoMeta"`

Add to `pomodoroConstants.storage.indexes`:
- `googleTaskId: "googleTaskId"`
- `googleListId: "googleListId"`

---

### Batch 4 — Sidecar Repository

#### 4A. Create `Services/Repositories/IPomodoroMetaRepository.cs` (new)
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

#### 4B. Create `Services/Repositories/PomodoroMetaRepository.cs` (new)

Uses `IIndexedDbService` with `Constants.Storage.PomoMetaStore`. Implementation follows existing `TaskRepository` pattern (Put/Get/GetAll/ClearAll).

#### 4C. Register in `ServiceRegistrationService.cs`
```
services.AddScoped<IPomodoroMetaRepository, PomodoroMetaRepository>();
```

#### 4D. Add mock to `TestHelper.cs`
```
Services.AddSingleton(Mock.Of<IPomodoroMetaRepository>());
```

---

### Batch 5 — Extend Repository Interfaces

#### 5A. Add to `ITaskRepository.cs`
```
Task<IReadOnlyList<TaskItem>> GetByGoogleListIdAsync(string listId);
Task<TaskItem?> GetByGoogleTaskIdAsync(string googleTaskId);
```

#### 5B. Implement in `TaskRepository.cs`

Uses `IIndexedDbService.GetByIndexAsync<TaskItem>(Constants.Storage.TasksStore, indexName, value)`:
- `GetByGoogleListIdAsync` → index `"googleListId"`, filter out deleted
- `GetByGoogleTaskIdAsync` → index `"googleTaskId"`

---

### Batch 6 — Extend `ITaskService` + `AppState` + `TaskService`

#### 6A. Add `CurrentListId` to `AppState.cs`

```csharp
private string? _currentListId;
public string? CurrentListId
{
    get => _currentListId;
    set
    {
        if (_currentListId != value)
        {
            _currentListId = value;
            NotifyStateChanged();
        }
    }
}
```

#### 6B. Add to `ITaskService.cs`
```
IReadOnlyList<TaskListRef> TaskLists { get; }
TaskListRef? CurrentList { get; }
string? CurrentListId { get; }
Task<IReadOnlyList<TaskItem>> GetTasksForListAsync(string listId);
Task SelectListAsync(string listId);
Task AddTaskAsync(string name, string? listId = null);
```

#### 6C. Add `CurrentListId` to `AppStateRecord` in `TaskService.cs:436-440`
```csharp
public string? CurrentListId { get; set; }
```

#### 6D. Restore `CurrentListId` in `TaskService.InitializeAsync()`

After `CurrentTaskId` restore block (~line 48), add:
```csharp
if (!string.IsNullOrEmpty(appState.CurrentListId))
{
    _appState.CurrentListId = appState.CurrentListId;
}
```

#### 6E. Persist `CurrentListId` in `SaveCurrentTaskIdAsync()`

Update the method to also save `CurrentListId` (method name kept — callers remain unchanged):
```csharp
var appStateRecord = new AppStateRecord
{
    Id = Constants.Storage.DefaultSettingsId,
    CurrentTaskId = _appState.CurrentTaskId,
    CurrentListId = _appState.CurrentListId
};
```

#### 6F. Implement multi-list members in `TaskService`

- `TaskLists` — builds list refs from **list metadata** (not task grouping):
  - Always include local Pomodoro list (`IsLocal=true`, title "Tasks", count from `Tasks.Where(!IsGoogleTask && !IsScheduled)`)
  - Always include Schedule tab (`IsLocal=true`, title "Schedule", count from `Tasks.Where(IsScheduled)`)
  - For each known Google list (from `_cachedGoogleLists` snapshot + `GoogleTasksSettings`): title, color, visibility from metadata; count from `Tasks.Where(GoogleListId == id)`. Empty Google lists still appear (title/color from metadata, count=0). `_cachedGoogleLists` is populated by `RefreshGoogleListsAsync()` (Batch 9C).
  - Requires `IGoogleTasksService` injected (already registered).
- `CurrentList` — returns the `TaskListRef` matching `CurrentListId`
- `CurrentListId` — delegates to `_appState.CurrentListId`
- `GetTasksForListAsync(listId)` — filters `_appState.Tasks` by list:
  - Local Pomodoro list: `!IsGoogleTask && !IsScheduled`
  - Schedule tab: `IsScheduled`
  - Google list: `GoogleListId == listId`
  - **Hydrate sidecar** (join-on-read): for Google-task results, fetch sidecar via `GetAllAsync()` once → build `Dictionary<string, PomodoroMeta>` keyed by `GoogleTaskId` → for each Google task, clone via `task.WithUpdates(c => { c.PomodoroCount = meta.PomodoroCount; ... })`. **Must clone, never mutate in-place.** The `_appState.Tasks` list holds the original references — mutating them would leak sidecar values into `SaveTaskAsync` calls (complete/rename would persist the overlay into the tasks store). The clones are display-only; the originals in `_appState.Tasks` keep zeroed pomodoro fields for Google tasks.
- `SelectListAsync(listId)` — sets `_appState.CurrentListId`, persists via `SaveCurrentTaskIdAsync()`, notifies
- `AddTaskAsync(name, listId)`:
  - **Phase 2 (read-only):** if `listId` is a Google list, throw `NotSupportedException` or silently redirect to local Pomodoro list. Creating tasks on Google lists requires `insertTask` (Phase 3). No orphans.
  - Local Pomodoro list: existing behavior unchanged

---

### Batch 7 — Fix Timer Write Path

#### 7A. Fix `TaskService.AddTimeToTaskAsync` (lines 268-292)

Branch on `task.IsGoogleTask`:
```csharp
public async Task AddTimeToTaskAsync(Guid taskId, int minutes)
{
    if (minutes <= 0) return;

    var task = _appState.FindTaskById(taskId);
    if (task == null) return;

    if (task.IsGoogleTask && !string.IsNullOrEmpty(task.GoogleTaskId))
    {
        var meta = await _pomodoroMetaRepo.GetAsync(task.GoogleTaskId);
        meta = new PomodoroMeta(
            task.GoogleTaskId,
            meta?.PomodoroCount + 1 ?? 1,
            meta?.TotalFocusMinutes + minutes ?? minutes,
            meta?.Priority ?? Priority.None);
        await _pomodoroMetaRepo.SaveAsync(meta);
        _appState.UpdateTask(taskId, t => { t.LastWorkedOn = DateTime.UtcNow; });
    }
    else
    {
        var updated = _appState.UpdateTask(taskId, t =>
        {
            t.TotalFocusMinutes += minutes;
            t.PomodoroCount++;
            t.LastWorkedOn = DateTime.UtcNow;
        });

        if (!updated) return;

        await SaveTaskAsync(task);
    }

    NotifyStateChanged();
}
```

Inject `IPomodoroMetaRepository` into `TaskService` constructor.

**Priority dual-home note:** `TaskItem.Priority` is used for local tasks; `PomodoroMeta.Priority` is the source of truth for Google tasks. On join-on-read (6F), sidecar `Priority` overwrites `TaskItem.Priority` for Google tasks. On timer write (7A), sidecar `Priority` is preserved. Drift is impossible — `TaskItem.Priority` for Google tasks is always display-only and overwritten on next read.

#### 7B. `HandleTimerCompletedAsync` (line 320)

No changes needed — it already delegates to `AddTimeToTaskAsync`.

---

### Batch 8 — Fix Export/Import

#### 8A. Extract `ExportData` to shared model

Move `ExportData` from `ImportService.cs:274-281` to `Models/ExportData.cs` (public).

Add `PomodoroMeta` field:
```csharp
public class ExportData
{
    public int Version { get; set; }
    public DateTime ExportDate { get; set; }
    public TimerSettings? Settings { get; set; }
    public List<ActivityRecord>? Activities { get; set; }
    public List<TaskItem>? Tasks { get; set; }
    public List<PomodoroMeta>? PomodoroMeta { get; set; }  // v2+
}
```

#### 8B. Update `ExportService.ExportToJsonAsync` and `ExportToJsonStringAsync`

- Inject `IPomodoroMetaRepository`
- Bump `Version` to `2`
- **Exclude Google tasks from export** — Decision 1: Tasks API owns Google task content; Drive envelope owns settings/history/stats/sidecar. Exporting Google tasks (even without IDs) would duplicate them as orphan locals on device 2. Filter: `Tasks.Where(t => !t.IsGoogleTask)`
- **Scrub Google fields** from exported local tasks (defensive — should be none after filter, but prevents leakage):
  ```csharp
  Tasks = tasks.Where(t => !t.IsGoogleTask).Select(t => t.WithUpdates(c =>
  {
      c.GoogleTaskId = null;
      c.GoogleListId = null;
      c.ETag = null;
      c.UpdatedAt = null;
  })).ToList()
  ```
- Add `PomodoroMeta = (await _pomodoroMetaRepo.GetAllAsync()).ToList()`
- Replace anonymous type with `ExportData`

#### 8C. Update `ImportService.ImportTasksAsync`

- Route through `WithUpdates()`:
  ```csharp
  var importedTask = task.WithUpdates(t =>
  {
      t.Id = newId;
  });
  ```
- After task import, if `PomodoroMeta` is present in import data (version 2+):
  - For each meta entry, if the corresponding `GoogleTaskId` exists on any imported task → save to sidecar
  - Orphan entries (no matching task) → discard silently
- Handle back-compat: version 1 data has no `PomodoroMeta` → skip
- `ExportData` is now public (was private) — update `ParseJsonDataAsync` reference

#### 8D. Update `CloudSyncService.PullAsync` and `PushAsync`

The `ExportService` change automatically flows through — `PushAsync` calls `ExportToJsonStringAsync` which now includes sidecar data (but excludes Google tasks). `PullAsync` calls `ImportFromStringAsync` which now handles sidecar import. The `SyncEnvelope` version (`Constants.Sync.SyncVersion`) does NOT need a bump because the sidecar data is embedded in the export JSON, not the envelope itself.

---

### Batch 9 — Read-Only Pull (GoogleTask → TaskItem map + upsert)

This is the pull-half of sync. Without it, multi-list UI shows empty Google lists.

#### 9A. Add `GoogleTasksSettings` to `ExportData` (8A amend)

Add to `Models/ExportData.cs`:
```csharp
public GoogleTasksSettings? GoogleTasksSettings { get; set; }  // v2+
```

Update `ExportService` (8B amend): include settings repository read.
Update `ImportService` (8C amend): restore on import. Device-local list prefs now cross devices via Drive.

#### 9B. Add `_cachedGoogleLists` to `TaskService`

```csharp
private List<GoogleTaskList> _cachedGoogleLists = new();
```

Populated by `RefreshGoogleListsAsync()` (9C). Used by `TaskLists` property (6F) to build refs from metadata (title, color, visibility) — not from task grouping.

#### 9C. Add `ITaskService.RefreshGoogleListsAsync()`

New method on `ITaskService` + `TaskService`:
```csharp
Task RefreshGoogleListsAsync();
```

Implementation:
1. If `!_googleTasksService.IsConnected()` → clear `_cachedGoogleLists`, return.
2. Call `await _googleTasksService.GetTaskListsAsync()` → update `_cachedGoogleLists`.
3. For each visible Google list, call `await _googleTasksService.GetTasksAsync(listId)` → map to `TaskItem` via `MapGoogleTaskToTaskItem()` (9D) → upsert into `_taskRepository`. **Per-list try/catch:** if `GetTasksAsync` throws for one list, log + skip that list. Do NOT proceed to 9E soft-delete for that list (a failed fetch is not "all tasks deleted"). Other lists continue unaffected.
4. Merge upserted Google tasks into `_appState.Tasks` (replace existing by `GoogleTaskId`, add new, remove deleted).
5. Notify state changed.

**Refresh triggers:**
- `TaskService.InitializeAsync()` — if Google connected, pull on startup.
- `CloudSyncService.ConnectAsync()` — after initial sync, pull Google tasks.
- Manual "Refresh" button in UI (Phase 4) — calls `RefreshGoogleListsAsync()`.

#### 9D. `GoogleTask` → `TaskItem` mapping

Static helper or method on `TaskService`:
```csharp
private static TaskItem MapGoogleTaskToTaskItem(GoogleTask g, string listId)
{
    return new TaskItem
    {
        Id = Guid.NewGuid(),  // local surrogate key — not the Google ID
        Name = g.Title,
        GoogleTaskId = g.Id,
        GoogleListId = listId,
        ETag = g.ETag,
        UpdatedAt = ParseGoogleDateTime(g.Updated),
        Notes = g.Notes,
        DueDate = ParseGoogleDate(g.Due),
        IsCompleted = g.Status == "completed",
        CreatedAt = DateTime.UtcNow,  // Google doesn't expose created_at
        Priority = Priority.None,     // sidecar overrides on join-on-read
        TotalFocusMinutes = 0,        // sidecar overrides on join-on-read
        PomodoroCount = 0             // sidecar overrides on join-on-read
    };
}
```

Date parsing helpers:
- `ParseGoogleDateTime(string?)` — Google returns ISO 8601 (`2025-01-15T10:00:00.000Z`) → `DateTime?`. Handle null/empty.
- `ParseGoogleDate(string?)` — Google returns date-only for due (`2025-01-15`) → `DateTime?` at midnight UTC. Handle null/empty.

#### 9E. Upsert logic

For each mapped `TaskItem` from a Google list:
1. Query existing local task by `GoogleTaskId`: `await _taskRepository.GetByGoogleTaskIdAsync(googleTaskId)`
2. If found → `existing.WithUpdates(c => { update Name, Status, Notes, DueDate, ETag, UpdatedAt from Google })` → save
3. If not found → new `TaskItem` (from `MapGoogleTaskToTaskItem`) → save + insert into `_appState.Tasks`
4. After processing all tasks in a list: find locally-stored tasks with that `GoogleListId` that were NOT in the API response → soft-delete them (`IsDeleted = true, DeletedAt = DateTime.UtcNow`). Google-task deletion detection (showDeleted=false in Phase 1 means deleted tasks are absent from API response).

**Existing `_appState.Tasks` handling:** Google tasks upserted into `_taskRepository` must also update `_appState.Tasks`. After per-list upsert, reload the full task list from repository (`ReloadAsync()` pattern) or surgically update in-memory state. Prefer surgical update to avoid redundant full reload.

#### 9F. Inject `IGoogleTasksService` into `TaskService`

Already registered in DI (Phase 1). Add to constructor + field.

#### 9G. Extend `ITaskService`
```
Task RefreshGoogleListsAsync();
```
Already added in 6B — confirmed.

---

## Constants to Add

`Constants.Storage.cs`:
- `PomoMetaStore = "pomoMeta"`

`Constants.Storage.cs` indexes (consumed by JS):
- `GoogleTaskId = "googleTaskId"` (used in indexedDbInterop.js index names)
- `GoogleListId = "googleListId"`

## Files Changed

| File | Action | Batch |
|------|--------|-------|
| `Models/Priority.cs` | New | 1A |
| `Models/PomodoroMeta.cs` | New | 1B |
| `Models/TaskListRef.cs` | New | 1C |
| `Models/GoogleTasksSettings.cs` | New | 1D |
| `Models/TaskItem.cs` | Extend + `WithUpdates()` | 2 |
| `Constants/Constants.Storage.cs` | Bump DB version, add store/index names | 3A |
| `wwwroot/js/indexedDbInterop.js` | Version-gated migration block for indexes + pomo_meta store | 3B |
| `wwwroot/js/constants.js` | Add `pomoMetaStore`, index names | 3B |
| `Services/Repositories/IPomodoroMetaRepository.cs` | New | 4A |
| `Services/Repositories/PomodoroMetaRepository.cs` | New | 4B |
| `Services/ServiceRegistrationService.cs` | Register sidecar repo | 4C |
| `tests/Pomodoro.Web.Tests/TestHelper.cs` | Add sidecar repo mock | 4D |
| `Services/Repositories/ITaskRepository.cs` | Add query methods | 5A |
| `Services/Repositories/TaskRepository.cs` | Implement query methods | 5B |
| `Models/AppState.cs` | Add `CurrentListId` | 6A |
| `Services/ITaskService.cs` | Add multi-list members | 6B |
| `Services/TaskService.cs` | Fix 5 copy sites, sidecar branch, `CurrentListId`, multi-list impl, join-on-read | 6C-6F, 7A |
| `Services/ImportService.cs` | Route through `WithUpdates()`, handle sidecar import | 8C |
| `Services/ExportService.cs` | Version bump, exclude Google tasks, scrub fields, sidecar export, typed `ExportData` | 8B |
| `Models/ExportData.cs` | Extract from ImportService, make public, add GoogleTasksSettings | 8A |
| `Services/TaskService.cs` | Pull: MapGoogleTaskToTaskItem, upsert, _cachedGoogleLists, RefreshGoogleListsAsync | 9B-9E |

## Tests

| Test | File |
|------|------|
| `TaskItem.WithUpdates()` copies all fields including new Google fields | New test class |
| `IsGoogleTask` computed property — true when `GoogleTaskId` set, false when null | New test class |
| `AddTimeToTaskAsync` — Google task → sidecar write, local task → TaskItem write | `TaskServiceTests` |
| `AddTimeToTaskAsync` — minutes <= 0 returns early | `TaskServiceTests` |
| `ITaskRepository.GetByGoogleListIdAsync` returns filtered results | `TaskRepositoryTests` |
| `ITaskRepository.GetByGoogleTaskIdAsync` returns matching task | `TaskRepositoryTests` |
| `IPomodoroMetaRepository` CRUD operations | `PomodoroMetaRepositoryTests` |
| `ExportData` version 2 excludes Google tasks, includes sidecar | `ExportServiceTests` |
| Version 1 import still works (back-compat, no PomodoroMeta) | `ImportServiceTests` |
| Import with orphan sidecar entries → silently discarded | `ImportServiceTests` |
| `AppStateRecord` persists/restores `CurrentListId` | `TaskServiceTests` |
| `TaskLists` builds correct refs from metadata (local + schedule + Google, including empty lists) | `TaskServiceTests` |
| `GetTasksForListAsync` filters + hydrates sidecar correctly | `TaskServiceTests` |
| `SelectListAsync` persists and notifies | `TaskServiceTests` |
| `AddTaskAsync` for Google list → blocked in read-only phase | `TaskServiceTests` |
| Sidecar priority overwrites TaskItem.Priority on join-on-read | `TaskServiceTests` |
| `MapGoogleTaskToTaskItem` — all fields mapped correctly (title, status, notes, due, etag, ids) | `TaskServiceTests` |
| Upsert — existing Google task updated, new task inserted | `TaskServiceTests` |
| Upsert — Google-deleted task detected (absent from API response) → soft-deleted | `TaskServiceTests` |
| `RefreshGoogleListsAsync` — not connected → cache cleared | `TaskServiceTests` |
| `RefreshGoogleListsAsync` — connected → cache populated, tasks upserted | `TaskServiceTests` |
| `TaskLists` uses `_cachedGoogleLists` metadata, not task grouping | `TaskServiceTests` |
| `GoogleTasksSettings` round-trips through export/import | `ExportServiceTests`/`ImportServiceTests` |
| GetTasksForListAsync clones via WithUpdates (sidecar overlay on cloned ref, original untouched) | `TaskServiceTests` |
| AddTimeToTaskAsync Google path — only LastWorkedOn in-memory, no PomodoroCount/TotalFocusMinutes bump | `TaskServiceTests` |

## Review History

### Round 2 (verified against code)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 1 | 🔴 Critical | `[JsonIgnore]` on Google IDs drops them from IndexedDB (Blazor uses System.Text.Json for IIndexedDbService calls). Cascade: `IsGoogleTask` always false after reload, indexes empty, queries return nothing. | Removed `[JsonIgnore]`. Scrub Google fields in export projection only (Batch 8B). |
| 2 | 🟠 High | Index migration inside `if (!storeNames.contains)` only runs on fresh install — v1→v2 upgrades skip it, indexes never created. | Version-gated migration block using `event.oldVersion < 2` + `tx.objectStore()` to access existing store. `indexNames.contains` guard prevents duplicate errors on fresh install. |
| 3 | 🟠 High | No join-on-read: sidecar pomodoro data written in 7A never surfaces in UI. `GetTasksForListAsync` just filters tasks. | Added hydrate step in `GetTasksForListAsync` (6F): merge `PomodoroMeta` by `GoogleTaskId` into transient display fields. Not persisted back to tasks store. |
| 4 | 🟠 High | Drive export includes all tasks (including Google). On device 2, Google tasks duplicate as orphan locals — contradicts Decision 1. | Exclude `IsGoogleTask` tasks from export (8B). Scrub Google fields defensively on remaining local tasks. |
| 5 | 🟡 Medium | `TaskLists` from grouping tasks loses empty Google lists + titles/colors (metadata comes from API, not tasks). | Build refs from list metadata (GoogleTasksSettings + GetTaskListsAsync cache), not task grouping. |
| 6 | 🟡 Medium | `AddTaskAsync` for Google list creates local task with `GoogleListId` that can't push (no `insertTask` wired in read-only phase). | Block adding to Google lists this phase — throw or redirect to local. |
| 7 | 🟡 Low | Priority dual-home (`TaskItem.Priority` + `PomodoroMeta.Priority`) risk of drift. | Documented: sidecar wins for Google tasks on join-on-read (6F). `TaskItem.Priority` for Google tasks is always display-only. |
| 8 | 🟡 Low | `SaveCurrentTaskIdAsync` rename — enumerate caller sites. | Kept method name unchanged. Added `CurrentListId` to existing record. |

### Round 3 (deeper pass)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 9 | 🟠 High | Read-only pull path missing — multi-list UI shows empty Google lists. Phase 2 builds shelves but nothing stocks them. | Added Batch 9: `RefreshGoogleListsAsync()`, `MapGoogleTaskToTaskItem()`, upsert logic, `_cachedGoogleLists`, soft-delete detection for removed Google tasks. Pull on connect + startup. |
| 10 | 🟠 Med-High | Join-on-read mutates in-place `_appState.Tasks` references → `SaveTaskAsync` leaks sidecar overlay into tasks store. | `GetTasksForListAsync` now clones via `task.WithUpdates(c => ...)` before applying sidecar overlay. Original refs in `_appState.Tasks` untouched. Stated explicitly. |
| 11 | 🟡 Medium | `GoogleTasksSettings` not in `ExportData` — per-list visibility/color won't cross devices. | Added `GoogleTasksSettings` to `ExportData` (8A amend). Export/import round-trips list prefs via Drive. |
| 12 | 🟡 Medium | `TaskLists` sources from "GetTaskListsAsync cache" but no cache field defined. Property can't await API. | Added `_cachedGoogleLists` field (9B), populated by `RefreshGoogleListsAsync()` (9C). `TaskLists` reads from snapshot. |
| 13 | 🟡 Low | Sidecar hydrate = N `GetAsync()` round-trips for large lists. | Changed to `GetAllAsync()` once → `Dictionary<string, PomodoroMeta>` keyed by `GoogleTaskId` → O(1) lookup per task. |
| 14 | Nit | Scrub lambda `t.WithUpdates(t => ...)` shadows outer `t`. | Renamed inner to `c`. |
| 15 | Nit | 7A mutates in-memory `PomodoroCount`/`TotalFocusMinutes` for Google tasks then doesn't persist — redundant (overwritten by join-on-read). | Google path now only bumps `LastWorkedOn` in-memory + writes sidecar. Skips `PomodoroCount`/`TotalFocusMinutes` in-memory bump. |

## Risks

- `ImportService.cs:184-195` is a 5th copy site easily missed — must be included in `WithUpdates` migration
- Timer sidecar branch is the architectural crux — if missed, Google tasks accumulate pomodoro data in the wrong store
- IndexedDB migration must be additive (no store drops) — existing v1 data must survive
- `ExportData` extraction from `ImportService` changes its visibility from `private` to `public` — verify no other code references the private class
- `AppStateRecord.CurrentListId` is new — old records without it must gracefully default to null (deserializers handle missing properties)
- Join-on-read must clone via `WithUpdates()` — never mutate `_appState.Tasks` references directly. Any `SaveTaskAsync` after hydrate would leak sidecar values into the tasks store.
- Pull upsert must handle the case where `_taskRepository` already contains Google tasks from a previous session — match by `GoogleTaskId`, not `Id`.
- `_cachedGoogleLists` is in-memory only — lost on page reload. Refresh on startup (if connected) mitigates this.
- `GoogleTask.CreatedAt` is not exposed by the API — new Google tasks get `DateTime.UtcNow` as CreatedAt. This is cosmetic; the true source of truth is the Google `updated` timestamp (`UpdatedAt`).
