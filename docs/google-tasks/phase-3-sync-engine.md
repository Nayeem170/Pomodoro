# Phase 3 — Sync Engine (2-way)

## Goal

Implement bidirectional sync between local IndexedDB and Google Tasks API: per-list reconcile with pull, push, conflict resolution, and delete handling.

## Prerequisites

- Phase 2 complete (data model, sidecar, multi-list `ITaskService`)
- Phase 1 API client write methods available (`insertTask`, `patchTask`, `deleteTask`)
- Existing: `CloudSyncService` debounce pattern (5000ms), `SafeTaskRunner`

## Steps

### 1. Widen auth scope

**File:** `wwwroot/js/googleDrive.js:32`

Change `tasks.readonly` → `tasks` (full read/write scope). This requires re-auth for existing users.

**File:** `Constants/Constants.Sync.cs` — swap `TasksScope` → `TasksScopeReadWrite` in init.

### 2. Create `IGoogleTasksSyncService`

**File:** `Services/IGoogleTasksSyncService.cs` (new)
```
interface IGoogleTasksSyncService
{
    Task<SyncResult> SyncAllListsAsync();
    Task<SyncResult> SyncListAsync(string listId);
    bool IsSyncing { get; }
    event EventHandler<SyncStateChangedEventArgs>? SyncStateChanged;
}
```

### 3. Create `GoogleTasksSyncService`

**File:** `Services/GoogleTasksSyncService.cs` (new)

Per-list reconcile loop:

#### Pull (Google → local)
```
1. Read `updatedMin` from `GoogleTasksSettings.Lists[listId].LastSync`
2. Call `IGoogleTasksService.GetTasksAsync(listId, updatedMin)` with showCompleted/showHidden/showDeleted
3. For each remote task:
   - Find local task by `GoogleTaskId`
   - If exists: conflict check (see below)
   - If not exists: create new `TaskItem` with Google fields populated, save to `ITaskRepository`
4. Update `GoogleTasksSettings.Lists[listId].LastSync = DateTime.UtcNow`
```

#### Push (local → Google)
```
1. Get local tasks for list via `ITaskRepository.GetByGoogleListIdAsync(listId)`
2. For each local task:
   - If `GoogleTaskId == null` (new local task): call `insertTask` → set `GoogleTaskId` + `ETag` on local
   - If `UpdatedAt > remote.Updated` (local is newer): call `patchTask` → update `ETag`
   - If `IsDeleted` and not yet confirmed deleted: call `deleteTask` → remove local tombstone
```

#### Conflict resolution
- **Last-writer-wins:** compare local `UpdatedAt` vs remote `Updated` timestamp
- **ETag guard:** if local `ETag` differs from current remote → remote was modified elsewhere → accept remote (pull wins)
- **Same timestamp:** prefer local (user is actively working)

#### Delete handling
- Local soft-delete (`IsDeleted = true`) → Google delete
- Keep local tombstone until confirmed by next successful sync
- Remote delete → mark local `IsDeleted = true`, soft-delete

### 4. Sync triggers

| Trigger | Action |
|---------|--------|
| Manual Sync button | `SyncAllListsAsync()` |
| On connect (after auth) | `SyncAllListsAsync()` |
| After local task edit | Debounced (reuse `Constants.Sync.DebounceDelayMs` = 5000ms) |
| On app load | Pull only (no push — user may not want immediate upload) |

Use `SafeTaskRunner.RunAndForget()` for isolation (no exception propagation to UI).

### 5. Per-list sync cursor

**File:** `Models/GoogleTasksSettings.cs` — already has `ListSetting.LastSync`

Store per-list `LastSync` timestamp. Updated after each successful pull/push for that list.

### 6. Error handling

| Error | Action |
|-------|--------|
| 401 | Disconnect Tasks, throw `UnauthorizedAccessException` (same pattern as Drive) |
| 429 | Exponential backoff + retry (max 3) from Phase 1; surface "rate-limited" state |
| Network error | Surface "offline" state, keep cached data, retry on next trigger |
| Partial failure (some lists sync, some fail) | Report per-list failure; sync successful lists, retry failed on next trigger |

### 7. ActivityRecord portability (review round 3 medium gap — deferred)

`ActivityRecord` groups by `TaskName` — Google task renames split history. Future: add `GoogleTaskId?` to `ActivityRecord` and group by it. Not blocking for Phase 3.

### 8. Schedule tab scope (review round 2)

Schedule (yellow) tab is a **local filtered view** of tasks with `ScheduledDate`/`Repeat`. Not affected by Google sync. No changes needed.

## Change sites

| File | Change |
|------|--------|
| `wwwroot/js/googleDrive.js:32` | Widen scope to `auth/tasks` |
| `Services/IGoogleTasksSyncService.cs` (new) | Interface |
| `Services/GoogleTasksSyncService.cs` (new) | Per-list reconcile engine |
| `Services/ServiceRegistrationService.cs` | Register sync service |
| `tests/.../Helpers/TestHelper.cs` | Add sync service mock |
| `Constants/Constants.Sync.cs` | Sync state messages |

## Tests

- Unit: pull — remote new task upserts locally; remote updated task merges with local
- Unit: push — local new task creates on Google; local dirty task patches on Google
- Unit: conflict — last-writer-wins by timestamp; ETag mismatch → remote wins
- Unit: delete — local soft-delete pushes Google delete; remote delete marks local deleted
- Unit: partial failure — some lists sync, failed lists reported
- Unit: debounce — multiple rapid edits trigger single sync
- Unit: per-list cursor — `LastSync` updated per list independently
- E2E: connect → add task locally → sync → verify on Google (mocked API)

## Risks

- Conflict resolution is best-effort (last-writer-wins) — no merge UI
- Subtasks = flat Google tasks with `parent`+`position` — fiddly ordering
- Token expiry mid-sync → 401 pattern handles; add `TrySilentAuth` retry before failing
- Keep Tasks-API sync and Drive-envelope sync from overlapping on task data (decision 1)
