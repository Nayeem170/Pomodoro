import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  // Run tests serially to avoid server issues with parallel execution
  fullyParallel: false,
  retries: process.env.CI ? 1 : 0,
  // Use single worker to avoid server connection issues
  workers: 1,
  // Fail fast on first test failure in CI to avoid wasting time
  timeout: process.env.CI ? 30000 : 0,
  reporter: [
    ['html'],
    ['json', { outputFile: 'test-results/playwright-results.json' }],
    ['junit', { outputFile: 'test-results/junit.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:5000',
    headless: process.env.CI !== 'true',
    serviceWorkers: 'block',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    viewport: { width: 1280, height: 720 },
    actionTimeout: 15000,
    navigationTimeout: 60000,
  },
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        // Enable WebAssembly support
        launchOptions: {
          args: [
            '--enable-features=WebAssembly',
            '--enable-features=WebAssemblySimd',
            '--no-sandbox',
            '--disable-setuid-sandbox',
          ],
        },
      },
    },
  ],
  webServer: {
    // Use serve with SPA support (rewrites all routes to index.html)
    // The -s flag enables SPA mode which rewrites 404s to index.html
    command: 'npx serve ./bin/e2e-publish/wwwroot -p 5000 -s',
    url: 'http://localhost:5000',
    // Reuse existing server if already running (avoids restart issues)
    reuseExistingServer: true,
    timeout: 60000, // 1 minute timeout for server startup
    stdout: 'pipe',
    stderr: 'pipe',
  },
});
