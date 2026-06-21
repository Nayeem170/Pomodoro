# Phase 0 — Auth Scope

## Goal

Add Google Tasks API read-only scope to the existing OAuth flow and surface the connected user's email.

## Prerequisites

- Existing GIS token flow via `googleDrive.js` + `GoogleDriveService` (401 → reconnect)
- `GoogleDriveJsFunctions` constants (`Constants.JsInterop.cs:110-123`)
- `CloudSyncSettings.razor` (Connect/Sync/Disconnect UI)

## Steps

### 1. Add scopes to GIS init

**File:** `wwwroot/js/googleDrive.js:32`

Append to the existing single space-separated scope string in `initTokenClient`:
```
scope: 'https://www.googleapis.com/auth/drive.appdata https://www.googleapis.com/auth/tasks.readonly openid email'
```

Three scopes in one `initTokenClient` call — do NOT create a second token client. GIS will request consent for all three in one popup.

**Rationale (review round 2):** `initTokenClient` returns `access_token` only, no `id_token`. The `openid email` scope enables `GET https://www.googleapis.com/oauth2/v3/userinfo` for the email address.

### 2. Add constants

**File:** `Constants/Constants.Sync.cs`

```
TasksScope = "https://www.googleapis.com/auth/tasks.readonly"
TasksScopeReadWrite = "https://www.googleapis.com/auth/tasks"   // Phase 3
UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo"
```

**File:** `Constants/Constants.JsInterop.cs`

Add `GoogleTasksJsFunctions` class mirroring `GoogleDriveJsFunctions`:
```
class GoogleTasksJsFunctions { const string GetUserInfo = "getGoogleUserInfo"; }
```

### 3. Add `getGoogleUserInfo()` to `googleDrive.js`

**File:** `wwwroot/js/googleDrive.js`

New function:
```js
window.getGoogleUserInfo = async function(accessToken) {
  const resp = await fetch('https://www.googleapis.com/oauth2/v3/userinfo', {
    headers: { Authorization: 'Bearer ' + accessToken }
  });
  if (!resp.ok) throw new Error('Failed to fetch user info: ' + resp.status);
  return await resp.json();
};
```

### 4. Surface email in `GoogleDriveService`

**File:** `Services/GoogleDriveService.cs`

- Add `string? AccountEmail { get; }` property
- On `ConnectAsync` success, call `getGoogleUserInfo(accessToken)` and store email
- On `DisconnectAsync`, clear email
- Persist email in `SyncStateRecord` (`CloudSyncService.cs:478-486`)

### 5. Re-auth UX for existing users

Changing scope invalidates existing sessions. On startup:

- If a stored token lacks the tasks scope → treat as not-connected for Tasks features
- Try `TrySilentAuth` first
- Fall back to interactive Connect prompt
- Drive sync keeps working with the old token until re-auth

### 6. Update `CloudSyncSettings.razor`

**File:** `Components/Settings/CloudSyncSettings.razor`

When connected, show: `Connected · {email}` (instead of just "Connected")

### 7. Add new constants/messages

**File:** `Constants/Constants.Sync.cs`

```
SyncMessages.LogTasksScopeMissing = "Tasks scope missing — re-auth required"
```

## Change sites

| File | Change |
|------|--------|
| `wwwroot/js/googleDrive.js:32` | Append scopes to string |
| `wwwroot/js/googleDrive.js` (new function) | Add `getGoogleUserInfo` |
| `Constants/Constants.Sync.cs` | Add TasksScope, UserInfoEndpoint, messages |
| `Constants/Constants.JsInterop.cs` | Add `GoogleTasksJsFunctions` |
| `Services/GoogleDriveService.cs` | Add `AccountEmail` property, fetch on connect |
| `Components/Settings/CloudSyncSettings.razor` | Display email when connected |
| `Services/CloudSyncService.cs` (`SyncStateRecord` `:478`) | Persist email |

## Tests

- Unit: `GoogleDriveService` connect sets email; disconnect clears email; userinfo call failure handled
- Unit: re-auth prompt triggers when scope missing
- E2E: (mocked) connect flow shows email

## Risks

- Existing users must re-authenticate — scope change invalidates current tokens
- `userinfo` endpoint is an extra network call on connect — add timeout
