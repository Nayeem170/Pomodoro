# Phase 5 — Tests + Rollout

## Goal

Comprehensive test coverage for all Google Tasks features, behind a connected-state gate so local Pomodoro is unaffected when disconnected.

## Prerequisites

- Phases 0–4 complete
- Existing test infrastructure: xUnit, bUnit, Moq, FluentAssertions, Playwright

## Unit Tests

### Service tests

| Test file | Covers |
|-----------|--------|
| `GoogleTasksServiceTests.cs` | Phase 1 — get lists, get tasks, pagination, 401/403/429 error handling |
| `GoogleTasksSyncServiceTests.Pull.cs` | Phase 3 — remote new task upserts, remote update merges, remote delete soft-deletes local |
| `GoogleTasksSyncServiceTests.Push.cs` | Phase 3 — local new task creates on Google, local dirty patches, local delete pushes |
| `GoogleTasksSyncServiceTests.Conflict.cs` | Phase 3 — last-writer-wins by timestamp, ETag mismatch → remote wins, same timestamp → local wins |
| `GoogleTasksSyncServiceTests.PartialFailure.cs` | Phase 3 — some lists sync, failed lists reported |
| `PomodoroMetaRepositoryTests.cs` | Phase 2 — sidecar CRUD, keyed by GoogleTaskId |

### Model tests

| Test file | Covers |
|-----------|--------|
| `TaskItemGoogleTests.cs` | Phase 2 — `IsGoogleTask`, `WithUpdates` copies all fields, `Priority` default |
| `TaskListRefTests.cs` | Phase 2 — computed properties |
| `GoogleTasksSettingsTests.cs` | Phase 2 — list visibility/color persistence |

### TaskService tests (additions)

| Test file | Covers |
|-----------|--------|
| `TaskServiceTests.Google.cs` (new) | Phase 2 — `AddTimeToTaskAsync` branches (sidecar vs TaskItem), `GetTasksForListAsync` |
| `TaskServiceTests.AppState.cs` (additions) | Phase 2 — `CurrentListId` persist/restore |

### Export/Import tests (additions)

| Test file | Covers |
|-----------|--------|
| `ExportServiceTests.Google.cs` (new) | Phase 2 — version 2 export includes sidecar, `[JsonIgnore]` strips Google IDs |
| `ImportServiceTests.Google.cs` (new) | Phase 2 — version 2 import reads sidecar, orphan sidecar cleanup, back-compat version 1 |

### Component tests

| Test file | Covers |
|-----------|--------|
| `TaskListTabsTests.cs` (new) | Phase 4 — renders tabs, selects tab, hides Google tabs when disconnected |
| `TaskListSyncStripTests.cs` (new) | Phase 4 — shows list name, sync status, stale indicator |
| `TaskEditPanelGoogleTests.cs` (new) | Phase 4 — priority/due/notes fields disabled for Google tasks (first cut), enabled for local |
| `GoogleConnectionSettingsTests.cs` (new) | Phase 4 — Connect/Disconnect + email display |
| `GoogleListTogglesTests.cs` (new) | Phase 4 — list toggles with color dots, visibility toggle updates settings |

### ConsentService/notification tests (additions)

| Test file | Covers |
|-----------|--------|
| `ConsentServiceTests.AutoStartListContext.cs` (new) | Phase 4 — auto-start carries `CurrentListId`, switches tab |

## E2E Tests

| Test file | Covers |
|-----------|--------|
| `google-tasks-connect.spec.ts` (new) | Connect flow → email displays → list tabs appear |
| `google-tasks-list-tabs.spec.ts` (new) | Tab selection → filtered tasks per list → Pomodoro tab unaffected |
| `google-tasks-sync-strip.spec.ts` (new) | Sync strip shows list name, read-only status |
| `google-tasks-task-edit.spec.ts` (new) | Edit local task → fields enabled; edit Google task → fields disabled (first cut) |
| `google-tasks-settings.spec.ts` (new) | Visibility toggles → list hidden/shown |

**E2E fixture additions** (`tests/e2e/fixtures/pomodoro.page.ts`):
```
selectListTab(listName)
getActiveListTab()
hasListTab(listName)
isListTabHidden(listName)
```

## Coverage Requirements

- Maintain 98% line and branch coverage threshold (CI enforced)
- New Google Tasks code must match this threshold
- No coverage drop from existing features

## Rollout Strategy

### Connected-state gate

All Google Tasks features are behind `IGoogleTasksService.IsConnectedAsync()`:

- **Not connected:** local Pomodoro list works exactly as today — no changes, no new UI elements
- **Connected:** Google list tabs appear, sync strip shows, extended edit fields visible
- **Disconnected mid-session:** Google tabs persist (cached data shown, flagged stale per review round 1), Drive sync unaffected

### Feature flags (optional)

Consider a `GoogleTasksEnabled` setting in `TimerSettings` to allow gradual rollout. Default: `true` when connected.

### Backward compatibility

- Existing IndexedDB data (v1) migrates to v2 additively — no data loss
- Existing Drive sync envelopes without sidecar data read fine (back-compat in `PullAsync`)
- Local Pomodoro list is completely unaffected when not connected
- No breaking changes to existing APIs — all additions are additive

## Change sites

| File | Change |
|------|--------|
| `tests/Pomodoro.Web.Tests/Services/GoogleTasksServiceTests.cs` (new) | Phase 1 service tests |
| `tests/Pomodoro.Web.Tests/Services/GoogleTasksSyncServiceTests.*.cs` (new) | Phase 3 sync tests |
| `tests/Pomodoro.Web.Tests/Services/PomodoroMetaRepositoryTests.cs` (new) | Sidecar repo tests |
| `tests/Pomodoro.Web.Tests/Components/Tasks/TaskListTabsTests.cs` (new) | Tabs component tests |
| `tests/Pomodoro.Web.Tests/Components/Settings/GoogleConnectionSettingsTests.cs` (new) | Connection settings tests |
| `tests/Pomodoro.Web.Tests/Components/Settings/GoogleListTogglesTests.cs` (new) | List toggles tests |
| `tests/e2e/pages/google-tasks-*.spec.ts` (new) | E2E test files |
| `tests/e2e/fixtures/pomodoro.page.ts` | Helper method additions |

## Risks

- E2E tests require mocked Google API responses — need a robust fixture strategy
- Coverage threshold may be tight for sync engine — focus on the reconcile paths
