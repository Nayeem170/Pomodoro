# Code Conventions

## Architecture

```
Pages (4) -> Components (49+) -> Services (24+) -> Repositories (3) -> IndexedDbService -> JS Interop
```

- **Models**: Plain classes, one per file, `namespace Pomodoro.Web.Models;`
- **Pages**: Razor + code-behind partial classes (split for large pages)
- **Presenter Services**: `*PresenterService` classes separate view formatting from rendering
- **Services**: Interface-based, DI-registered, scoped lifetime
- **Repositories**: `I*Repository` / `*Repository` pattern, all go through IndexedDbService
- **Constants**: `public static partial class Constants` with nested static classes per category

## File organization

```
src/Pomodoro.Web/
  Program.cs
  App.razor
  _Imports.razor
  Components/
    Timer/            Timer-specific components
    Tasks/            Task-related components
    History/          History components
    Settings/         Settings components
    Shared/           Cross-cutting components (ErrorBanner, ConsentModal, etc.)
    Schedule/         Schedule components
  Pages/
    Index.razor       + partial .razor.cs files
    History.razor     + partial .razor.cs files
    Settings.razor    + partial .razor.cs files
    About.razor
  Services/
    I*.cs             Interfaces
    *.cs              Implementations
    Formatters/       Domain formatters (TimeFormatter, ChartDataFormatter, etc.)
    Repositories/     I*Repository / *Repository
  Models/             One class or enum per file
  Constants/          Constants.Category.cs partial files
  Layout/
    MainLayout.razor  + code-behind
  wwwroot/
    js/               Browser interop
    css/              Stylesheets
    lib/              Third-party CSS via LibMan
```

## Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | File-scoped `namespace Pomodoro.Web.*;` | `namespace Pomodoro.Web.Models;` |
| Interface | `I` prefix + PascalCase | `ITimerService`, `IIndexedDbService` |
| Service impl | PascalCase, no suffix | `TimerService`, `TaskService` |
| Repository | `I` + Entity + `Repository` | `ITaskRepository`, `TaskRepository` |
| Presenter | Page + `PresenterService` | `IndexPagePresenterService` |
| Formatter | Domain + `Formatter` | `TimerThemeFormatter`, `TimeFormatter` |
| Model | Plain PascalCase | `TimerSettings`, `TaskItem`, `AppState` |
| Page base | Page name + `Base` | `IndexBase`, `HistoryBase` |
| Page partial | `Page.razor.Feature.cs` | `Index.razor.Timer.cs` |
| Component base | Component name + `Base` | `TimerDisplayBase` |
| Constants file | `Constants.Category.cs` | `Constants.Timer.cs` |
| Private fields | `_camelCase` | `_indexedDb`, `_appState` |
| Protected props | PascalCase | `IsLoading`, `CurrentSessionType` |
| Event handlers | `HandleXxx` or `OnXxx` | `HandleTimerStart`, `OnTimerCompleted` |
| Async methods | `XxxAsync` suffix | `StartPomodoroAsync`, `UpdateStateAsync` |

## Coding rules

1. **File-scoped namespaces** -- always, no braces
2. **Implicit usings enabled** + nullable reference types enabled
3. **Every service has an interface** -- DI uses interface-to-implementation mapping
4. **Scoped DI** -- `services.AddScoped<>()` for all services
5. **AppState as state container** -- thread-safe with locks, event-driven via `OnStateChanged`
6. **Event-driven architecture** -- `ITimerEventPublisher` with typed events
7. **EventWiringService** -- wires publisher to subscribers at startup
8. **Code-behind pattern** -- Razor files contain only markup; logic in `.razor.cs` partial classes
9. **Presenter pattern** -- `*PresenterService` classes handle view formatting logic, injected into pages
10. **No comments unless explaining non-obvious WHY** -- XML doc comments on public APIs are fine
11. **ASCII-only source** -- no em dash, smart quotes, unicode arrows, emoji
12. **Files < ~400 lines** -- split large classes with `partial class`
13. **Functions < ~60 lines**
14. **No fallbacks** -- always fix root cause
15. **Safe fire-and-forget** -- use `SafeTaskRunner.RunAndForget()` instead of discard pattern
16. **Constants extracted** -- all magic values in `Constants` partial class
17. **Property clamping** -- `Math.Clamp` on setters for validated ranges
18. **Switch expressions** -- preferred for session type-based logic
19. **Plain classes for models** -- not records; manual equality with `HashCode.Combine`
20. **`[Parameter]`** for data flow into components
21. **`[Inject]`** for DI in components/pages
22. **Regions in code-behind** -- `#region Services`, `#region State`, `#region Lifecycle`, `#region Cleanup`

## Testing

| Aspect | Pattern |
|--------|---------|
| Framework | xUnit with `[Fact]` and `[Trait("Category", "...")]` |
| Component tests | bUnit `TestContext`, `RenderComponent<T>()`, `cut.Find()`, `cut.Markup` |
| Service tests | Moq for interfaces, real `AppState`, manual `ServiceCollection` for `CreateService()` |
| Assertions | FluentAssertions (`Should().NotBeNull()`) AND xUnit (`Assert.Equal`) |
| Test hierarchy | `TestHelper : TestContext` for component tests; `TestBase` for shared mocks |
| Large test classes | Split into `Tests.Category.cs` partial files |
| Method naming | `MethodUnderTest_Scenario_ExpectedBehavior` |
| Structure | Explicit `// Arrange`, `// Act`, `// Assert` comments |
| Mocks | `new Mock<IXxx>()`, `.Setup()`, `.Returns()`, `.ReturnsAsync()`, `.Verify()` |
| Helpers | `CreateService()`, `CreateDefaultSettings()`, `CreateTestActivities()` |
| Parallel | No parallel execution (`DisableTestParallelization=true`) |
| Coverage | 99.5% target (codecov.yml) |

## Conventional commits

Format: `type: description`
Types: `feat`, `fix`, `chore`, `docs`, `refactor`
Branch: `type/ticket-name` from `develop`
