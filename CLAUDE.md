# Claude Code - Pomodoro Project Guide

You are working on a Blazor WebAssembly (.NET 9) Pomodoro timer PWA.
Read AGENTS.md for the full project guide. This file covers Claude-specific setup.

## Commands

```bash
dotnet format Pomodoro.sln --verify-no-changes
dotnet build Pomodoro.sln --configuration Release
dotnet test Pomodoro.sln --configuration Release --verbosity normal
python tools/gates/run.py
python tools/gates/run.py --ticket T-001 --base develop
python tools/gates/run.py --ticket T-001 --cleanup
python tools/gates/run.py --skip-format
cd tests/e2e && npx playwright test
```

## Architecture

```
src/Pomodoro.Web/
  Components/   # Razor components (Tasks/, Timer/, Settings/, History/, Shared/, Schedule/)
  Constants/    # Partial class Constants (11 files: UI, Sync, Timer, Messages, etc.)
  Models/       # TaskItem, TimerSettings, Activity, ScheduleAgenda, etc.
  Pages/        # Index, Settings, About, History
  Services/     # Interfaces + implementations; ServiceRegistrationService.cs for DI
  wwwroot/      # googleTasks.js, compressionInterop.js
  Program.cs    # Entry point -> ApplicationStartupService
tests/Pomodoro.Web.Tests/   # Unit tests (xUnit + bUnit + Moq + FluentAssertions)
tests/e2e/                     # E2E tests (Playwright, Chromium, 8 shards)
tools/gates/                   # Deterministic gate orchestrator (Python)
```

## Code Conventions

- No comments unless asked; nullable reference types enabled
- Constants: partial class pattern - `Constants.UI.ButtonClass`, `Constants.Sync.SyncFileName`
- Services: interface + impl (e.g. `ITimerService` / `TimerService`)
- Code-behind: `@inherits ComponentBase` (e.g. `SettingsPageBase`)
- CSS uses concise utility names (e.g. `.sr`, `.tog`, `.stepper`) - follow existing patterns
- All services registered as **Scoped** via `ServiceRegistrationService` (Scoped = Singleton in WASM)
- New service checklist: create interface -> create impl -> `services.AddScoped<IService, Service>()` -> add mock to `TestHelper`

## Testing

- xUnit, bUnit, Moq, FluentAssertions
- Traits: `[Trait("Category", "Service|Component|Page")]`
- Component tests extend `TestHelper` (all mocks pre-registered) or `TestContext`
- New injected service -> add `Mock.Of<INewService>()` to `TestHelper` constructor
- E2E: use `PomodoroPage` fixture from `tests/e2e/fixtures/pomodoro.page.ts`
- `IJSRuntime.InvokeAsync<T>` is directly mockable by Moq - prefer Moq before custom test doubles
- `NavigationManager` is non-virtual - use `TestNavigationManager` in `TestHelper.cs`
- `BL0005` suppressed in test project via `<NoWarn>BL0005</NoWarn>`
- 99.5% line coverage threshold (Codecov)

## Gate Pipeline

Run `python tools/gates/run.py` before pushing. It runs format + build + test +
line-coverage gates (99.5% threshold, same cobertura computation as the CI
unit-test job) and writes state files to `.gates/`.

CI additionally enforces cache-version discipline (cache-version job): any
wwwroot/ change requires CACHE_VERSION to increase with a matching CACHE_NAME,
and the publish output must ship service-worker.js byte-identical to the source
worker (regression guard for #160 - the worker must actually deploy).

For isolated worktree runs: `python tools/gates/run.py --ticket T-001 --base develop`
Spawns a worktree at `../.worktrees-pomodoro/ticket-T-001/`, runs gates, tears down on failure.

## Requirements Workflow

This project follows an AI-augmented SDLC with 5 layers. See `docs/requirements/000-framework/`.

- Requirements use EARS notation in RSDs: `WHEN <trigger> THE SYSTEM SHALL <response>`
- One folder per task under `docs/requirements/NNN-task-slug/`
- RSD must be signed off before TDS is written
- Spec first, always. No implementation without a reviewed design document.

## Git Workflow

- `main` = production, `develop` = integration
- Branches: `feature/description` or `fix/description` off `develop`
- Every PR: targets `develop`, body includes `Closes #XX`

## Do Not

- Do not add comments to code
- Do not silently patch out-of-scope discoveries - create new tasks
- Do not mock `NavigationManager` with Moq - use `TestNavigationManager`
- Do not install libraries not already in the solution
- Do not commit to `main` directly
- Do not set GitHub issues to "Review" unless PR is merged AND CodeRabbit feedback resolved
