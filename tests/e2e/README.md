# Playwright End-to-End Tests

Playwright e2e tests for the Pomodoro WebAssembly application.

## Setup

```bash
npm install
npm run install:browsers
```

## Running Tests

```bash
npm test                # Run all tests
npm run test:headed     # With browser window
npm run test:ui         # Interactive UI mode
npm run report          # View HTML report
```

## Test Structure

```
tests/e2e/
├── fixtures/
│   └── pomodoro.page.ts        # Shared page object
├── pages/                      # 67 test suites
│   ├── timer*.spec.ts          # Timer (8 files)
│   ├── tasks*.spec.ts          # Tasks (5 files)
│   ├── settings*.spec.ts       # Settings (5 files)
│   ├── history*.spec.ts        # History (8 files)
│   ├── consent*.spec.ts        # Consent flow (3 files)
│   ├── import*.spec.ts         # Import/export (6 files)
│   ├── notification*.spec.ts   # Notifications (3 files)
│   ├── data-*.spec.ts          # Data management (4 files)
│   ├── *nav*.spec.ts           # Navigation (4 files)
│   ├── pwa-*.spec.ts           # PWA/offline (2 files)
│   ├── pip-*.spec.ts           # Picture-in-Picture (2 files)
│   └── ...                     # Error handling, duration, about, etc.
└── components/
    └── timer-controls.spec.ts  # Component-level tests
```

## Configuration

Configured in [`playwright.config.ts`](../../playwright.config.ts):

| Setting | Value |
|---------|-------|
| Browser | Chromium |
| Viewport | 1280x720 |
| Base URL | `http://localhost:5000` |
| Parallelism | Serial (1 worker) |
| Retries | 2 on CI, 0 locally |
| Artifacts | Trace, screenshot, video on failure |

## Notes

- Tests use a pre-published Blazor WASM build served via `npx serve` in SPA mode
- Selectors use CSS classes and `data-testid` attributes
- Failed tests produce screenshots, videos, and traces in `test-results/`
