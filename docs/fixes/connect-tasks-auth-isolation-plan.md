# Plan: TC4 fix — isolate Tasks auth failure in `ConnectAsync` (Fix 8)

> Branch: `fix/connect-tasks-auth-isolation` (based on `fix/tasks-not-rendering-sync`).
> Must merge **after** PR #109 (depends on `ReconnectRequired` / `SetReconnectRequired`).

## Problem

`CloudSyncService.ConnectAsync` calls `await _taskService.RefreshGoogleListsAsync()` directly
inside its `try` (`CloudSyncService.cs:176`). When Google Tasks returns **403 (scope gap)** or
**401**, `RefreshGoogleListsAsync` throws `UnauthorizedAccessException` (its catch filter is
`when (ex is not UnauthorizedAccessException)`, so UAE propagates). That UAE lands in
`ConnectAsync`'s generic catch, which logs a misleading **"Google Drive authentication failed"**
and **returns false** — aborting the **entire** connect, *including Drive*, even though Drive has
its own valid `drive.appdata` scope.

Observed in manual test TC4: a Tasks-only scope gap (the documented 403) made `Connect` fail
outright, so the user could never reach the "stale list" recovery scenario the test exercises.

```
ConnectAsync:176  RefreshGoogleListsAsync  -- UAE (403) -->
  RefreshGoogleListsAsync finally runs, UAE propagates -->
  ConnectAsync catch-all --> log "Drive auth failed" --> return false
  (Drive never establishes; SyncNowAsync never runs)
```

## Goal

A Tasks-only auth failure during Connect must **not** abort the Drive connect. It should:
- let the connect succeed (`return true`) so Drive sync + periodic sync start, and
- surface the Tasks problem via the existing **`ReconnectRequired`** banner (Fix 3), which
  prompts the user to re-consent (a 403 scope gap is resolved by a full re-consent that widens
  the grant — exactly what `ConnectAsync`→`requestAuth` does).

This mirrors the **Fix 2** isolation already applied to `InitializeAsync`.

## Implementation steps

### 1. `src/Pomodoro.Web/Services/CloudSyncService.cs` — `ConnectAsync`
Replace the bare call with an isolated one:

```csharp
StartPeriodicSync();

try
{
    await _taskService.RefreshGoogleListsAsync();
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning(ex, "Google Tasks auth failed during connect; Drive sync will continue. Reconnect required.");
    SetReconnectRequired(true);
}

NotifyStatusChanged();

var syncResult = await SyncNowAsync();
// ... existing handling
```

Rationale for placement (before `SyncNowAsync`):
- `RefreshGoogleListsAsync` failing should not block the Drive `SyncNowAsync` that follows.
- `SetReconnectRequired(true)` is idempotent + notifies; the banner appears.
- Keeping it inside the outer `try` means a *non-auth* exception still fails the connect
  (unchanged behaviour); only UAE is tolerated here.

### 2. Tests — `tests/.../CloudSyncServiceTests.cs`
Add (mirroring `InitializeAsync_TasksRefreshUnauthorized_StillInitializesWithoutRetry`):

- `ConnectAsync_TasksRefreshUnauthorized_StillConnectsAndReturnsTrue`
  - `_mockTaskService.RefreshGoogleListsAsync()` throws `UnauthorizedAccessException`.
  - `_mockGoogleDrive.ConnectAsync()` returns a token (Drive success).
  - Assert: result is `true`, `_sut.ReconnectRequired` is `true`, and Drive
    `ConnectAsync()` was invoked once.

- `ConnectAsync_TasksRefreshUnauthorized_StillRunsDriveSyncNow`
  - Same setup, plus a Drive sync path that succeeds.
  - Assert: `FindSyncFileAsync`/`SyncNowAsync` was reached (Drive connected), i.e. the Tasks
    throw did not abort the connect.

### 3. Doc — `docs/fixes/tasks-not-rendering.md`
Add a **Fix 8** subsection under the Fixes area describing the Connect-path isolation, and
update the status note + Files-touched list.

## Verification

- `dotnet build` — 0 errors
- `dotnet format Pomodoro.sln --verify-no-changes` — clean
- `dotnet test` — green (target 3618 with the two new tests)
- Manual re-run of **TC4**: Connect now succeeds even with a Tasks 403; the reconnect banner
  appears; Drive sync works; clicking the Tasks tab still recovers to local tasks.

## Out of scope

- The 403 scope gap itself remains a **configuration** issue (revoke + reconnect / enable Tasks
  API). Fix 8 only stops it from killing the Drive connect and routes the user to reconnect.
- No change to the 401/403 distinction in `GoogleTasksService` (401 retries via silent re-auth;
  403 throws `UnauthorizedAccessException`, which Fix 8 now tolerates in Connect).
