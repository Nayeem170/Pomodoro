# Blazor Server Component Tests

Blazor Server component tests using bUnit for code coverage tracking.

## Why Blazor Server?

Blazor WASM compiles to `.wasm` bytecode which **cannot be instrumented** by coverlet. This project compiles components as Blazor Server (standard .NET assemblies) to enable accurate coverage.

## Running Tests

```bash
dotnet test --project tests/Pomodoro.Web.Tests.Components
```

## Project Structure

```
tests/Pomodoro.Web.Tests.Components/
├── TestHelper.cs                          # Base class with common service mocks
├── Components/
│   ├── TimerControlsTests.cs              # Timer controls component
│   └── History/
│       ├── DailyViewTests.cs              # Daily view layout
│       ├── WeeklyViewTests.cs             # Weekly view layout
│       └── WeeklySummarySectionTests.cs   # Weekly summary section
└── Pages/
    ├── IndexBaseTests.cs                  # Index initialization & lifecycle
    ├── IndexPageTests.cs                  # Index page rendering
    ├── IndexPageExpandedTests.cs          # Index expanded scenarios
    ├── IndexConsentTests.cs               # Consent flow
    ├── IndexEventsTests.cs                # Event handling
    ├── IndexKeyboardShortcutTests.cs      # Keyboard shortcuts
    ├── IndexTasksTests.cs                 # Task operations
    ├── IndexTimerTests.cs                 # Timer operations
    ├── HistoryPageTests.cs                # History rendering
    ├── HistoryPageExpandedTests.cs        # History expanded scenarios
    ├── HistoryPageEventHandlersTests.cs   # Activity change events
    ├── HistoryPagePaginationTests.cs      # Infinite scroll pagination
    ├── HistoryPageInfiniteScrollTests.cs  # Scroll loading behavior
    ├── HistoryPageIntegrationTests.cs     # Tab navigation integration
    ├── HistoryPageLifecycleTests.cs       # Lifecycle management
    ├── HistoryPageDisposeTests.cs         # Cleanup on dispose
    └── SettingsPageTests.cs               # Settings page
```

## TestHelper

The `TestHelper` base class provides mocked services:

- `ITimerService`, `ITaskService`, `IActivityService`
- `INotificationService`, `IExportService`, `IIndexedDbService`
- `IConsentService`, `IPipTimerService`, `IKeyboardShortcutService`
- `IJSInteropService`, `IJSRuntime`, `ITodayStatsService`

And concrete instances for stateless services:

- `AppState`, `HistoryStatsService`, `IndexPagePresenterService`, `HistoryPagePresenterService`
- All formatters (`TimeFormatter`, `ChartDataFormatter`, `StatCardFormatter`, etc.)

## Writing New Tests

```csharp
public class MyComponentTests : TestHelper
{
    [Fact]
    public void MyComponent_RendersWithoutErrors()
    {
        var cut = RenderComponent<MyComponent>();
        cut.Markup.Should().NotBeNullOrEmpty();
    }
}
```

## Limitations

- **JS Interop** — Moq cannot mock extension methods like `InvokeAsync`; test behavior indirectly
- **Private Methods** — Test through public APIs or make them internal for direct testing
