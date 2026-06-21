# Phase 1 — Google Tasks API Client

## Goal

Create a JS interop layer and C# service for Google Tasks API REST calls, mirroring the existing `GoogleDriveService` pattern.

## Prerequisites

- Phase 0 complete (auth scope added, access token available)
- Existing patterns: `GoogleDriveService` (401 → reconnect), `SafeTaskRunner`

## Steps

### 1. Create `googleTasks.js`

**File:** `wwwroot/js/googleTasks.js` (new)

REST base: `https://tasks.googleapis.com/tasks/v1`

Functions:
- `listTaskLists(accessToken)` — `GET /users/@me/lists`
- `listTasks(accessToken, listId, { updatedMin?, showCompleted?, showHidden?, showDeleted?, pageToken? })` — `GET /lists/{listId}/tasks`
- `insertTask(accessToken, listId, { title, notes, due, status? })` — `POST /lists/{listId}/tasks` (Phase 3)
- `patchTask(accessToken, listId, taskId, { title?, notes?, due?, status? })` — `PATCH /lists/{listId}/tasks/{taskId}` (Phase 3)
- `deleteTask(accessToken, listId, taskId)` — `DELETE /lists/{listId}/tasks/{taskId}` (Phase 3)

All functions:
- Set `Authorization: Bearer {accessToken}` header
- Return parsed JSON; throw on non-OK with status code in message
- Handle pagination (loop on `nextPageToken` for `listTasks`, return all results as flat array)

**Pull query params (review round 2):**
- `tasks.list` hides completed and hidden tasks by default
- Use `showCompleted=true&showHidden=true` for displaying/finishing tasks
- For Phase 3 tombstone reconcile: also `showDeleted=true`
- Page size is 100 — loop on `nextPageToken`

### 2. Add script tag

**File:** `wwwroot/index.html:101`

Add `<script src="js/googleTasks.js"></script>` immediately after `googleDrive.js`.

The SW (`service-worker.published.js`) caches static `.js` assets — no changes needed.

### 3. Add interop constants

**File:** `Constants/Constants.JsInterop.cs`

Expand `GoogleTasksJsFunctions`:
```
ListTaskLists = "listGoogleTaskLists"
ListTasks = "listGoogleTasks"
InsertTask = "insertGoogleTask"        // Phase 3
PatchTask = "patchGoogleTask"          // Phase 3
DeleteTask = "deleteGoogleTask"        // Phase 3
```

**File:** `Constants/Constants.Sync.cs`

```
TasksApiBase = "https://tasks.googleapis.com/tasks/v1"
TasksListTasksPageSize = 100
```

### 4. Create DTOs

**File:** `Models/GoogleTaskList.cs` (new)
```
record GoogleTaskList(string Id, string Title);
```

**File:** `Models/GoogleTask.cs` (new)
```
record GoogleTask(
    string Id, string Title, string? Notes, string? Due,
    string Status, string Updated, string? Parent, string? Position, string? ETag
);
```

### 5. Create service interface + implementation

**File:** `Services/IGoogleTasksService.cs` (new)
```
interface IGoogleTasksService
{
    Task<IReadOnlyList<GoogleTaskList>> GetTaskListsAsync();
    Task<IReadOnlyList<GoogleTask>> GetTasksAsync(string listId, string? updatedMin = null);
    Task<GoogleTaskList?> GetTaskListByIdAsync(string listId);  // Phase 3
    Task<GoogleTask?> GetTaskByIdAsync(string listId, string taskId);  // Phase 3
    Task<bool> IsConnectedAsync();
}
```

**File:** `Services/GoogleTasksService.cs` (new)

Mirror `GoogleDriveService` pattern:
- Inject `IJSRuntime`, `ILogger<GoogleTasksService>`
- Get access token from `GoogleDriveService` (or `SyncStateRecord`)
- All methods: try fetch → 401 → set not connected, throw `UnauthorizedAccessException`
- 429 → exponential backoff + retry (max 3 retries); surface non-fatal "rate-limited" state
- 403 → log warning, throw with actionable message

**Error messages (new constants):**
```
SyncMessages.LogTasksRateLimited = "Google Tasks API rate-limited, retrying"
SyncMessages.LogTasksForbidden = "Tasks API access forbidden — check scope"
SyncMessages.TasksReconnectRequired = "Tasks connection lost — reconnect required"
```

### 6. Register in DI

**File:** `Services/ServiceRegistrationService.cs`

```
services.AddScoped<IGoogleTasksService, GoogleTasksService>();
```

### 7. Add test mock

**File:** `tests/.../Helpers/TestHelper.cs`

Add `Mock.Of<IGoogleTasksService>()` to constructor.

## Change sites

| File | Change |
|------|--------|
| `wwwroot/js/googleTasks.js` (new) | REST functions for Tasks API |
| `wwwroot/index.html:101` | Add script tag |
| `Constants/Constants.JsInterop.cs` | Add `GoogleTasksJsFunctions` |
| `Constants/Constants.Sync.cs` | Add API base, page size, error messages |
| `Models/GoogleTaskList.cs` (new) | DTO |
| `Models/GoogleTask.cs` (new) | DTO |
| `Services/IGoogleTasksService.cs` (new) | Interface |
| `Services/GoogleTasksService.cs` (new) | Implementation |
| `Services/ServiceRegistrationService.cs` | Register service |
| `tests/.../Helpers/TestHelper.cs` | Add mock |

## Tests

- Unit: `GoogleTasksService` — get lists returns parsed data; get tasks with pagination loops correctly
- Unit: 401 → throws `UnauthorizedAccessException`, sets not-connected
- Unit: 429 → retries with backoff, eventually throws
- Unit: 403 → throws with forbidden message
- Unit: empty list returns empty collection (not null)
- E2E: (mocked) — service returns task lists

## Risks

- Token expiry mid-call → 401 pattern handles; add `TrySilentAuth` retry before giving up
- Google Tasks API quotas (429) — backoff prevents runaway retries
