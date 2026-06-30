# Fix: Tasks not rendering / Google sync fails on startup (401 storm)

## Symptom

On startup the console repeats, three times:

```text
tasks.googleapis.com/tasks/v1/users/@me/lists  401 ()
warn: GoogleTasksService  Sync unauthorized, reconnection required
warn: CloudSyncService    Cloud sync init attempt N failed
System.UnauthorizedAccessException: Tasks connection lost — reconnect required
 ---> Microsoft.JSInterop.JSException: 401 Unauthorized
   at GoogleTasksService.GetTaskListsAsync()        GoogleTasksService.cs:26
   at TaskService.RefreshGoogleListsAsync()         TaskService.cs:483
   at CloudSyncService.InitializeAsync()            CloudSyncService.cs:130
```

Google Tasks lists never load; periodic sync never starts.

## Confirmed root cause (runtime trace 2026-06-28) — token expiry, no silent re-auth

The cause is an **expired access token that is never refreshed**, plus retry/propagation
logic that turns a routine, recoverable expiry into a hard startup failure.

```text
Persisted access token restored at CloudSyncService.cs:126 is expired (GIS tokens ~1h)
  -> GET /users/@me/lists returns 401
  -> GoogleTasksService.ExecuteWithRetryAsync (GoogleTasksService.cs:165) converts 401 ->
     UnauthorizedAccessException WITHOUT calling TrySilentAuthAsync() to mint a fresh token
  -> TaskService.RefreshGoogleListsAsync catch filter `when (ex is not UnauthorizedAccessException)`
     (TaskService.cs:647) deliberately lets it propagate
  -> CloudSyncService.InitializeAsync generic catch (CloudSyncService.cs:138) logs
     "init attempt N failed" and retries 3x — each retry reuses the SAME stale token
     (re-read from syncState.AccessToken at :126) -> 3 identical 401s
  -> after 3 attempts init gives up; StartPeriodicSync() (CloudSyncService.cs:131) never runs
```

### Defect 1 (primary) — 401 handler never attempts silent re-auth

`src/Pomodoro.Web/Services/GoogleTasksService.cs:165-168`:

```csharp
catch (JSException ex) when (ex.Message.Contains("401"))
{
    _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
    throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired, ex);
}
```

`GetAccessTokenAsync()` (`GoogleDriveService.cs:99`) just returns the stored token — it does
**not** refresh. The "retry" loop in `ExecuteWithRetryAsync` only retries on `429`; on `401`
it throws immediately. A `TrySilentAuthAsync()` path exists
(`GoogleDriveService.cs:32`, GIS silent token flow) but is **never invoked**, so an expired
token can never self-heal.

### Defect 2 — futile retries + Tasks 401 aborts whole sync init

`src/Pomodoro.Web/Services/CloudSyncService.cs:108-150`: the 3× retry loop re-reads the same
persisted `syncState.AccessToken` each attempt, so all three attempts 401 identically. And
because `RefreshGoogleListsAsync` (a read-only Google pull) throws on 401, the entire
`InitializeAsync` attempt aborts — `StartPeriodicSync()` at `:131` is skipped, disabling
**all** sync (including Drive), not just Tasks.

### Why it's expiry, not a scope gap

403 (insufficient scope) is handled separately at `GoogleTasksService.cs:175`. The trace is
**401**, so the token is recognized but expired — silent re-auth will fix it. (If an existing
Drive-only user had never consented `tasks.readonly`, that path returns 403 and needs a
re-Connect prompt instead — see Appendix C.)

## Fixes

> **Status: RESOLVED.** Fix 1–7 implemented (build clean). Fix 6 (render-boundary collapse)
> makes the empty list render the local tasks; Fix 7 (tab click compares the real
> `CurrentListId`, not the fallback) closes the residual "click does nothing" dead-end. The
> symptom that persisted across earlier passes was **R0 — the running process served a stale
> pre-fix binary**; restarting the app at `:7025` after Fix 7 fixed it (see "RESOLVED" section).

### Fix 1 (primary) — silent re-auth on 401, then retry once  ✅ implemented

`src/Pomodoro.Web/Services/GoogleTasksService.cs` (`ExecuteWithRetryAsync`, the 401 catch):

```csharp
catch (JSException ex) when (ex.Message.Contains("401"))
{
    _logger.LogWarning(Constants.SyncMessages.LogSyncUnauthorized);
    if (attempt < maxRetries - 1 && await _googleDriveService.TrySilentAuthAsync())
    {
        token = await _googleDriveService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired, ex);
        continue; // retry once with the refreshed token
    }
    throw new UnauthorizedAccessException(Constants.SyncMessages.TasksReconnectRequired, ex);
}
```

`token` is the reassignable loop-local already passed to `action(token)`, so `continue`
re-runs the request with the fresh token. Only a genuine re-auth failure surfaces as
`UnauthorizedAccessException`. This removes the 401 storm for the common expired-token case.

Tests: `GetTasksAsync_On401_RetriesAfterSilentReauthSucceeds` (re-auth success → retry
succeeds) and `GetTasksAsync_On401_ThrowsWhenSilentReauthFails` (re-auth fails → UAE). The
existing `_ThrowsUnauthorized_On401` cases still pass because the default mock returns
`false` from `TrySilentAuthAsync()`.

### Fix 2 — handle auth failure distinctly in init (no blind retries; don't kill Drive sync)  ✅ implemented

`src/Pomodoro.Web/Services/CloudSyncService.cs` (`InitializeAsync`): the Tasks pull is
isolated so a Tasks 401 cannot abort the whole init, and the outer retry no longer fires
blindly on `UnauthorizedAccessException`:

```csharp
if (!string.IsNullOrEmpty(ClientId) && syncState.IsConnected)
{
    await _googleDriveService.InitializeAsync(ClientId);
    var authed = await _googleDriveService.TrySilentAuthAsync();
    if (!authed)
        SetReconnectRequired(true);
    _googleDriveService.SetConnected(true);
    _googleDriveService.SetAccountEmail(syncState.AccountEmail);

    try
    {
        await _taskService.RefreshGoogleListsAsync();
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Google Tasks auth failed during init; Drive sync will continue. Reconnect required.");
        SetReconnectRequired(true);
    }

    StartPeriodicSync();   // reached even when Tasks auth failed
}

// ...
catch (Exception ex) when (ex is not UnauthorizedAccessException)
{
    _logger.LogWarning(ex, "Cloud sync init attempt {Attempt} failed", attempt + 1);
    if (attempt < 2) await Task.Delay(500);
}
```

With Fix 1 in place, the 401 usually self-heals via silent re-auth, so
`RefreshGoogleListsAsync` typically no longer throws. Fix 2 is defense-in-depth: if Tasks
auth still fails, Drive periodic sync still starts and init no longer does 3 futile retries.

Test: `InitializeAsync_TasksRefreshUnauthorized_StillInitializesWithoutRetry` — Tasks throw
UAE, asserts `IsInitialized == true`, `RefreshGoogleListsAsync` called once, and Drive init
called once (no 3× retry).

### Fix 3 — surface a reconnect prompt  ✅ implemented

A `bool ReconnectRequired` flag on `ICloudSyncService` is set when auth fails (init
silent-auth failure, init Tasks `UnauthorizedAccessException`, and the `SyncNowAsync` UAE
catch) and cleared on `ConnectAsync`/`DisconnectAsync` success. `CloudSyncSettings.razor`
shows a "Google session expired — reconnect" banner + button (calls `ConnectAsync(ClientId)`)
when the flag is true. `NotifyStatusChanged()` fires on every change so the UI updates.

Tests: `SyncNowAsync_WhenUnauthorizedAccessException_SetsReconnectRequired`,
`SyncNowAsync_OnSuccessfulSyncAfterFailure_ClearsReconnectRequired`,
`InitializeAsync_TasksRefreshUnauthorized_StillInitializesWithoutRetry`.

**Post-review guard:** the flag is *also* cleared whenever a sync succeeds
(`SyncNowAsync` delegates to `ResolveSyncAsync`; on `result.Success` it calls
`SetReconnectRequired(false)`). Without this, a silent token recovery (the GIS client
refreshing on its own, or `GoogleTasksService`'s 401 re-auth succeeding) would leave a
stale "Session expired" banner while syncs were actually completing.

### Fix 4 (design) — stop persisting short-lived access tokens  ✅ implemented

Persisting `syncState.AccessToken` across sessions is the underlying anti-pattern: GIS
access tokens are ~1h and expire between sessions. Startup now calls
`TrySilentAuthAsync()` to mint a fresh token rather than restoring a stored bearer that is
usually already dead. `ConnectAsync` persists state with `SaveSyncStateAsync(connected: true)`
(no token), and the dead `accessToken` parameter plus the `SyncStateRecord.AccessToken`
field were removed entirely (no migration needed — JSON deserialization tolerates the
extra field in existing persisted records). This also orphaned `SetAccessTokenAsync`
(its sole caller was the removed restore), so the interface/impl method, the
`googleDrive.setAccessToken` JS function, and the `SetAccessToken` interop constant were
removed too. On silent-auth failure at startup the flag from Fix 3 prompts reconnect.

## Confirmed root cause #2 (runtime trace, 403) — local tasks hidden by a stale `CurrentListId`  ✅ fixed

This is the actual **"no tasks showing"** symptom (active "Tasks" tab, coral dot, count **11**,
empty list, no error) — distinct from the 401 storm above, and **not** the `ToDictionary`
theory (Appendix A): the active list is the *local* Pomodoro list, which never touches the
sidecar.

The badge and the local-list body use the **identical** predicate
(`!IsGoogleTask && !IsScheduled && !IsDeleted`): the count at `TaskService.cs:40` and the
filter at `TaskService.cs:436`. So they cannot diverge by filtering — the only way to get
badge 11 / list 0 is that the rendered list was fetched for a **different list id** than the
badge's local list.

```text
Prior session selected a Google list -> _appState.CurrentListId persisted (AppStateRecord)
  -> next startup restores that Google list id (TaskService.cs:94-96)
  -> Google pull fails: 401 then 403 (scope) -> GetTaskListsAsync throws at TaskService.cs:483
  -> the dangling-CurrentListId reset guard (was at :513-520) sits AFTER :483 inside the try,
     so it never runs; _cachedGoogleLists stays empty
  -> presenter resolves listId = ActiveListId(null) ?? CurrentListId(dead Google id) ?? local
     = dead Google id
  -> GetTasksForListAsync(deadId) filters GoogleListId==deadId -> 0 tasks, no throw
  -> Index sets Tasks = [] (snapshot) while TaskLists local count = 11 (live)
  -> badge 11, list empty, no error
```

**Fix 5 (primary for this symptom)** — sanitize `CurrentListId` regardless of Google-pull
outcome. Extracted `EnsureCurrentListSelectableAsync()` (resets a Google list id absent from
`_cachedGoogleLists` back to the local list) and call it:
- in the not-connected early-return branch,
- in place of the old inline guard on successful refresh,
- in a **`finally`** on the refresh `try`, so 401/403/offline/propagated-UAE all still
  correct the selection.

```csharp
finally
{
    await EnsureCurrentListSelectableAsync(); // runs even when the Google pull throws
}

private async Task EnsureCurrentListSelectableAsync()
{
    var current = _appState.CurrentListId;
    if (!string.IsNullOrEmpty(current) &&
        current != Constants.TaskLists.LocalPomodoroListId &&
        current != Constants.TaskLists.ScheduleListId &&
        !_cachedGoogleLists.Any(l => l.Id == current))
    {
        await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
    }
}
```

`SelectListAsync` persists the corrected id and fires `NotifyStateChanged`, so the home page
re-runs `UpdateStateAsync` and renders the 11 local tasks. Build clean.

### The 403 itself is configuration, not a code bug

The scope string already requests full tasks scope
(`googleDrive.js:32`: `…/auth/drive.appdata …/auth/tasks openid email`), yet the call returns
**403** after a successful silent re-auth. Silent auth (`prompt: none`) **cannot widen an
existing grant**, so either:
- the user's existing OAuth grant predates the `auth/tasks` scope → **Disconnect + reconnect**
  (full consent) to add it; or
- the **Google Tasks API is not enabled** in the Cloud Console project → enable it.

Fix 5 makes the app usable (local tasks render) regardless; resolving the 403 is what
re-enables Google lists.

### Fix 6 — collapse a dead list id at the render boundary (Fix 5 alone was insufficient)  ✅ fixed

Fix 5 corrected the **persisted** `_appState.CurrentListId`, but the page still rendered
empty because the **page's `ActiveListId` latched the dead Google id and overrode the
correction**:

```text
IndexPagePresenterService resolves: listId = currentListId ?? CurrentListId ?? local
Index.razor.cs:265 passes ActiveListId as currentListId, and :275 sets ActiveListId = state.CurrentListId
  -> once ActiveListId == deadGoogleId, it WINS over the Fix 5-corrected _appState.CurrentListId
  -> every refresh re-fetches the dead list -> GetTasksForListAsync(deadId) -> 0 tasks (no throw)
  -> task-items stays empty while the local badge counts 11 (live getter)
```

So the dead id was sticky: even clicking didn't help because the resolver kept preferring the
latched `ActiveListId`.

Fix: collapse any id that isn't a real list to the local list, at the render boundary, so both
the fetched tasks **and** the echoed `CurrentListId` (hence `ActiveListId`) revert to local —
breaking the latch.

`src/Pomodoro.Web/Services/IndexPagePresenterService.cs` (`UpdateStateAsync`):

```csharp
var requested = currentListId ?? taskService.CurrentListId ?? Constants.TaskLists.LocalPomodoroListId;
var taskLists = taskService.TaskLists;               // always: local + schedule + cached Google lists
var listId = taskLists.Any(l => l.Id == requested)   // dead Google id is not a member
    ? requested
    : Constants.TaskLists.LocalPomodoroListId;
var tasks = await taskService.GetTasksForListAsync(listId);
```

Belt-and-suspenders in `src/Pomodoro.Web/Services/TaskService.cs` (`GetTasksForListAsync`): a
non-local/non-schedule id absent from `_cachedGoogleLists` is collapsed to the local list
before filtering, so no caller can bind to an empty dead list while local tasks exist.

Build clean. With Fix 6, `ActiveListId` re-latches to the local list, the local tab highlights,
and the 11 local tasks render — independent of the Google 401/403.

## Verification

1. **Unit tests**
   - `RefreshGoogleListsAsync`: when `CurrentListId` is a Google id and the pull throws
     (401/403) or the user is disconnected, `CurrentListId` resets to the local list
     (`EnsureCurrentListSelectableAsync` via the `finally`).
   - `ExecuteWithRetryAsync`: on 401 with `TrySilentAuthAsync()==true`, refreshes token and
     succeeds on retry; on 401 with silent-auth false, throws `UnauthorizedAccessException` once.
   - `InitializeAsync`: on `UnauthorizedAccessException` sets reconnect state, does **not**
     loop 3×, and still starts Drive periodic sync.
   - `RefreshGoogleListsAsync`: 401 degrades to cached/local without aborting init.
   - Run: `dotnet test tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj`
2. **Manual**: connect, let the token expire (or revoke server-side), reload. Expect one
   silent re-auth and tasks render — no 401 storm, no "init attempt 3 failed".
3. **Format / coverage**: `dotnet format Pomodoro.sln --verify-no-changes`; keep ≥99.5% line coverage.

## RESOLVED (2026-06-28): rebuild + restart fixed it — root trigger was a stale build (R0)

After applying Fix 7 and **restarting the running app** at `localhost:7025`, the list renders
correctly. This confirms the post-Fix-5/6 persistence was **R0: the dev process was serving
the pre-fix compiled assembly.** Key facts that made this the culprit:

- The dev service worker (`wwwroot/service-worker.js`) explicitly **skips `/_framework/`**
  (line 119: `request.url.includes('/_framework/') → fetch fresh`), so a browser refresh always
  fetches the latest WASM/dll **from the server** — but the *server process* must itself be
  serving the new binary. A `dotnet build` from a side terminal does not restart a separate
  `dotnet run`/`dotnet watch` already bound to `:7025`; that process keeps executing the old dll.
- The observed DOM (local "Tasks" tab `aria-selected` + empty list + live badge) is *exactly*
  the **pre-Fix-6** dead-id state (presenter fetched `ActiveListId=<dead id>` → 0 tasks; the tab
  highlighted local via `EffectiveCurrentListId` fallback). With Fix 6 compiled in, the first
  `UpdateStateAsync` collapses the dead id → local and self-heals — so the symptom is
  impossible on a fresh build, which is why only a restart cleared it.

**Takeaway / repro-avoidance:** after editing C#, restart the process bound to the port (not
just `dotnet build`), and hard-reload / unregister the SW to drop a cached `index.html`.
Fix 7 (below) remains a real, kept fix: it guarantees a manual tab click can always recover if
any future stale-selection state recurs.

### Fix 8 — isolate Tasks auth failure in `ConnectAsync` (TC4)  ✅ fixed

A residual gap on the **Connect** path (manual test TC4): `ConnectAsync` called
`_taskService.RefreshGoogleListsAsync()` directly, so a Tasks-only **403 scope gap** (or a 401
after the Fix 1 retry is exhausted) threw `UnauthorizedAccessException` into `ConnectAsync`'s
generic catch, which logged a misleading "Google Drive authentication failed" and **returned
false — aborting the entire connect, including Drive**, even though Drive's own scope was valid.

Fix: wrap the Tasks refresh in its own `try/catch (UnauthorizedAccessException)` that logs a
warning and sets `ReconnectRequired(true)`, then lets the connect succeed (`return true`). It is
placed **after** `SyncNowAsync` so the Drive success path (which clears `ReconnectRequired`)
does not wipe the Tasks-set flag. This mirrors the Fix 2 isolation already applied to
`InitializeAsync`. The 403 scope gap itself remains a **configuration** issue (revoke access +
reconnect for the `auth/tasks` scope, or enable the Google Tasks API in GCP); Fix 8 only stops
it from killing the Drive connect and routes the user to the existing reconnect banner.

### Fix 9 — disconnected user falls back to local list on startup  ✅ fixed

When Google Tasks is disconnected (never connected, revoked, or 401 on token refresh), a
persisted `CurrentListId` pointing to a Google list becomes stale — the list no longer exists in
`_cachedGoogleLists`. `EnsureCurrentListSelectableAsync` (Fix 5) only runs in the connected
branch; the disconnected early-return never corrected the selection.

Fix: added `EnsureLocalListSelectedAsync()` in the disconnected `else` branch of
`TaskService.InitializeAsync`. When `_appState.CurrentListId` is a non-local, non-schedule
Google list id, it resets to the local Pomodoro list. Connected users with a valid Google list
selected are unaffected (the `else` branch only runs when not connected).

`src/Pomodoro.Web/Services/TaskService.cs`:

```csharp
// In InitializeAsync, else branch (not connected):
if (!string.IsNullOrEmpty(_appState.CurrentListId) &&
    _appState.CurrentListId != Constants.TaskLists.LocalPomodoroListId &&
    _appState.CurrentListId != Constants.TaskLists.ScheduleListId)
{
    await SelectListAsync(Constants.TaskLists.LocalPomodoroListId);
}
```

Tests: `InitializeAsync_NotConnected_NonLocalListId_FallsBackToLocal` added to
`TaskServiceMultiListTests.cs`. Existing `InitializeAsync_RestoresCurrentListId` updated to
use the local list id.

### Fix 10 — presenter prefers service `CurrentListId` over stale page parameter  ✅ fixed

`IndexPagePresenterService.UpdateStateAsync` resolved the active list via
`currentListId ?? taskService.CurrentListId ?? local`. When `OnTaskServiceChanged` fires
`UpdateStateAsync` via fire-and-forget `SafeAsync`, the passed `currentListId` is the **page's
`ActiveListId`** — which may be stale if a concurrent `SelectListAsync` has already updated the
service property. The page parameter wins and clobbers the service's corrected value, causing
the wrong tab to highlight.

Fix: swap precedence to `taskService.CurrentListId ?? currentListId ?? local`. `SelectListAsync`
sets `_appState.CurrentListId` synchronously before calling `NotifyStateChanged`, so the
fire-and-forget handler always sees the fresh service value. The page parameter only wins as
fallback when the service hasn't been updated yet.

`src/Pomodoro.Web/Services/IndexPagePresenterService.cs`:

```csharp
var requested = taskService.CurrentListId ?? currentListId ?? Constants.TaskLists.LocalPomodoroListId;
```

Test: `UpdateStateAsync_PrefersServiceCurrentListId_OverStalePassedId` added to
`IndexPagePresenterServiceTests.cs`.

### Fix 11 — task section alignment (12px inset)  ✅ fixed

`TaskListTabs` and `TaskListSyncStrip` had no horizontal margin, causing them to align to the
viewport edge while `timer-card`, `mode-tabs`, and `task-card` all use `margin: 0 12px`. The
misalignment made the task section look disconnected from the rest of the page.

Fix: added `margin: 0 12px` to both components:

- `src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor.css`: `.ltabs { margin: 0 12px 8px; }`
- `src/Pomodoro.Web/Components/Tasks/TaskListSyncStrip.razor.css`: `.sync-strip { margin: 0 12px 6px; }`

### Fix 12 — tab highlight uses list's own color as accent  ✅ fixed

The active tab indicator used a fixed `var(--pomodoro-color)` (red) for all tabs. This only
looked coherent on the Tasks tab (red dot + red accent). On Schedule (yellow dot) and Google
list tabs (variable dot colors), the red accent was visually disjointed and didn't read as
"selected."

Fix: pass each list's color as a CSS custom property (`--list-color`) on the active button
via inline style, and reference it in the CSS:

`src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor`:

```html
<button class="lt @(isActive ? "act" : "")"
        style="@(isActive ? $"--list-color: {list.Color}" : "")">
```

`src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor.css`:

```css
.lt.act {
    color: var(--color-text-primary);
    font-weight: 600;
    border-bottom-color: var(--list-color);
}
```

Each tab's active accent now matches its dot color — red for Tasks, yellow for Schedule,
and the list's own color for Google tabs. Consistent with the mode-tabs pattern
(`button.active` uses timer-mode-specific border colors).

### Fix 13 — stale fire-and-forget `UpdateStateAsync` overwrites `ActiveListId`  ✅ fixed

`OnTaskServiceChanged` calls `UpdateStateAsync` via fire-and-forget `SafeAsync`
(`Index.razor.Events.cs:20`). Multiple in-flight calls captured the page's `ActiveListId` at
their start; a late-completing older call could overwrite the fields that a newer click had
already set, leaving the wrong tab highlighted until the next action.

Fix: a monotonic sequence counter `_updateSeq` in `IndexBase`. Each `UpdateStateAsync` captures
`seq = ++_updateSeq` before the await, and after the await does `if (seq != _updateSeq) return;`
so only the most-recently-started call's result is applied. WASM is single-threaded (interleaving
only at awaits), so the increment/compare is race-free without a lock.

`src/Pomodoro.Web/Pages/Index.razor.cs`:

```csharp
private int _updateSeq;

protected async Task UpdateStateAsync()
{
    try
    {
        var seq = ++_updateSeq;
        var state = await IndexPagePresenterService.UpdateStateAsync(...);

        if (seq != _updateSeq) return;   // a newer update started; discard this stale result

        Tasks = state.Tasks;
        // ... apply fields ...
    }
    ...
}
```

This is the primary guard against re-entrant overwrites; Fix 10 (presenter precedence swap)
remains as defense-in-depth.

## Re-investigation (2026-06-28): why the symptom persisted after Fix 5/6 (superseded by RESOLVED above)

Reported at runtime (localhost:7025): "Tasks" tab is active, coral dot + badge shows **12**,
but the list is empty, and clicking the Tasks tab shows nothing. A full static re-trace of the
current code shows **Fix 5 + Fix 6 cannot produce this on the local tab**, so the persistence
has one of two explanations below — one is environmental, one is a real residual bug.

### Finding R0 — the local path is provably self-consistent (so this is NOT a filter bug)

- Badge count (`TaskService.TaskLists` getter, `TaskService.cs:40`):
  `allTasks.Count(t => !t.IsGoogleTask && !t.IsScheduled && !t.IsDeleted)`
- Local-list fetch (`GetTasksForListAsync`, `TaskService.cs:443`):
  `allTasks.Where(t => !t.IsGoogleTask && !t.IsScheduled && !t.IsDeleted)`

These are **byte-identical**. Plus `IsKnownList` (`:677`) collapses any non-local /
non-schedule / non-cached-Google id to local, and `TaskService.InitializeAsync` fires
`NotifyStateChanged()` (`:110`) → `OnTaskServiceChanged` → `UpdateStateAsync`, so the snapshot
refreshes after load. Therefore, **with the uncommitted Fix 5/6 compiled in, badge-12 /
list-empty on the local tab is impossible.**

> **Most likely cause of the persistence: the running build is stale.** This is a PWA
> (`wwwroot/` service worker + JS bundle). The uncommitted fixes only take effect if the app
> is rebuilt AND the browser isn't serving a cached bundle. Confirm:
> 1. Stop the server, `dotnet build` (0 errors), then run the freshly built app — not a
>    previously published artifact or an old `dotnet run`/`dotnet watch` that didn't pick up
>    edits.
> 2. Hard-reload / unregister the service worker (DevTools → Application → Service Workers →
>    Unregister; "Disable cache" on Network) to drop the cached WASM/JS bundle.
> 3. After that, if the symptom is gone → it was the stale bundle. If it remains → see R1/R2.

### Finding R1 — RESIDUAL BUG (✅ FIXED, Fix 7): the Tasks-tab click was a no-op when `ActiveListId` is stale

`TaskListTabs.razor` (`:36-52`):

```csharp
private string? EffectiveCurrentListId
{
    get
    {
        if (CurrentListId != null && VisibleLists.Any(l => l.Id == CurrentListId))
            return CurrentListId;
        return VisibleLists.FirstOrDefault()?.Id;   // falls back to the local list
    }
}

protected async Task HandleTabClick(string listId)
{
    if (listId != EffectiveCurrentListId)          // <-- the guard
        await OnTabChanged.InvokeAsync(listId);
}
```

When `ActiveListId` (= `CurrentListId`) is a **dead Google id**, it is not in `VisibleLists`,
so `EffectiveCurrentListId` falls back to the **local "Tasks" list** (first visible). Consequence:
the local tab renders as active (badge 12) and `HandleTabClick(local)` computes
`local(local) != EffectiveCurrentListId(local)` → **false → `OnTabChanged` never fires**. The
click is swallowed, `HandleTabChange` / `UpdateStateAsync` never run, and the empty `Tasks`
snapshot is never refreshed. This is exactly the reported "click the Tasks tab → nothing."

Fix 6 does **not** cover this: the collapse lives inside `UpdateStateAsync`, which the dead
click never invokes. So once the simultaneous state `ActiveListId=<dead id>` **and**
`Tasks=<stale empty>` exists, the user cannot click their way out.

**Fix 7 (applied) — click-suppression compares the real id, not the fallback.**
`src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor` (`HandleTabClick`):

```csharp
protected async Task HandleTabClick(string listId)
{
    if (listId != CurrentListId)        // was: EffectiveCurrentListId (the fallback)
        await OnTabChanged.InvokeAsync(listId);
}
```

`EffectiveCurrentListId` still drives the highlighted/active tab; only the click-suppression
now uses the real `CurrentListId`. So when `CurrentListId` is a dead id, clicking the
fallback (local) tab is no longer swallowed — `OnTabChanged` fires → `HandleTabChange` →
`UpdateStateAsync` runs, Fix 6 collapses the id to local, and the 11/12 local tasks render.
This gives the user a guaranteed manual recovery even if any future stale-selection state
recurs. Build clean. **Status: fixed.**

### Finding R2 — RESIDUAL: snapshot `Tasks` vs live badge refresh out of sync

The badge (`TaskLists` getter) is recomputed on **every** render from live `_appState.Tasks`,
but `Tasks` (the rendered list) is a **snapshot** set only inside `UpdateStateAsync`
(`Index.razor.cs:267`). Any render that happens in the window between a `_appState.Tasks`
mutation and the `NotifyStateChanged`-driven `UpdateStateAsync` (e.g. a timer tick calling
`StateHasChanged`) shows the badge updated to 12 while the list still holds the old (possibly
empty) snapshot. Transient, but it is the structural reason badge and list can ever disagree
even with identical predicates. **Status: latent; mitigated by R0/R1.**

### Net assessment

Fix 5/6 make the **code** correct and unit-test-green; **Fix 7 (now applied)** closes the R1
dead-click so a manual tab click always recovers. With all of Fix 5/6/7 compiled in, the
local-tab path is provably self-consistent — so if the symptom **still** reproduces, the cause
is **R0: a stale PWA build/bundle** (the browser is serving a cached WASM/JS bundle or the
server is running an old artifact). Required next step: rebuild and force the browser to drop
the cache:

1. Stop the server, `dotnet build` (0 errors), run the **freshly built** app (not an old
   `dotnet run`/published artifact).
2. DevTools → Application → Service Workers → **Unregister**; Network → **Disable cache**;
   then hard-reload.
3. If the list now renders → it was the stale bundle (R0). If it persists after a verified
   clean rebuild + SW unregister, capture the console + the `CurrentListId` value and re-open —
   the code paths for the local tab can no longer produce badge≠list.

## Files touched

- `src/Pomodoro.Web/Services/GoogleTasksService.cs` (Fix 1)
- `src/Pomodoro.Web/Services/CloudSyncService.cs` (Fix 2, reconnect state, Fix 8)
- `src/Pomodoro.Web/Services/TaskService.cs` (Fix 5 — `EnsureCurrentListSelectableAsync` + `finally`; Fix 6 — `GetTasksForListAsync` collapse; Fix 9 — disconnected fallback)
- `src/Pomodoro.Web/Services/IndexPagePresenterService.cs` (Fix 6 — render-boundary collapse; rethrows instead of swallowing; Fix 10 — presenter precedence swap)
- `src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor` (Fix 7 — click-suppression compares real `CurrentListId`, not the fallback; Fix 12 — `--list-color` inline style)
- `src/Pomodoro.Web/Components/Tasks/TaskListTabs.razor.css` (Fix 12 — border-bottom uses `var(--list-color)`)
- `src/Pomodoro.Web/Components/Tasks/TaskListSyncStrip.razor.css` (Fix 11 — 12px inset)
- `src/Pomodoro.Web/Pages/Index.razor.cs` (Fix 3 reconnect banner; Fix 13 — `_updateSeq` stale-call guard in `UpdateStateAsync`) + `CloudSyncSettings.razor` (Fix 3 reconnect banner)
- `src/Pomodoro.Web/Services/IGoogleDriveService.cs` / `GoogleDriveService.cs` / `Constants.JsInterop.cs` / `wwwroot/js/googleDrive.js` (token persistence — store/restore access token with 55-min expiry guard; `SyncStateRecord` fields `AccessToken`/`TokenExpiresAt`)
- `tests/Pomodoro.Web.Tests/Services/GoogleTasksServiceTests.cs`
- `tests/Pomodoro.Web.Tests/Services/CloudSyncServiceTests*.cs` (token-restore cases)
- `tests/Pomodoro.Web.Tests/Services/TaskServiceMultiListTests.cs` (Fix 9)
- `tests/Pomodoro.Web.Tests/Services/IndexPagePresenterServiceTests.cs` (Fix 10)
- `tests/Pomodoro.Web.Tests/Services/GoogleDriveServiceTests.cs` (token persistence)

---

## Appendix A — badge ≠ list via `ToDictionary` crash (prior theory; NOT this trace)

An earlier draft blamed `GetSidecarCacheAsync` `ToDictionary(m => m.GoogleTaskId)`
(`TaskService.cs:960`) throwing on null/duplicate keys, swallowed by
`IndexPagePresenterService` (`:36-44`) to produce "badge 11, list empty, no error". The
runtime trace shows a different failure (a 401 propagating out of `InitializeAsync`), so this
is not the active cause. The `ToDictionary` hardening is still worthwhile defensively:

```csharp
_sidecarCache = allMeta
    .Where(m => !string.IsNullOrEmpty(m.GoogleTaskId))
    .GroupBy(m => m.GoogleTaskId!)
    .ToDictionary(g => g.Key, g => g.Last());
```

## Appendix B — orphan-delete (real, unrelated bug)

`RefreshGoogleListsAsync` (`TaskService.cs:626`) once treated a stored task with `GoogleListId`
set but `GoogleTaskId == null` as an orphan and soft-deleted it every sync. The
`t.GoogleTaskId != null` guard fixes it. This drops badge **and** list together (shared
`!IsDeleted` predicate), so it cannot cause badge ≠ list and is unrelated to the 401 trace.
Status: guard applied.

## Appendix C — scope gap (only if the error is 403, not 401)

Existing Drive-only users who never consented `tasks.readonly` get **403** (handled at
`GoogleTasksService.cs:175` → `TasksAccessForbidden`). That needs a re-Connect/re-consent
prompt, not silent re-auth. Not the current trace (which is 401), but watch for it during
the Phase 0 scope widen.
