# Pomodoro — Project Guide for AI Agents

## Project Overview

Blazor WebAssembly (.NET 9) Pomodoro timer PWA. Single-page app with timer, tasks, history, settings, and cloud sync (Google Drive).

## Architecture

```
src/Pomodoro.Web/
  Components/       # Razor components (Tasks/, Timer/, Settings/, History/, Shared/)
  Constants/        # Partial class Constants (11 files: UI, Sync, Timer, Messages, etc.)
  Layout/           # MainLayout.razor
  Models/           # Data models (TaskItem, TimerSettings, Activity, etc.)
  Pages/            # Routeable pages (Index, Settings, About, History)
  Services/         # Business logic (ITimerService, ITaskService, ICloudSyncService, etc.)
  wwwroot/          # Static assets, JS interop files (googleDrive.js, compressionInterop.js)
  Program.cs        # DI registration
```

## Tech Stack

- **Frontend:** Blazor WebAssembly, C# 13, .NET 9
- **Testing (Unit):** xUnit, bUnit, Moq, FluentAssertions
- **Testing (E2E):** Playwright (Chromium only), TypeScript
- **Formatting:** `dotnet format Pomodoro.sln --verify-no-changes` (enforced in CI)
- **CI:** GitHub Actions (`pull-request.yml`, `deploy.yml`)

## Key Conventions

### Code Style
- No comments in code unless explicitly asked
- Nullable reference types enabled
- Constants use partial class pattern: `Constants.UI.ButtonClass`, `Constants.Sync.SyncFileName`
- Services use interface + implementation pattern (e.g., `ITimerService` / `TimerService`)
- Component code-behind uses `@inherits ComponentBase` pattern (e.g., `SettingsPageBase`)
- CSS classes are concise: `.sr` (setting row), `.ss` (settings section), `.tog` (toggle), `.stepper`, `.step-btn`, `.step-input`, `.step-unit`, `.sr-lbl`, `.sr-sub`, `.card`, `.card-hdr`, `.card-title`, `.mode-tabs`, `.mode-btn`, `.ring-area`, `.timer-time`, `.timer-mode-label`, `.task-row`, `.task-checkbox`, `.active-task`, `.sec-btn`, `.danger-btn`

### Testing Conventions
- Unit tests use `[Trait("Category", "Service")]` or `"Component"` or `"Page"`
- Component tests extend `TestHelper` (provides all mock services) or `TestContext` directly
- `TestHelper` auto-registers mocks for all services including `ICloudSyncService`
- When adding a new injected service, register `Mock.Of<INewService>()` in `TestHelper` constructor
- E2E tests use `PomodoroPage` fixture from `tests/e2e/fixtures/pomodoro.page.ts`
- E2E selectors use CSS class names from the actual rendered components

### Git Workflow
- `main` — production, `develop` — integration
- Feature branches: `feature/description` or `fix/description`
- PRs target `develop`, merge to `main` after
- Every PR must reference an issue via `Closes #XX` / `Fixes #XX` / `Resolves #XX`

### Development Cycle
When the user says "next item", follow this cycle:
1. Pick the next item from the board: **In Progress** first, then **Todo**
2. If the item is **In Progress** and already has an open PR:
   a. Update the PR branch with latest `develop` if needed (`git merge develop`)
   b. Try to merge the PR (`gh pr merge <number> --merge --admin`)
   c. If merge succeeds, set issue to **Review** and repeat from step 1
   d. If merge fails (CI pending/failing, API error), move to the next item and repeat from step 1
3. If the item is **Todo** or **In Progress** without a PR, implement it:
   a. Set its status to **In Progress** on the board
   b. Create a feature branch from `develop`: `feature/description` or `fix/description`
   c. Implement the changes
   d. Run `dotnet format Pomodoro.sln --verify-no-changes` and `dotnet test`
   e. Commit and push to the feature branch
   f. Create PR targeting `develop` with `Closes #XX` in the body
   g. Repeat from step 1
4. After all items are implemented and PRs are merged, set all merged issue statuses to **Review**
5. **Never set an issue to Review unless its PR is merged** — if a PR is still open/unmerged, the issue must remain In Progress

### Project Board Rules
- **In Progress** — set when starting work on an issue
- **Review** — set after PR is merged
- **Done** — manual only, set by user when they verify the change
- Only issues (cards) on the board — PRs are auto-removed by `pr-check.yml`
- Board node ID: `PVT_kwHOAJBk4M4BWD1D`, Status field ID: `PVTSSF_lAHOAJBk4M4BWD1DzhRbEOY`
- Status options: Todo (`0cdfd9a0`), In Progress (`ae38fc2d`), Review (`10a3102c`), Done (`a881df4c`)

### DI Registration
All services registered in `Program.cs`. When adding a new service:
1. Create interface in `Services/`
2. Create implementation in `Services/`
3. Register in `Program.cs`: `builder.Services.AddSingleton<IService, Service>()`
4. Add mock to `TestHelper.cs` constructor

## Common Commands

```bash
dotnet build Pomodoro.sln
dotnet test tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj
dotnet format Pomodoro.sln --verify-no-changes
npx playwright test tests/e2e/pages/
```

## CI Pipeline (ci.yml)

1. `build` → publishes app artifact once
2. `unit-test` ∥ `e2e` (parallel, both download build artifact)
3. `e2e-gate` — depends on all 16 E2E shards, creates single check for branch protection
4. 98% line coverage threshold configured in codecov.yml; coverage reported to Codecov via codecov-action

### E2E Shards (16 total)
`timer-flow`, `timer-ring`, `long-break`, `tasks`, `settings`, `history`, `consent-modal`, `consent-auto-continue`, `today-summary`, `pip`, `cloud`, `persistence`, `sound`, `mobile`, `about`, `navigation`

### Workflow Files
- `ci.yml` — Build, Test & Coverage (PR pipeline: build → unit-test ∥ e2e → e2e-gate)
- `e2e.yml` — E2E Tests (reusable workflow with 16-shard matrix)
- `deploy.yml` — Deploy to Cloudflare Pages (auto on main push, manual preview for any branch)
- `pr-check.yml` — PR Rules (enforce issue linkage, auto-remove PRs from board)
- `reports.yml` — Generate Coverage & E2E Reports (manual trigger)

## Coverage

- Codecov: https://app.codecov.io/github/Nayeem170/Pomodoro
- Local report: `dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage && reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:"Html"`

## Known Issues

- `NavigationManager` cannot be mocked with Moq (non-virtual properties) — use `TestNavigationManager` subclass instead
- `IJSRuntime.InvokeAsync<T>` is an extension method — cannot be mocked with Moq, use custom `IJSRuntime` test double
- `page.clock.install()` must be called AFTER the timer is started in E2E tests — installing it before Blazor initialization can freeze `DateTime.UtcNow` and prevent the app from loading
- Timer Start button requires a task to be selected first (`IsStartDisabled` is true by default) — E2E tests must call `addTask()` + `selectTask()` before `startTimer()`
- `BL0005` warnings from Blazor analyzer are suppressed in test project via `<NoWarn>BL0005</NoWarn>` (intentional parameter setting in tests)
