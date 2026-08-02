# Pipeline configuration

## Models

developer_id = glm-5-turbo
developer_variant = high
reviewer_id = claude-sonnet-5
reviewer_variant = high

## Commands

build = dotnet build Pomodoro.sln -c Release --no-incremental
test = dotnet test tests/Pomodoro.Web.Tests -c Release --verbosity normal
lint = dotnet format Pomodoro.sln --verify-no-changes --verbosity diagnostic
e2e_build = dotnet publish src/Pomodoro.Web -c Release -o bin/e2e-publish
e2e_test = npx playwright test

## Branching

base_branch = develop

## Pen CLI

pen_cli = pen

## Additional repos

none

## Convention files

AGENTS.md
docs/CODE_CONVENTIONS.md
docs/REVIEW_RULES.md

## Project structure

src/Pomodoro.Web/          Main app (Blazor WASM)
  Components/               UI components (Timer/, Tasks/, History/, Settings/, Shared/, Schedule/)
  Pages/                    Route pages (Index, History, Settings, About)
  Services/                 Business logic (interfaces + implementations)
    Formatters/             View formatting (TimeFormatter, ChartDataFormatter, etc.)
    Repositories/          Data access (ITaskRepository, IActivityRepository, ISettingsRepository)
  Models/                   Domain models (one class/enum per file)
  Constants/                Constants.Category.cs partial files
  Layout/                   MainLayout.razor + code-behind
tests/Pomodoro.Web.Tests/  Unit tests (bUnit + xUnit + Moq + FluentAssertions)
tests/e2e/                 E2E tests (Playwright, Chromium)
docs/                      Documentation and plans

## Architecture

Pages -> Components -> Services -> Repositories -> IndexedDbService -> JS Interop

- Interface-based DI with scoped registration
- Code-behind pattern (markup in .razor, logic in .razor.cs)
- Presenter services for view formatting logic
- Constants in partial static class
- Event-driven via ITimerEventPublisher + EventWiringService
- AppState as state container (thread-safe, event-driven)
- SafeTaskRunner for fire-and-forget

## Deploy

deploy = User deploys via Cloudflare Pages (static Blazor WASM)

## gh CLI

gh_prereq = $env:GH_TOKEN = $null
develop is a protected branch. PRs must be created via `gh pr create` after
pushing to a feature branch. Clear `GH_TOKEN` before any `gh` command
to avoid stale token overriding keyring auth.
