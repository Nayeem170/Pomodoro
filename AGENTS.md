# Pomodoro — Project Guide for AI Agents

## Project Overview
Blazor WebAssembly (.NET 9) Pomodoro timer PWA — timer, tasks, history, settings, Google Drive sync.

## Architecture
src/Pomodoro.Web/
  Components/   # Razor components (Tasks/, Timer/, Settings/, History/, Shared/)
  Constants/    # Partial class Constants (11 files: UI, Sync, Timer, Messages, etc.)
  Models/       # TaskItem, TimerSettings, Activity, etc.
  Pages/        # Index, Settings, About, History
  Services/     # Interfaces + implementations; ServiceRegistrationService.cs for DI
  wwwroot/      # googleDrive.js, compressionInterop.js
  Program.cs    # Entry point → ApplicationStartupService

## Tech Stack
- Blazor WASM, C# 13, .NET 9
- Unit tests: xUnit, bUnit, Moq, FluentAssertions
- E2E: Playwright (Chromium), TypeScript
- Formatting: `dotnet format Pomodoro.sln --verify-no-changes` (CI enforced)

## Key Conventions

### Code Style
- No comments unless asked; nullable reference types enabled
- Constants: partial class pattern — `Constants.UI.ButtonClass`, `Constants.Sync.SyncFileName`
- Services: interface + impl (e.g. `ITimerService` / `TimerService`)
- Code-behind: `@inherits ComponentBase` (e.g. `SettingsPageBase`)
- CSS uses concise utility names (e.g. `.sr`, `.tog`, `.stepper`) — follow existing component patterns

### Testing
- Traits: `[Trait("Category", "Service|Component|Page")]`
- Component tests extend `TestHelper` (all mocks pre-registered) or `TestContext`
- New injected service → add `Mock.Of<INewService>()` to `TestHelper` constructor
- E2E: use `PomodoroPage` fixture from `tests/e2e/fixtures/pomodoro.page.ts`; selectors use rendered CSS class names
- `IJSRuntime.InvokeAsync<T>` is an interface method and is directly mockable by Moq — no custom test double needed for basic setups. Custom test doubles in the codebase exist for advanced scenarios (sequential call tracking, per-identifier config), not as a workaround for a Moq limitation. Prefer Moq setups before introducing custom test doubles.

### Git Workflow
- `main` = production, `develop` = integration
- Branches: `feature/description` or `fix/description` off `develop`
- Every PR: targets `develop`, body includes `Closes #XX`

### Development Cycle
Triggered by "next item":
1. Pick next item — **In Progress** first, then **Todo**
2. **In Progress + open PR** → merge with `develop` if needed → `gh pr merge <n> --merge --admin`
   - Merged → set **Review**, repeat
   - Failed (CI/API) → skip, next item
3. **Todo or In Progress without PR** → implement:
   - Set **In Progress**; branch from `develop`
   - Run `dotnet format Pomodoro.sln --verify-no-changes` and `dotnet test`
   - Commit, push, open PR with `Closes #XX`; repeat from step 1
4. After all PRs merged → resolve CodeRabbit comments:
   - Read: `gh api repos/Nayeem170/Pomodoro/pulls/<n>/reviews`
   - Fix all actionable items on one branch → PR → merge
   - Then set issues to **Review**
5. **Never set Review** unless PR is merged AND CodeRabbit feedback resolved

### Project Board

| Status | Meaning | ID |
|---|---|---|
| Todo | not started | `0cdfd9a0` |
| In Progress | being worked on | `ae38fc2d` |
| Review | PR merged | `10a3102c` |
| Done | user-verified (manual only) | `a881df4c` |

Board: `PVT_kwHOAJBk4M4BWD1D` · Status field: `PVTSSF_lAHOAJBk4M4BWD1DzhRbEOY`
PRs are auto-removed from board by `pr-check.yml`.

### DI Registration
All services registered as **Scoped** via `ServiceRegistrationService` (Scoped = Singleton in WASM).
New service checklist: create interface → create impl → `services.AddScoped<IService, Service>()` → add mock to `TestHelper`.

### Cloud Sync
- Last-write-wins on `LastSyncedAt`; entire dataset pushed/pulled as a unit (no field-level merge)
- Debounced 300ms; compressed via `compressionInterop.js` before upload
- On push failure: surface error to user, do not silently swallow

## Common Commands
```bash
dotnet build Pomodoro.sln
dotnet test tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj
dotnet format Pomodoro.sln --verify-no-changes
npx playwright test tests/e2e/pages/
# Local coverage report:
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:"Html"
```

Codecov: https://app.codecov.io/github/Nayeem170/Pomodoro

## CI Pipeline
1. `build` → publishes artifact
2. `unit-test` ∥ `e2e` (parallel, both use build artifact; 16 shards, gated by `e2e-gate`)
3. 98% line coverage threshold (codecov.yml)

Workflows: `ci.yml` (PR pipeline) · `e2e.yml` (reusable, 16-shard matrix) · `deploy.yml` (auto on main, manual preview) · `pr-check.yml` (issue linkage + board cleanup) · `reports.yml` (manual)

## Known Issues
- `NavigationManager` — non-virtual, can't mock with Moq → use `TestNavigationManager` in `TestHelper.cs`
- `page.clock.install()` must be called **after** timer start in E2E — installing before Blazor init freezes `DateTime.UtcNow`
- `BL0005` suppressed in test project via `<NoWarn>BL0005</NoWarn>` (intentional)
