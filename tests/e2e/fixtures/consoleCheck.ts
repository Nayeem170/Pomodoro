import { test as base, expect, browser } from '@playwright/test';
import type { Page, BrowserContext, ConsoleMessage } from '@playwright/test';

// Console messages that are NOT app defects (missing optional assets, etc.).
// Keep this list small and specific; an unfiltered error means a real defect.
const BENIGN: RegExp[] = [
  /favicon/i,
  // Blazor's transient boot-config fetch hiccup under high concurrency: during
  // a rapid reload while many workers boot WASM at once, a single boot.json
  // fetch can fail with "Failed to fetch" - Blazor retries and recovers, so the
  // functional assertions still pass. This does NOT suppress the corruption we
  // actually want to catch: an SRI failure ("Failed to find a valid digest") or
  // a hard 404 on a _framework .wasm/.pdb asset.
  /Failed to load config file .*blazor\.boot\.json.*Failed to fetch/i,
];

// Every e2e spec imports `test` from here instead of '@playwright/test', so the
// browser console is checked automatically for the WHOLE suite - not just a
// single smoke test. Any console.error / uncaught pageerror fails the test
// unless the test opts out with `test.use({ allowConsoleErrors: true })`.
//
// This catches regressions the unit suite cannot see: SRI/integrity failures,
// 404s on `_framework` assets (stale build manifests), and broken Blazor WASM
// boot - across every page and flow, on every run.
export const test = base.extend<{ allowConsoleErrors: boolean; consoleGuard: void }>({
  allowConsoleErrors: [false, { option: true }],

  // auto-use: set up before the test body, assert on teardown (after `use()`).
  consoleGuard: [async ({ page, allowConsoleErrors }, use) => {
    const errors: string[] = [];

    page.on('console', (msg: ConsoleMessage) => {
      if (msg.type() !== 'error') return;
      const text = msg.text();
      if (BENIGN.some((re) => re.test(text))) return;
      errors.push(text);
    });
    page.on('pageerror', (err: Error) => {
      errors.push(`uncaught: ${err.message}`);
    });

    await use();

    if (!allowConsoleErrors) {
      expect(errors, `Console errors during test:\n${errors.join('\n')}`).toEqual([]);
    }
  }, { auto: true }],
});

// Re-export the usual bindings so specs only change the import *path*, not the
// named imports (test, expect, browser, Page, BrowserContext, ConsoleMessage).
export { expect, browser };
export type { Page, BrowserContext, ConsoleMessage };
