# Google Tasks Integration — Implementation Plan

Status: planning · Created 2026-06-21 · Source mock: `mock/Pomodoro Task System.html`

## Goal

Integrate Google Tasks into the Pomodoro app: multiple task lists (local Pomodoro + Google Tasks
lists), list tabs in the task panel, per-list 2-way sync, and task fields (priority, due date,
notes, subtasks) per the mock design.

## Mock design summary

From `mock/Pomodoro Task System.html` (artboards `m2` Settings — Sync & Lists, `m1`/`d1` task panel):

- **Task panel list tabs:** `Pomodoro` (coral) | one tab per visible Google Tasks list (list color) | `Schedule` (yellow).
- **Per-list sync strip:** header "Google Tasks · {list name} · 2-way sync".
- **Checkbox color:** list color for Google tasks (blue/green/etc), coral for local Pomodoro tasks.
- **Settings → Sync section (merged):** Google auth (Connect / when connected: email + Sync + Disconnect)
  plus "Show beside Pomodoro" list toggles — each list shows color dot, name, task count, on/off toggle.
- **Task edit:** name, priority (High/Medium/None), due date, repeat, pomodoro stepper, notes, subtasks.

## Current implementation (grounding)

- **OAuth:** Google Identity Services token flow already exists via JS interop
  (`Constants.GoogleDriveJsFunctions.RequestAuth/TrySilentAuth/SetAccessToken/RevokeAuth`).
  Currently Drive scope only.
- **`GoogleDriveService`** wraps Drive appdata REST via JS interop; 401 → `UnauthorizedAccessException`.
- **`CloudSyncService`** syncs a JSON envelope (settings/tasks/history) to a Drive appdata file.
- **`TaskItem`** (`Models/TaskItem.cs`): Id (Guid), Name, CreatedAt, IsCompleted, TotalFocusMinutes,
  PomodoroCount, LastWorkedOn, IsDeleted/DeletedAt (soft delete), Repeat (`RepeatRule`), ScheduledDate.
  No: priority, due date, notes, subtasks, list membership, Google id, etag.
- **Storage:** single local task list in IndexedDB via `ITaskRepository`.

## Key constraint

Google Tasks API stores only: title, notes, due, status, subtasks (via `parent`+`position`).
It has **no** pomodoro count, focus minutes, or priority. Integration is therefore **hybrid**:
Google = source of truth for task content; a local sidecar holds pomodoro metadata keyed by Google task id.

## Decisions (locked)

1. **Sync model:** Google Tasks API syncs tasks/lists. Keep `CloudSyncService` (Drive envelope) for
   settings, history, pomodoro stats, and the pomodoro-metadata sidecar.
2. **Pomodoro-only fields** (focus minutes, pomodoro count, priority): local IndexedDB sidecar keyed by
   Google task id; carried cross-device by the existing Drive-envelope sync.
3. **First deliverable:** read-only pull (connect, list tabs, show Google tasks read-only) before 2-way.

## Phases

| Phase | Title | Doc | Scope |
|-------|-------|-----|-------|
| 0 | Auth Scope | [phase-0-auth.md](google-tasks/phase-0-auth.md) | Add tasks + email scopes, re-auth UX, surface email |
| 1 | API Client | [phase-1-api-client.md](google-tasks/phase-1-api-client.md) | JS interop, C# service, DTOs, error handling |
| 2 | Data Model | [phase-2-data-model.md](google-tasks/phase-2-data-model.md) | TaskItem extension, sidecar, multi-list ITaskService, timer write fix |
| 3 | Sync Engine | [phase-3-sync-engine.md](google-tasks/phase-3-sync-engine.md) | 2-way per-list reconcile, conflict, delete, triggers |
| 4 | UI | [phase-4-ui.md](google-tasks/phase-4-ui.md) | List tabs, sync strip, edit fields, settings split, state pipeline |
| 5 | Tests + Rollout | [phase-5-tests-rollout.md](google-tasks/phase-5-tests-rollout.md) | Unit/E2E coverage, connected-state gate, backward compat |

## First cut (read-only pull) — concrete steps

Phases 0 + 1 + 2 (includes read-only pull, Batch 9) + 4 (read-only UI only). Phase 3 (2-way push/reconcile) and Phase 5 (full tests) deferred.

1. Append `tasks.readonly` + `openid email` to the existing single space-separated scope string in
   `googleDrive.js:32` `initTokenClient`. Add startup scope-check → re-Connect prompt for existing users.
   Email via `oauth2/v3/userinfo` (not id_token — token flow has none).
2. `googleTasks.js`: `listTaskLists()`, `listTasks(listId, pageToken)`. Add `<script>` tag after
   `googleDrive.js` in `index.html:101`. Add `GoogleTasksJsFunctions` + API base/scope constants.
3. `IGoogleTasksService` + `GoogleTasksService` (401 → reconnect; 429 → backoff). Register in `ServiceRegistrationService.cs`; add `TestHelper.cs` mock.
4. DTOs `GoogleTaskList`, `GoogleTask` (read fields).
5. `TaskItem`: add `GoogleTaskId?`, `GoogleListId?`, `UpdatedAt`, `Notes?`, `DueDate?`. **No `[JsonIgnore]`**
   (it would drop the ids from IndexedDB — Blazor serializes all `IIndexedDbService` calls with System.Text.Json);
   scrub Google fields in the export projection only (`ExportData`, Phase 2 Batch 8B). Add `TaskItem.WithUpdates(...)`
   helper; route the 5 copy methods (TaskService + ImportService) through it. `TaskListRef` for tabs;
   `GoogleTasksSettings` (visible/color) via settings repo.
6. `TaskService` exposes lists + per-list tasks; Google tasks read-only (no writes yet). Offline → serve cached, flag stale.
7. UI: `TaskListTabs` (Pomodoro + Google lists), sync strip, list-color checkboxes (write disabled).
   Split `CloudSyncSettings` into `GoogleConnectionSettings` + `GoogleListToggles`.
8. Bump `Constants.Storage.DatabaseVersion` 1→2; add `pomo_meta` store (additive) via a version-gated
   migration block in `indexedDbInterop.js` `onupgradeneeded` (`event.oldVersion < 2` — not inside the
   create-if-missing block, which existing users skip). Sidecar keyed by GoogleTaskId; joined on read; register
   sidecar repo in DI. For cross-device: sidecar rides inside the export JSON (`ExportData.Version=2` with
   back-compat read) — **no `SyncEnvelope.Version` bump needed** (Phase 2 Batch 8D).

Deferred to 2-way phase: push/patch/delete, conflict+ETag, subtasks, priority, debounced auto-sync, scope widen to `auth/tasks`.

## Risks

- Hybrid metadata store (Phase 2) is the architectural crux.
- Subtasks = flat Google tasks with `parent`+`position` ordering — fiddly.
- Token expiry mid-sync → 401 pattern handles; add silent re-auth retry.
- Keep Tasks-API sync and Drive-envelope sync from overlapping on task data (decision 1 separates them).

## Review history

### Round 1 — infrastructure gaps (verified against code 2026-06-21)

Conflicts: scope mechanism, re-auth UX, `TaskItem` 4 copy sites, debounce, export leaks.
Missing: offline handling, settings persistence, IndexedDB migration, DI registration, test mocks, rate limiting, new constants, script load order, account email.
Reusable: `GoogleDriveJsFunctions`, 401 pattern, `SafeTaskRunner`, `CloudSyncSettings`, `SyncStateRecord`, CSP.

### Round 2 — deeper integration

Email mechanism corrected (userinfo, not id_token). Sidecar cross-device via Drive envelope. Pull query params. Per-list sync cursor. Schedule tab scope (local filtered view). Sidecar key strategy. Read-only clarified. DB migration additive. Field mapping caveats.

### Round 3 — deep integration gaps (verified against code 2026-06-21)

Critical: timer sidecar write path, `ImportService` 5th copy site, export/import sidecar + orphan handling.
High: `AppState.CurrentListId`, `ITaskService` interface changes, `ConsentService`/notification list context.
Medium: `ActivityRecord` rename grouping, `TimerCompletedEventArgs` Google context, `ITaskRepository` list queries, `CloudSyncSettings` split, SW no-change, `IndexPagePresenterService` list state.
Low: `CurrentTaskIndicator` filtering, Schedule + Google interaction, JS scope hardcoding, anonymous export type, `DailyStats` Guid portability.

All round 3 gaps are resolved in the phase docs above.
