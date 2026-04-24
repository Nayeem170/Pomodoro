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

## CI Pipeline (pull-request.yml)

1. `dotnet restore` → `dotnet format --verify-no-changes` → `dotnet build` → `dotnet test` (with coverage)
2. Generate coverage report → Upload artifact → Publish to Codecov
3. E2E smoke tests (navigation, timer, settings, index, about)

## Coverage

- Codecov: https://app.codecov.io/github/Nayeem170/Pomodoro
- Local report: `dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage && reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:"Html"`

## Known Issues

- E2E smoke tests use outdated selectors from before UI redesign (`.btn-start`, `.settings-header`, `.footer`, etc.) — these need updating to match current CSS classes
- `NavigationManager` cannot be mocked with Moq (non-virtual properties) — use `TestNavigationManager` subclass instead
- `IJSRuntime.InvokeAsync<T>` is an extension method — cannot be mocked with Moq, use custom `IJSRuntime` test double
