# Phase 3 — Sync Engine (2-way)

## Goal

Implement bidirectional sync between local IndexedDB and Google Tasks API: add write operations to `IGoogleTasksService`, remove read-only guards from `TaskService`, wire sync triggers, and handle conflict resolution.

## Prerequisites

- Phase 2 complete (data model, sidecar, multi-list `ITaskService`) — merged PR #91
- Phase 4 complete (read-only UI, list tabs, guards) — PR #98 pending
- JS functions already exist: `googleTasks.insertTask`, `googleTasks.patchTask`, `googleTasks.deleteTask`
- C# constants already defined: `Constants.JsInterop.GoogleTasksJsFunctions.InsertTask/PatchTask/DeleteTask`
- `Constants.Sync.TasksScopeReadWrite` already defined
- `TrySilentAuthAsync` pattern exists on `IGoogleDriveService`

## Current State (what Phase 4 left)

| Location | State |
|----------|-------|
| `TaskService.AddTaskAsync(name, "google-list-id")` | Throws `NotSupportedException` |
| `TaskService.CompleteTaskAsync` (Google task) | No-op early return |
| `TaskService.UncompleteTaskAsync` (Google task) | No-op early return |
| `TaskService.DeleteTaskAsync` (Google task) | No-op early return |
| `TaskService.UpdateTaskAsync` | Only updates `Name`, no Google push |
| `googleDrive.js:32` scope | `tasks.readonly` |
| `IGoogleTasksService` | Read-only (get only) |
| `CloudSyncService.SyncNowAsync()` | Drive-envelope only, never calls `RefreshGoogleListsAsync` |

## Steps

### 1. Widen auth scope

**Files:** `wwwroot/js/googleDrive.js:32`

- Change `tasks.readonly` → `tasks` in scope string
- Existing users with `tasks.readonly` token must re-authenticate
- `trySilentAuth()` returns null for existing users (new scope not granted) → `requestAuth()` needed
- No prompt for re-auth if user only uses Drive sync (don't block)

### 2. Add write methods to `IGoogleTasksService`

**File:** `Services/IGoogleTasksService.cs`

```
Task<GoogleTask> InsertTaskAsync(string listId, GoogleTask task);
Task<GoogleTask?> PatchTaskAsync(string listId, string taskId, GoogleTaskPatch updates, string? etag = null);
Task DeleteTaskAsync(string listId, string taskId);
```

**New model:** `Models/GoogleTaskPatch.cs`
```csharp
public record GoogleTaskPatch(string? Title = null, string? Notes = null, string? Status = null, string? Due = null);
```

### 3. Implement write methods in `GoogleTasksService`

**File:** `Services/GoogleTasksService.cs`

- `InsertTaskAsync` → `googleTasks.insertTask(accessToken, listId, task)` via `IJSRuntime`
- `PatchTaskAsync` → `googleTasks.patchTask(accessToken, listId, taskId, updates)` with `If-Match` ETag header (see step 3a)
- `DeleteTaskAsync` → `googleTasks.deleteTask(accessToken, listId, taskId)` via `IJSRuntime`

**Retry:** Reuse `ExecuteWithRetryAsync<T>` for insert/patch. Add non-generic `ExecuteVoidWithRetryAsync` for delete (JS returns undefined on success).

### 3a. Add `If-Match` ETag header to `patchTask` JS

**File:** `wwwroot/js/googleTasks.js`

Current `patchTask` sends `Content-Type: application/json` but no `If-Match`. Google Tasks API uses ETags for optimistic concurrency. Add ETag parameter:

```js
patchTask: function(accessToken, listId, taskId, updates, etag) {
    var headers = this._getAuthHeaders(accessToken);
    if (etag) headers['If-Match'] = etag;
    ...
}
```

On ETag mismatch (HTTP 412 Precondition Failed), throw `'412 ETag mismatch'`. `ExecuteWithRetryAsync` treats this as a conflict — caught in step 6.

### 4. Widen `UpdateTaskAsync` for Google tasks

**File:** `Services/TaskService.cs`, `Components/Tasks/TaskEditPanel.razor`

Current `UpdateTaskAsync` only updates `Name`. Phase 3 needs to push `Notes` and `DueDate` to Google when they change.

**Option A (minimal):** Accept the full `TaskItem` parameter (already does), diff against existing in-memory task, and push only changed Google fields. No TaskEditPanel change needed yet — the field values just won't come from UI (always null/default from current edit form).

```csharp
public async Task UpdateTaskAsync(TaskItem task)
{
    // existing name validation...
    
    var existingTask = _appState.FindTaskById(task.Id);
    if (existingTask == null) return;
    
    var taskToSave = existingTask.WithUpdates(c =>
    {
        c.Name = task.Name ?? c.Name;
        c.Notes = task.Notes;
        c.DueDate = task.DueDate;
    });
    
    await SaveTaskAsync(taskToSave);
    _appState.UpdateTask(task.Id, t =>
    {
        t.Name = taskToSave.Name;
        t.Notes = taskToSave.Notes;
        t.DueDate = taskToSave.DueDate;
    });
    
    if (taskToSave.IsGoogleTask)
    {
        var patch = BuildPatch(existingTask, taskToSave);
        if (patch != null)
        {
            try
            {
                var result = await _googleTasksService.PatchTaskAsync(
                    taskToSave.GoogleListId!, taskToSave.GoogleTaskId!, patch, taskToSave.ETag);
                if (result != null)
                {
                    var updatedEtag = result.ETag;
                    _appState.UpdateTask(task.Id, t => t.ETag = updatedEtag);
                }
            }
            catch (ConflictException)
            {
                await _googleTasksService.RefreshGoogleListsAsync();
            }
        }
    }
    
    NotifyStateChanged();
    MarkDirty();
}
```

### 5. Wire direct push into `TaskService` write paths

**File:** `Services/TaskService.cs`

Remove read-only guards and add Google push in each:

| Method | Guard removed | Google API call |
|--------|---------------|-----------------|
| `AddTaskAsync(name, googleListId)` | Remove `NotSupportedException` | `InsertTaskAsync` → set `GoogleTaskId` + `ETag` |
| `CompleteTaskAsync` (Google) | Remove no-op return | `PatchTaskAsync(status: completed, etag)` |
| `UncompleteTaskAsync` (Google) | Remove no-op return | `PatchTaskAsync(status: needsAction, etag)` |
| `DeleteTaskAsync` (Google) | Remove no-op return | `DeleteTaskAsync` → then soft-delete local |
| `UpdateTaskAsync` (Google) | N/A (no guard) | `PatchTaskAsync` with changed fields |

**Helper: `BuildPatch(existing, updated)`**
Returns `GoogleTaskPatch` with only the fields that actually changed. Omit unchanged fields (Google API PATCH is field-replacement).

### 6. Error handling for write operations

| Error | Action |
|-------|--------|
| 401 | Throw `UnauthorizedAccessException` — triggers re-auth flow |
| 403 | Scope insufficient — surface "re-authenticate for full Google Tasks access" |
| 412 (ETag mismatch) | Conflict — local was stale. Pull via `RefreshGoogleListsAsync` to reconcile. **Do not retry push.** |
| 429 | Retry with backoff (handled by `ExecuteWithRetryAsync`) |
| Network error | Push fails — set `_localDirty = true` on the task, surface error, retry on next manual sync |

### 6a. Offline/push-failure handling — dirty flag per task

**Problem:** If push fails (network), next `RefreshGoogleListsAsync` pull overwrites local changes (e.g., `IsCompleted = true` gets set back to `needsAction` because Google never received the patch).

**Solution:** Add `bool IsLocalDirty` to `TaskItem`. Set to `true` when push fails or when local mutation hasn't been confirmed by Google.

`RefreshGoogleListsAsync` merge logic change:
```
For each remote task:
  Find local by GoogleTaskId
  If local.IsLocalDirty → SKIP overwrite (local wins, push will retry)
  Else → accept remote (existing pull behavior)
```

`IsLocalDirty` is cleared when `RefreshGoogleListsAsync` confirms local state matches remote (title, status, notes, due all match).

**Persistence:** `IsLocalDirty` stored in IndexedDB alongside `TaskItem` (already saved via `_taskRepository`).

### 7. Wire Google pull into `CloudSyncService`

**File:** `Services/CloudSyncService.cs`

`CloudSyncService.SyncNowAsync()` currently only syncs Drive envelope. Add:
```csharp
var taskService = _serviceProvider.GetService<ITaskService>();
if (taskService != null)
    await taskService.RefreshGoogleListsAsync();
```
After successful Drive pull (not on failure — avoid cascading errors).

Also add to `ConnectAsync` (after successful auth).

### 8. Suppress `MarkDirty()` for Google-only mutations

**Problem:** Removing guards means `CompleteTaskAsync`/`UncompleteTaskAsync`/`DeleteTaskAsync` call `MarkDirty()`, triggering Drive envelope push 5s later. This is dual-write: direct Google push + Drive envelope push.

**Solution:** In `TaskService`, before `MarkDirty()` calls in write paths:
```csharp
if (!existingTask.IsGoogleTask)
    MarkDirty();
```

Google-only mutations don't trigger Drive envelope push. Pomodoro metadata (sidecar) updates from `AddTimeToTaskAsync` also don't call `MarkDirty()` — this is intentional and already the case.

### 9. Scope validation on connect

**Problem:** `GetTaskListsAsync` works with `tasks.readonly` scope — can't verify write access with a read call.

**Solution:** After successful auth, attempt a lightweight write test: insert a test task, immediately delete it. If either returns 403 → scope insufficient → surface "re-authenticate" message.

**Simpler alternative:** Inspect the OAuth token response's `scope` field from `googleDrive.js`. Store scope string in `SyncStateRecord` or `GoogleTasksSettings`. Check for `https://www.googleapis.com/auth/tasks` (not `tasks.readonly`).

### 10. Handle hidden completed Google tasks

**Problem:** `listTasks` JS sends `showCompleted=true&showHidden=true`. Google auto-hides completed tasks after ~1 week (`hidden=true`). These accumulate in local IndexedDB.

**Solution:** In `RefreshGoogleListsAsync`, when upserting a remote task that is `status: completed` AND `hidden: true` (add `hidden` to `GoogleTask` model), skip the upsert — these are auto-archived by Google. If local copy exists, soft-delete it.

### 11. Remove UI read-only guards

**File:** `Components/Tasks/TaskItemComponent.razor`

| Element | Change |
|---------|--------|
| Checkbox `disabled` | Remove `disabled="@Item.IsGoogleTask"` |
| Delete button | Remove `@if (!Item.IsGoogleTask)` wrapper |
| Add task button | Enable on Google list tabs (already works once `AddTaskAsync` stops throwing) |

**File:** `Components/Tasks/TaskListSyncStrip.razor`

- Change "read-only" → "synced"

## Change Sites

| File | Change |
|------|--------|
| `wwwroot/js/googleDrive.js:32` | Widen scope `tasks.readonly` → `tasks` |
| `wwwroot/js/googleTasks.js` | Add `etag` param to `patchTask`, `If-Match` header, throw on 412 |
| `Models/GoogleTask.cs` | Add `bool? Hidden` property |
| `Models/GoogleTaskPatch.cs` (new) | Typed patch record |
| `Models/TaskItem.cs` | Add `bool IsLocalDirty` property |
| `Services/IGoogleTasksService.cs` | Add `InsertTaskAsync`, `PatchTaskAsync`, `DeleteTaskAsync` |
| `Services/GoogleTasksService.cs` | Implement write methods, add `ExecuteVoidWithRetryAsync` |
| `Services/TaskService.cs` | Remove guards, add Google push in write paths, `BuildPatch` helper, dirty flag handling |
| `Services/TaskService.cs` `RefreshGoogleListsAsync` | Skip overwrite for `IsLocalDirty` tasks, handle hidden completed |
| `Services/CloudSyncService.cs` | Call `RefreshGoogleListsAsync` after Drive pull |
| `Components/Tasks/TaskItemComponent.razor` | Remove `disabled`/hidden for Google tasks |
| `Components/Tasks/TaskListSyncStrip.razor` | "read-only" → "synced" |
| `Constants/Constants.Sync.cs` | Update scope reference |
| `Constants/Constants.JsInterop.cs` | Add `DeleteTask` to `GoogleTasksJsFunctions` (already defined but verify) |

## Tests

| Category | Test |
|----------|------|
| Unit | `InsertTaskAsync` for Google list → calls JS, sets `GoogleTaskId` + `ETag` on local |
| Unit | `CompleteTaskAsync` for Google task → `PatchTaskAsync` with `status: completed` + etag |
| Unit | `UncompleteTaskAsync` for Google task → `PatchTaskAsync` with `status: needsAction` |
| Unit | `DeleteTaskAsync` for Google task → calls `DeleteTaskAsync` then soft-deletes local |
| Unit | `UpdateTaskAsync` for Google task → `PatchTaskAsync` only with changed fields |
| Unit | Push failure (network) → sets `IsLocalDirty = true`, keeps local state |
| Unit | ETag mismatch (412) → caught as conflict, triggers pull |
| Unit | `RefreshGoogleListsAsync` skips overwrite when `IsLocalDirty` |
| Unit | `IsLocalDirty` cleared when local matches remote after pull |
| Unit | `MarkDirty` not called for Google-only mutations |
| Unit | Hidden completed Google tasks → soft-deleted locally |
| Component | Checkbox enabled for Google tasks, delete visible |
| Component | Add button works on Google list tabs |
| Component | Sync strip shows "synced" |

## Risks

- **Scope change breaks existing users** — users with `drive.appdata` + `tasks.readonly` token must re-authenticate. Handle with re-auth prompt, not forced logout.
- **`IsLocalDirty` flag adds state** — must persist to IndexedDB, clear correctly, and not accumulate indefinitely. Risk of stale dirty flags preventing pulls. Mitigate: clear dirty when local matches remote after successful pull.
- **No subtask support** — Google Tasks subtasks use `parent` + `position`. Phase 3 does not create/move subtasks. Defer.
- **Direct push is not transactional** — if app crashes between local write and Google push, local state may diverge. `IsLocalDirty` + pull reconciliation handles this.
- **`DeleteTaskAsync` returns 404 if already deleted remotely** — handle gracefully (idempotent delete, no error).

## Implementation Order

1. Widen auth scope (step 1) — single JS line change
2. Add `Hidden` to `GoogleTask` model, `GoogleTaskPatch` model, `IsLocalDirty` to `TaskItem`
3. Add `If-Match` + 412 handling to `patchTask` JS (step 3a)
4. Add write methods to interface + implementation (steps 2-3)
5. Add `BuildPatch` helper (step 5)
6. Wire push into TaskService write paths + remove guards (step 5)
7. Widen `UpdateTaskAsync` for Google tasks (step 4)
8. Wire Google pull into `CloudSyncService` (step 7)
9. Suppress `MarkDirty()` for Google mutations (step 8)
10. `RefreshGoogleListsAsync` dirty-flag reconciliation + hidden task handling (steps 6a, 10)
11. Remove UI guards (step 11)
12. Error handling + scope validation (steps 6, 9)
13. Tests

## Review History

### Round 1 (verified against code)
| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 1 | HIGH | `CloudSyncService` never calls `RefreshGoogleListsAsync` — plan falsely claims it does | Step 7: add explicit call in `SyncNowAsync` after Drive pull |
| 2 | HIGH | `UpdateTaskAsync` only updates `Name` — plan flags but never fixes | Step 4: widen `UpdateTaskAsync` to accept Notes/DueDate, push changed Google fields |
| 3 | HIGH | `patchTask` JS has no `If-Match` ETag header — conflict detection is non-functional | Step 3a: add `etag` param to JS, `If-Match` header, 412 error |
| 4 | HIGH | No offline queue — failed push gets overwritten by next pull | Step 6a: `IsLocalDirty` flag on `TaskItem`, pull skips dirty tasks |
| 5 | MED | `_googleSyncDirty` flag (old step 4) contradicts direct-push | Removed. Replaced with per-task `IsLocalDirty` (step 6a) |
| 6 | MED | `deleteTask` JS returns undefined — can't use generic retry | Add non-generic `ExecuteVoidWithRetryAsync` |
| 7 | MED | `GetTaskListsAsync` works with `tasks.readonly` — false scope validation | Step 9: inspect token scope string instead |
| 8 | MED | `MarkDirty()` on Google mutations triggers dual Drive+Tasks push | Step 8: suppress `MarkDirty()` for Google-only mutations |
| 9 | MED | Step 9 (push unpushed on pull) contradicts direct-push | Removed. Direct push means no unpushed tasks at pull time (except `IsLocalDirty` push-failures) |
| 10 | LOW | `AddTimeToTaskAsync` doesn't `MarkDirty()` — intentional? | Yes. Sidecar is Pomodoro-only metadata, not Google Tasks data. Documented in step 8. |
| 11 | LOW | 403 already throws `UnauthorizedAccessException` in existing code | No change needed — existing behavior is correct |
| 13 | LOW | Patch helper always sends `notes`/`due` — should only send changed | Step 5: `BuildPatch` only includes changed fields |
| 14 | LOW | Hidden completed Google tasks accumulate | Step 10: auto-soft-delete hidden completed tasks on pull |
