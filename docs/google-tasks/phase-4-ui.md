# Phase 4 — UI

## Goal

Build the multi-list task panel with list tabs, per-list sync strip, extended task edit fields, and settings integration per the mock design.

## Prerequisites

- Phase 2 complete (data model, `TaskListRef`, multi-list `ITaskService`)
- Phase 1 complete (Google task data available for read-only display)

## Steps

### 1. Wire `GoogleTasksSettings` into `TaskService` (Phase 2↔4 gap — blocker for steps 5 + 6)

**File:** `Services/TaskService.cs` + `Services/Repositories/ISettingsRepository.cs`

`TaskLists` currently hardcodes `Color = "var(--pomodoro-color)"` and `IsVisible = true` for Google lists. The mock design requires per-list colors and visibility toggles. Fix:

1. Add `GoogleTasksSettings` to `ISettingsRepository` load/save pattern (similar to `TimerSettings`).
2. Inject `ISettingsRepository` into `TaskService`.
3. `RefreshGoogleListsAsync` loads settings after pulling lists — merge color/visibility into cached snapshot.

**Color seeding (Google API has no list colors):** Google Tasks API returns no color for task lists. Assign from a palette on first sight:
   - Pre-define a palette: `["#4285F4", "#0B8043", "#E67C73", "#9C27B0", "#F59E0B", "#EC407A", "#AB47BC", "#FF5722", "#795548"]`.
   - On pull, for each Google list not yet in settings, assign `palette[listIndex % palette.Length]` and persist to `GoogleTasksSettings`.
   - User can override via `GoogleListToggles` (Phase 3).

**Cache type:** `_cachedGoogleLists` is `List<GoogleTaskList>` (`{Id, Title}` only) — can't hold color/visibility. Replace with a dedicated `record GoogleListCacheEntry(string Id, string Title, string Color, bool IsVisible)` as the cache type. Derive `TaskLists` property from this cache + local/schedule refs.

**Count is volatile** — don't cache it. `TaskListRef.Count` changes on every add/complete/delete between pulls. The `TaskLists` getter overwrites `Count` live from `_appState.Tasks` (matching current Phase 2 impl at `TaskService.cs:42`). Cached `GoogleListCacheEntry` has no Count field.

4. Add `ITaskService.UpdateListVisibilityAsync(string listId, bool isVisible)`. This persists to `GoogleTasksSettings` via settings repo (no Google API call needed — local pref only). Defer `UpdateListColorAsync` to Phase 3 (colors are auto-assigned in first cut; no color picker UI exists).

### 2. Create `TaskListTabs` component

**File:** `Components/Tasks/TaskListTabs.razor` (new)

Renders tabs: `Pomodoro` (coral) | visible Google lists (per-list color) | `Schedule` (yellow).

- Tabs show: color dot, list name, task count badge
- Active tab: highlighted with list color
- Inject `ITaskService` for `TaskLists`, `CurrentListId`, `SelectListAsync`
- On tab click: call `SelectListAsync(listId)`, update `AppState.CurrentListId`
- Hide Google tabs when not connected (not due to transient network error)
- Schedule tab is a local filtered view — unaffected by Google sync

### 3. Update `IndexPagePresenterService` / `IndexPageState`

**File:** `Services/IndexPagePresenterService.cs`

Current state pipeline is sync (all data available synchronously). `GetTasksForListAsync` is async (awaits sidecar). Ripples:

1. `UpdateState` becomes `async Task<IndexPageState> UpdateStateAsync()`.
2. Update all callers to `await` it — verify no callers hold a reference to the old sync method.
3. Add to `IndexPageState`:

```
class IndexPageState
{
    List<TaskItem> Tasks              // filtered to active tab
    TaskItem? CurrentTask           // new — resolved, list-independent (step 9)
    Guid? CurrentTaskId
    string? CurrentListId
    IReadOnlyList<TaskListRef> TaskLists
}
```

**Per-tick performance — avoid IndexedDB on every timer tick:**

`GetTasksForListAsync` currently awaits `_sidecarRepo.GetAllAsync()` (IndexedDB) on every call. The presenter runs on every timer tick (state changes each second) — that's one IndexedDB read/sec. Fix:

- `TaskService` maintains an in-memory `Dictionary<string, PomodoroMeta> _sidecarCache`, hydrated lazily on first access.
- `InvalidateSidecarCache()` (sync — sets dirty flag / clears dict; re-hydrated on next `GetTasksForListAsync` call).
- `GetTasksForListAsync` reads from `_sidecarCache` instead of calling the repo (pure in-memory filter after hydration).
- Callers that need fresh sidecar data (e.g., after a sync pull) call `InvalidateSidecarCache()` to force re-hydration on next access.

**Invalidation sites** (all in `TaskService` — repo can't reach a private cache):

| Method | When | Why |
|--------|------|-----|
| `AddTimeToTaskAsync` | After `_sidecarRepo.SaveAsync()` | New/updated pomodoro count won't show otherwise |
| `ImportDataAsync` | After sidecar restore (Phase 2 batch 8) | Imported metadata replaces cache |
| `ClearAllAsync` | After `_sidecarRepo.DeleteAllAsync()` | Cache is now empty |

This keeps `UpdateStateAsync` cheap (no I/O) while preserving fresh data after writes/pulls.

`UpdateStateAsync` must:
1. Read `CurrentTaskId` + `CurrentListId` from `AppState`
2. Get `TaskLists` from `ITaskService` (sync — cached)
3. Resolve `CurrentTask` from `AllTasks` by `CurrentTaskId` (not from filtered list — step 9 requirement)
4. Filter tasks to active list via `await GetTasksForListAsync(CurrentListId)` (async surface — pure in-memory after lazy cache hydration)
5. Pass filtered tasks + list data + resolved `CurrentTask` to `IndexBase`

### 4. Update `Index.razor` / `Index.razor.cs`

**File:** `Pages/Index.razor` + partial class files (`Index.razor.Tasks.cs`, `Index.razor.Timer.cs`, `Index.razor.cs`)

- Add `TaskListTabs` above `TaskList` component
- Pass `IndexPageState.TaskLists`, `IndexPageState.CurrentListId` to `TaskListTabs`
- Pass filtered `IndexPageState.Tasks` to `TaskList`
- Pass `IndexPageState.CurrentTask` to `CurrentTaskIndicator` (step 9)
- Wire all `UpdateState` call sites to `await UpdateStateAsync()`

### 5. Checkbox color

**File:** `Components/Tasks/TaskItemComponent.razor`

- Add `[Parameter] string CheckboxColor { get; set; } = "var(--pomodoro-color)"`
- Google tasks: checkbox color = `CheckboxColor` (per-list color from parent)
- Local Pomodoro tasks: checkbox color = `"var(--pomodoro-color)"`
- Parent (`TaskList.razor`) passes `TaskListRef.Color` (or the hard-coded coral) via parameter

### 6. Per-list sync strip

**File:** `Components/Tasks/TaskListSyncStrip.razor` (new)

When on a Google list tab, show: `Google Tasks · {list name} · read-only`.

- Inject `ICloudSyncService` for connection state (not a Phase 3 sync service — doesn't exist in first cut)
- Show stale indicator when disconnected

### 7. Split `CloudSyncSettings` into sub-components

**File:** `Components/Settings/CloudSyncSettings.razor`

Currently 187 lines. Split into:
- `GoogleConnectionSettings.razor` — Connect/Sync/Disconnect + email display
- `GoogleListToggles.razor` — "Show beside Pomodoro" list toggles (color dot, name, count, on/off) + `UpdateListVisibilityAsync` write-back

This follows the existing codebase pattern (`TimerDurationSettings`, `AutomationSettings`, `SoundNotificationSettings`).

`CloudSyncSettings.razor` becomes the container rendering both sub-components.

### 8. Extend `TaskEditPanel`

**File:** `Components/Tasks/TaskEditPanel.razor` + `.razor.cs`

Add fields (per mock):
- **Priority** dropdown: High / Medium / None (local only — not synced to Google for Phase 3)
- **Due date** picker: `<input type="date">` bound to `TaskItem.DueDate`
- **Notes** textarea: bound to `TaskItem.Notes`
- **Subtasks**: deferred to Phase 3 (flat Google tasks with `parent`+`position`)

For first cut (read-only Google): priority/due/notes fields are **disabled** for Google tasks (no writes to Google yet).

### 9. Fix `ConsentService` / notification actions for list context

**File:** `Services/ConsentService.cs:203` (`StartSessionAsync`)

Auto-start reuses `CurrentTaskId` — no list context. Fix:
- Inject `IAppState` or accept `CurrentListId` parameter
- After starting pomodoro, if `CurrentTaskId` resolves to a Google task → call `ITaskService.SelectListAsync` to switch to that task's list

**File:** `Pages/Index.razor.Events.cs:102` (notification action)

Same issue — notification action uses `AppState.CurrentTaskId` directly. Fix:
- After starting pomodoro from notification, switch to the task's list via `SelectListAsync`
- Low priority for first cut (indicator already works across lists via `CurrentTask` param; tab switch is cosmetic)

### 10. Fix `CurrentTaskIndicator` for list-aware filtering

**File:** `Components/Timer/CurrentTaskIndicator.razor`

Currently receives full `Tasks` list and looks up by `CurrentTaskId`. With filtered lists, this already works — the lookup doesn't depend on filtered view.

Fix: receive resolved `CurrentTask` directly from `IndexPageState` instead of looking it up. Removes unnecessary full-list parameter and aligns with step 3's `IndexPageState.CurrentTask`.

### 11. Schedule tab interaction with Google tasks

Schedule shows only Pomodoro-list tasks with `ScheduledDate`/`Repeat`. Google tasks with `DueDate` do NOT appear in Schedule (pure local planning view).

Schedule tab filters: `!IsGoogleTask && (IsScheduled || IsRecurring)`.

## Change sites

| File | Change |
|------|--------|
| `Services/TaskService.cs` | Wire GoogleTasksSettings into TaskLists; add UpdateListVisibilityAsync |
| `Models/GoogleTasksSettings.cs` | Already exists from Phase 2 — wire to settings repo |
| `Components/Tasks/TaskListTabs.razor` (new) | List tabs component |
| `Components/Tasks/TaskListSyncStrip.razor` (new) | Per-list sync strip |
| `Services/IndexPagePresenterService.cs` | Async UpdateState + list data + CurrentTask |
| `Services/IndexPagePresenterService.cs` (`IndexPageState`) | Add `CurrentListId`, `TaskLists`, `CurrentTask` |
| `Pages/Index.razor` + partials | Add TaskListTabs, async state, pass CurrentTask |
| `Components/Tasks/TaskItemComponent.razor` | Checkbox color param |
| `Components/Settings/GoogleConnectionSettings.razor` (new) | Split from CloudSyncSettings |
| `Components/Settings/GoogleListToggles.razor` (new) | Split from CloudSyncSettings |
| `Components/Settings/CloudSyncSettings.razor` | Container for sub-components |
| `Components/Tasks/TaskEditPanel.razor` | Add priority, due date, notes |
| `Services/ConsentService.cs:203` | Add list context to auto-start |
| `Pages/Index.razor.Events.cs:102` | Add list context to notification action |
| `Components/Timer/CurrentTaskIndicator.razor` | Receive CurrentTask directly |

## Tests

- Component: `TaskListTabs` renders Pomodoro + Google list tabs; selects tab on click
- Component: `TaskListTabs` hides Google tabs when disconnected
- Component: `TaskListSyncStrip` shows list name and connection status
- Component: `TaskEditPanel` renders priority/due/notes for local tasks; disabled for Google tasks (first cut)
- Component: `GoogleConnectionSettings` shows Connect/Disconnect + email
- Component: `GoogleListToggles` shows list toggles with color dots; toggles persist
- Unit: `TaskLists` reads Color/IsVisible from GoogleTasksSettings
- Unit: `UpdateListVisibilityAsync` persists to settings repo
- Unit: `IndexPageState.CurrentTask` resolves from `AllTasks`, not filtered list
- E2E: connect → list tabs appear → select Google tab → tasks display

## Risks

- `CloudSyncSettings` split changes existing component structure — ensure no regression in Drive sync UI
- `UpdateState` → async is a breaking change to the presenter pipeline — every caller must `await`
- `CurrentTaskIndicator` change (full-list param → direct CurrentTask) is backward compatible via optional param
- `ConsentService` auto-start with list context requires careful state coordination

## Review History

### Round 1 (verified against code)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 1 | 🟠 High | `TaskLists` hardcodes coral color + `IsVisible=true`. GoogleTasksSettings never read. Steps 5 + 6 have no backing data. | New step 1: wire settings into TaskService; `TaskLists` reads color/visibility from loaded settings; `RefreshGoogleListsAsync` loads settings after pull; add `UpdateListVisibilityAsync`/`UpdateListColorAsync` for toggle write-back. |
| 2 | 🟡 Medium | `UpdateState` becomes async due to `GetTasksForListAsync`. Plan doesn't note ripple. | Step 3: `UpdateState` → `UpdateStateAsync`; document that all callers must `await`. Verify callers. |
| 3 | 🟡 Medium | `CurrentTaskIndicator` needs current task outside filtered list, but step 2 only adds `CurrentListId`/`TaskLists` to state — not `CurrentTask`. | Step 3: add `CurrentTask` (resolved from `AllTasks`) to `IndexPageState`. Step 10: receive directly. |
| 4 | 🟡 Low | Step 4 injects `IGoogleTasksSyncService` (Phase 3, doesn't exist). | Use `ICloudSyncService` for connection state; static "read-only" label. |
| 5 | 🟡 Low | Step 8 tab-switch on auto-start may be unnecessary if indicator works across lists. | Defer tab-switch to cosmetic follow-up. |
| 6 | Nit | "186 lines" → actually 187. | Fixed. |
| 7 | Nit | Plan says `Pages/IndexBase.cs` — no such file. | Fixed: `Index.razor.cs` (primary partial class). |

### Round 2 (deep pass — material findings)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 8 | 🟡 Medium | Google API returns no list colors. Plan says "merge from settings" but nothing populates settings — all lists fall to default coral. | Step 1: palette-based seeding on first sight (`palette[listIndex % palette.Length]`), persisted to GoogleTasksSettings. User override deferred to Phase 3. |
| 9 | 🟡 Medium | `UpdateStateAsync` awaits `GetTasksForListAsync` which reads IndexedDB every tick (~1 read/sec). | Step 3: in-memory `_sidecarCache` in TaskService, hydrated once, invalidated on sidecar write. `GetTasksForListAsync` reads from cache (pure in-memory). New `InvalidateSidecarCacheAsync()` for post-pull refresh. |
| 10 | 🟡 Low | `_cachedGoogleLists` is `List<GoogleTaskList>` (`{Id, Title}` only) — can't hold color/visibility. | Step 1: replace with `List<TaskListRef>` as cache. Already has `Id`, `Title`, `Color`, `Count`, `IsVisible`. |
| 11 | 🟡 Low | `UpdateListColorAsync` unneeded in first cut — mock has no color picker, colors are auto-assigned. | Step 1: defer `UpdateListColorAsync` to Phase 3. Keep `UpdateListVisibilityAsync` only. |

### Round 3 (deep pass — cache semantics)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 12 | 🟡 Medium | Caching `TaskListRef` bakes stale `Count` — changes on every add/complete/delete between pulls. Tab badges go stale. | Step 1: cache only stable metadata (`GoogleListCacheEntry` record: Id, Title, Color, IsVisible — no Count). `TaskLists` getter recomputes `Count` live from `_appState.Tasks`. |
| 13 | 🟡 Low-Med | Sidecar cache invalidation "in repo" won't compile — `_sidecarCache` is a private `TaskService` field. | Step 3: explicit invalidation table — `AddTimeToTaskAsync` (after save), `ImportDataAsync` (after restore), `ClearAllAsync` (after delete). `InvalidateSidecarCache()` is sync (lazy re-hydration). |
| 14 | Nit | `var(--pomododo-color)` — typo. | Fixed: `var(--pomodoro-color)`. |

### Round 4 (final)

| # | Severity | Finding | Fix |
|---|----------|---------|-----|
| 15 | Nit | `ImportDataAsync`/`ClearAllAsync` live in other services — can't directly call `TaskService.InvalidateSidecarCache()`. | Wire via injected `ITaskService` or route through `ReloadAsync()` at impl time. Noted in invalidation table. |
