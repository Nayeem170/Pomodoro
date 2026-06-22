# Known Issues — Fixability Analysis

Analysis of each entry in `AGENTS.md > Known Issues` and whether it can be resolved.

---

## 1. NavigationManager — Can't Mock with Moq

**Verdict: Cannot fix. Subclass approach is the only option.**

`NavigationManager` is an abstract class where the critical public members (`Uri`, `BaseUri`, `NavigateTo()`) are non-virtual. `Initialize()` is also non-virtual (`protected void`), so Moq cannot call it to set up the internal `_isInitialized` flag. Any attempt to use `Uri` or `NavigateTo()` on a raw Moq proxy throws `InvalidOperationException`.

### Cleanup opportunity

Three test files each define a duplicate subclass and create a dead `Mock<NavigationManager>` that gets overridden in DI:

| File | Subclass | Dead Mock |
|---|---|---|
| `SettingsPageBaseTests.cs:408` | `TestNavigationManager` | `Mock<NavigationManager>` |
| `SettingsTests.cs:486` | `MockNavigationManager` | `Mock<NavigationManager>` |
| `SettingsPageBaseCoverageTests.cs:236` | `TestNavManager` | `Mock<NavigationManager>` |

Since the last DI registration wins, the `Mock<>` is never resolved. The three subclasses are identical in behavior and could be consolidated into one shared class in `TestHelper.cs`.

**Recommended action:** Consolidate into a single `TestNavigationManager` in `TestHelper.cs`, remove the three private subclasses and their dead `Mock<NavigationManager>` fields.

---

## 2. IJSRuntime.InvokeAsync\<T\> — Extension Method Claim Is Incorrect

**Verdict: AGENTS.md claim is factually wrong. No fix needed — remove the entry.**

`InvokeAsync<T>` is a genuine interface method on `IJSRuntime`, not an extension method. The interface defines:

```csharp
public interface IJSRuntime
{
    ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args);
    ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args);
}
```

The extension methods in `JSRuntimeExtensions` are `params` convenience wrappers that delegate to these interface methods. Moq generates a dynamic proxy implementing the interface and intercepts all interface method calls, including generic ones.

The project already mocks `IJSRuntime` with Moq extensively — `TestBase.cs` has `SetupJsInvokeAsync<T>()` and `SetupJsInvokeVoidAsync()` helpers, and most tests use `Mock.Of<IJSRuntime>()` via `TestHelper`.

The 6 custom test doubles that exist in the codebase are convenience choices for complex scenarios (sequential call tracking, per-identifier config), not workarounds for a Moq limitation:

| Test Double | Location | Why It Exists |
|---|---|---|
| `TestJsRuntime` | `GoogleDriveServiceTests.cs:246` | Sequential call tracking with ordered results/exceptions |
| `TestJsRuntime` | `WeeklyMiniChartTests.cs:661` | Simple invocation counting |
| `TestJsRuntime` | `TimerServiceTests.Coverage.cs:404` | Per-identifier configurable behavior via dictionary |
| `TestIndexJsRuntime` | `IndexCoverageTests.cs:373` | Minimal no-op |
| `ThrowingTestIndexJsRuntime` | `IndexCoverageTests.cs:373` | Always throws |
| `ThrowingJSRuntime` | `InfiniteScrollInteropTests.cs:223` | Always throws `JSException` |

**Recommended action:** Remove this Known Issue entry from AGENTS.md. Optionally consolidate the test doubles if desired, but they are not bugs.

---

## 3. page.clock.install() Must Be Called After Timer Start

**Verdict: Cannot practically fix. Current workaround is correct.**

Playwright's `page.clock.install()` replaces `window.Date`, `setTimeout`, `setInterval`, and `performance.now()` globally. In Blazor WASM, `DateTime.UtcNow` maps to `Date.now()` through the WASM runtime. If the clock is installed before Blazor init:

1. Every `DateTime.UtcNow` during the init chain returns the same frozen value
2. `TimerService.InitializeAsync()` sets `StartedAt` to that frozen value
3. `OnTimerTickJs()` computes `remaining = EndAt - DateTime.UtcNow` which never decreases
4. Blazor's own bootstrapping (`setTimeout(0)` for render scheduling) may break entirely

The lazy-install pattern in `PomodoroPage.startTimer()` (install clock on first call, after the timer is already running) is the pragmatic solution.

### Theoretical fix (not recommended)

Introduce an `ITimeProvider` abstraction with `DateTime GetUtcNow()`, replace all 38 `DateTime.UtcNow`/`DateTime.Now` call sites, and control time via JS interop in E2E instead of Playwright's clock. This eliminates the root cause but is significant refactoring with no user-facing benefit.

**Recommended action:** Keep as-is. The documented workaround is the correct approach.

---

## 4. Timer Start Requires a Selected Task

**Verdict: Not a bug. Intentional UX design. Remove from Known Issues.**

This is a deliberate Pomodoro Technique constraint — focus sessions must be tied to a specific task. Evidence:

- `CanStart` logic is session-aware: `CurrentTaskId.HasValue || CurrentSessionType != SessionType.Pomodoro` — breaks don't require a task
- Explicit UI affordances: tooltip "Select a task first", hint message "Select a task to start"
- Defense-in-depth handler in `HandleTimerStart` re-checks and shows `Constants.Messages.SelectTaskBeforePomodoro`
- Auto-selection on task add (`TaskService.cs:102`) reduces friction
- Unit tests explicitly verify the disabled state (`TimerControlsBaseTests.cs:72-83`)

**Recommended action:** Remove from Known Issues. This is intentional, documented behavior — not an issue.

---

## 5. BL0005 Suppression in Test Project

**Verdict: Cannot fix. Suppression is the standard, accepted practice.**

BL0005 warns when `[Parameter]` properties are set from outside the component. bUnit's core API (`RenderComponent<T>(parameters => parameters.Add(p => p.Prop, value))`) fundamentally requires this — it's the official, recommended way to pass parameters in tests. The Blazor analyzer cannot distinguish test code from production code.

The suppression is correctly scoped to the test project only (`<NoWarn>BL0005</NoWarn>` in `Pomodoro.Web.Tests.csproj`), not the production project.

**Recommended action:** No change needed.

---

## Summary Table

| # | Issue | Fixable? | AGENTS.md Action |
|---|---|---|---|
| 1 | NavigationManager mock | No | Keep entry; clean up dead code separately |
| 2 | IJSRuntime extension method | **Claim is wrong** | **Remove entry** |
| 3 | page.clock.install() ordering | No (practically) | Keep entry as-is |
| 4 | IsStartDisabled | Not a bug | **Remove entry** — intentional UX |
| 5 | BL0005 suppression | No | Keep entry as-is |
