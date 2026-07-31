# AGENTS.md

## Project overview

Blazor WebAssembly (C#, .NET 9) Pomodoro timer web app deployed to Cloudflare Pages via GitHub Actions.

## Repository structure

```
src/Pomodoro.Web/          Main app (Blazor WASM)
tests/Pomodoro.Web.Tests/  Unit tests (bUnit + xUnit + Moq + FluentAssertions)
tests/e2e/                 E2E tests (Playwright, Chromium)
docs/                      Documentation and plans
.agentic/                  Agentic pipeline configuration
```

## Tech stack

- **Frontend**: Blazor WebAssembly (.NET 9), HTML5, CSS3
- **State**: IndexedDB via JS interop, in-memory AppState with event-driven updates
- **Testing**: bUnit (component), xUnit (unit), Moq (mocking), FluentAssertions (assertions), Playwright (e2e)
- **CI/CD**: GitHub Actions, Cloudflare Pages
- **Coverage**: Coverlet + Codecov, 99.5% target

## Commands

```bash
dotnet build Pomodoro.sln -c Release              # Build
dotnet format Pomodoro.sln --verify-no-changes     # Lint
dotnet test tests/Pomodoro.Web.Tests -c Release    # Unit tests
dotnet publish src/Pomodoro.Web -c Release -o bin/e2e-publish  # E2E build
npx playwright test                                # E2E tests
```

## Conventions

See `docs/CODE_CONVENTIONS.md` for full coding standards.
See `docs/REVIEW_RULES.md` for active review rules.

Key points:
- Interface-based DI with scoped registration
- Code-behind pattern (markup in .razor, logic in .razor.cs)
- Presenter services for view formatting logic
- Constants in partial static class
- ASCII-only source, no unnecessary comments
- Conventional commits, branch from `develop`

## Coverage

99.5% coverage target. Do not merge below this threshold.
