# Pomodoro

A Pomodoro-style focus timer built as a Progressive Web App with Blazor WebAssembly (.NET 9).

## Features

- **Timer** — Pomodoro, short break, and long break sessions with configurable durations
- **Tasks** — Add, complete, and delete tasks; select an active task per session
- **History** — Daily and weekly views with activity timelines and time-distribution charts
- **Settings** — Duration, sound, notifications, auto-start, and data management (import/export/clear)
- **PWA** — Installable, offline-capable with service worker caching
- **PiP Timer** — Picture-in-Picture timer window for multitasking
- **Keyboard Shortcuts** — Full keyboard navigation and timer controls
- **Browser Notifications** — Desktop alerts on timer completion with action buttons

## Prerequisites

- .NET 9 SDK
- Node.js 18+ (for e2e tests only)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/Pomodoro.Web
```

The app is available at `http://localhost:7025`.

## Project Structure

```
src/Pomodoro.Web/
├── Components/          # Blazor components (49 total)
│   ├── History/         # History page sub-components
│   └── Settings/        # Settings page sub-components
├── Pages/               # Razor pages (Index, History, Settings, About)
├── Services/            # Application services with DI
│   ├── Repositories/    # IndexedDB data access
│   └── Formatters/      # Display formatting logic
├── Models/              # Domain models and records
├── Constants/           # Shared constants
├── Layout/              # App shell and navigation
└── wwwroot/
    ├── js/              # JavaScript interop (11 files)
    ├── css/             # Stylesheets
    └── lib/             # Client libraries (Bootstrap 5.3, Chart.js 4.4)
```

## Architecture

The app follows an MVP-like pattern:

- **Models** — Domain objects (`ActivityRecord`, `TaskItem`, `TimerSettings`, etc.)
- **Views** — Blazor Razor components and pages
- **Presenters** — `*PresenterService` classes separating UI logic from rendering
- **Services** — Interface-based application services registered via DI
- **Repositories** — Data persistence over IndexedDB through JS interop

## Testing

### Unit & Component Tests

```bash
dotnet test
```

| Project | Framework | Tests |
|---------|-----------|-------|
| `Pomodoro.Web.Tests` | xUnit + bUnit + Moq | ~3,300 |
| `Pomodoro.Web.Tests.Components` | xUnit + bUnit (Blazor Server) | ~189 |

The Components project compiles components as Blazor Server to enable coverlet instrumentation (WASM bytecode cannot be instrumented). See [tests/Pomodoro.Web.Tests.Components/README.md](tests/Pomodoro.Web.Tests.Components/README.md) for details.

### End-to-End Tests

```bash
npm install
npm run install:browsers
npm test
```

69 Playwright test suites covering all pages, components, and user flows. See [tests/e2e/README.md](tests/e2e/README.md) for details.

### Coverage Reports

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool run reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:CoverageReport/HtmlReport -reporttypes:Html
```

| Report | Local | GitHub Pages |
|--------|-------|-------------|
| Unit & Component Coverage | [CoverageReport/HtmlReport/index.html](CoverageReport/HtmlReport/index.html) | [GitHub Pages](https://<username>.github.io/<repo>/unit/index.html) |
| E2E Test Results | [playwright-report/index.html](playwright-report/index.html) | [GitHub Pages](https://<username>.github.io/<repo>/e2e/index.html) |

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | Blazor WebAssembly (.NET 9) |
| Styling | Bootstrap 5.3.3 |
| Charts | Chart.js 4.4.1 |
| Data Storage | IndexedDB (via JS interop) |
| PWA | Service Worker (stale-while-revalidate) |
| Unit Tests | xUnit, bUnit, Moq, FluentAssertions |
| E2E Tests | Playwright (Chromium) |
| Coverage | Coverlet, ReportGenerator |

## License

Private repository.
