# Known Issues Cleanup — Development Plan

## Objective

Act on the findings from `docs/archive/known-issues-analysis.md`: remove 2 incorrect Known Issues entries from AGENTS.md, consolidate 3 duplicate `NavigationManager` test subclasses, and remove dead `Mock<NavigationManager>` fields.

**This cleanup is intended to be behavior-preserving.** No production code changes. No intentional test behavior changes.

---

## Pre-flight Checks

Before any code changes, run these greps to confirm safety:

1. **`_mockNavigationManager.Verify`** — grep all 3 test files.
   - *Expected: no matches.*
   - Rationale: If any test calls `.Verify()` on the dead mock, those assertions are silently no-ops today (the mock is never resolved by DI). Confirm none exist so removal is safe.

2. **Navigation state usage** — grep the 3 test files for `.NavigateTo(`, `.Uri`, `.BaseUri`, `ToAbsoluteUri(`.
   - *Expected: no matches.*
   - Rationale: These would indicate implicit dependence on navigation state.

3. **`MockNavigationManager` constructor parity** — `MockNavigationManager` in `SettingsTests.cs` initializes with `"http://localhost/settings"` as the URI, while the other two use `"http://localhost/"`. The shared `TestNavigationManager` uses `"http://localhost/"`.
   - *Expected: no matches for `.Uri`, `.BaseUri`, or `ToAbsoluteUri(` in `SettingsTests.cs`.*
   - Already verified: no test references them.

4. **`IEnumerable<NavigationManager>`** — confirm no test resolves `NavigationManager` as `IEnumerable<T>`.
   - *Expected: no matches.*
   - Rationale: The "last DI registration wins" argument only applies to single-service resolution (`GetRequiredService<T>`). Multiple registrations would both be visible via `IEnumerable<T>`.

5. **`LocationChanged` subscriptions** — grep the 3 test files for `LocationChanged`.
   - *Expected: no matches.*
   - Rationale: This would indicate implicit dependence on navigation state — changing the concrete instance type while a subscription exists could alter event delivery.

---

## Changes

### A. Documentation cleanup

#### 1. AGENTS.md — Known Issues section

**Remove entries:**
- `IJSRuntime.InvokeAsync<T>` — claim is factually wrong, `InvokeAsync<T>` is an interface method mockable by Moq
- `IsStartDisabled` — intentional UX design, not an issue

**Update entry:**
- `NavigationManager` — change "use `TestNavigationManager` subclass" → "use `TestNavigationManager` in `TestHelper.cs`"

**Add to Testing conventions section:**
- `IJSRuntime.InvokeAsync<T>` is an interface method and is directly mockable by Moq — no custom test double needed for basic setups. Custom test doubles in the codebase exist for advanced scenarios (sequential call tracking, per-identifier config), not as a workaround for a Moq limitation. Prefer Moq setups before introducing custom test doubles.

#### 2. docs/known-issues-analysis.md — Archive

Move to `docs/archive/known-issues-analysis.md`. The analysis contains useful historical reasoning that may help during future regressions or onboarding. Archival preserves historical context while avoiding permanent loss.

### B. Shared Blazor/bUnit component test infrastructure consolidation

#### 3. TestHelper.cs — Add shared class

**Status: See PR #79.** Added `using Microsoft.AspNetCore.Components.Routing` and consolidated `TestNavigationManager` class at the bottom of the file. Constructor calls `Initialize("http://localhost/", "http://localhost/")` — matches 2 of 3 existing subclasses. The third (`MockNavigationManager` with `/settings` URI) has no tests reading `.Uri` or calling `ToAbsoluteUri()`, so the difference is irrelevant.

### C. Dead registration removal

#### 4. SettingsPageBaseTests.cs — Remove dead code

| Line(s) | What | Action |
|---|---|---|
| 30 | `private readonly Mock<NavigationManager> _mockNavigationManager;` | Remove field |
| 43 | `_mockNavigationManager = new Mock<NavigationManager>();` | Remove init |
| 57 | `Services.AddSingleton(_mockNavigationManager.Object);` | Remove registration |
| 58 | `Services.AddSingleton<NavigationManager, TestNavigationManager>();` | Keep as-is (resolves to shared class) |
| 408–416 | `internal class TestNavigationManager : NavigationManager { ... }` | Remove class |

#### 5. SettingsTests.cs — Remove dead code

| Line(s) | What | Action |
|---|---|---|
| 30 | `private readonly Mock<NavigationManager> _mockNavigationManager;` | Remove field |
| 43 | `_mockNavigationManager = new Mock<NavigationManager>();` | Remove init |
| 56 | `Services.AddSingleton(_mockNavigationManager.Object);` | Remove registration |
| 63 | `Services.AddSingleton<NavigationManager>(new MockNavigationManager());` | Change to `Services.AddSingleton<NavigationManager, TestNavigationManager>();` |
| 486–496 | `internal class MockNavigationManager : NavigationManager { ... }` | Remove class |

#### 6. SettingsPageBaseCoverageTests.cs — Remove duplicate subclass

| Line(s) | What | Action |
|---|---|---|
| 57 | `Services.AddSingleton<NavigationManager, TestNavManager>();` | Change to `Services.AddSingleton<NavigationManager, TestNavigationManager>();` |
| 236–243 | `internal class TestNavManager : NavigationManager { ... }` | Remove class |

---

## Verification

### Mandatory validation

```bash
dotnet build Pomodoro.sln
dotnet test --no-build tests/Pomodoro.Web.Tests/Pomodoro.Web.Tests.csproj
dotnet format Pomodoro.sln --verify-no-changes
```

All tests must pass. Format check must be clean. The cleanup should produce zero compiler warnings related to removed types or registrations. No observable test behavior changes are expected. CI must be green before merge.

### Cleanup completeness checks

```bash
grep -R "class .*NavigationManager : NavigationManager" tests/
```

*Expected: only `TestNavigationManager` in `TestHelper.cs` (exit code 0).* No stray duplicate subclasses should remain.

```bash
grep -R "_mockNavigationManager" tests/
```

*Expected: no matches (exit code 1).* Confirms the misleading mocks are fully eliminated.

```bash
grep -R "new Mock<NavigationManager>" tests/
```

*Expected: no matches (exit code 1).* Confirms no orphaned or DI-shadowed `NavigationManager` mocks remain.

> Note: equivalent PowerShell or ripgrep commands are acceptable for cross-platform environments.

---

## Risk

Low. Rollback is straightforward because all changes are isolated to test infrastructure and documentation. The dead `Mock<NavigationManager>` registrations are overridden by the subclass registrations (last single-service DI registration wins; no tests resolve `IEnumerable<NavigationManager>`). The 3 subclasses have functionally equivalent behavior for current test usage. No production code is touched. The only subtlety is the `MockNavigationManager` URI difference (`/settings` vs `/`), which is confirmed safe — no test reads `.Uri`, `.BaseUri`, calls `.NavigateTo()`, or subscribes to `.LocationChanged`.

**Why removal matters beyond cleanup:** The dead `Mock<NavigationManager>` fields are not just dead code — they are misleading test infrastructure. Any future test that adds `.Verify()` or `.Setup()` calls on these mocks would silently pass without asserting anything, creating false confidence. Removing them eliminates that risk entirely.

**Regression prevention:** If future tests require custom URI initialization behavior, prefer parameterization or configuration on `TestNavigationManager` before subclassing again. Only subclass if the required behavior cannot be represented through configuration — and in that case, extend `TestNavigationManager` rather than reintroducing standalone local subclasses.

---

## Future Considerations

Reevaluate extraction into `Testing/Infrastructure/` only if multiple shared helpers emerge — one class in `TestHelper.cs` is appropriate for the current scope.

Once navigation test infrastructure is centralized, shared navigation assertion helpers are a likely next consolidation target. Beyond that, service registration setup, test context builders, and helper DI factories are common duplication points that benefit from centralization as the test suite grows. If `TestNavigationManager` accumulates behavioral branches over time, pivot to composable or configurable helper patterns rather than continually extending one class.
