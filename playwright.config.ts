import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  retries: 0,
  workers: 8,
  timeout: 120000,
  reporter: [
    ['list'],
  ],
  use: {
    baseURL: 'http://localhost:5000',
    headless: true,
    serviceWorkers: 'block',
    trace: 'off',
    screenshot: 'only-on-failure',
    video: 'off',
    viewport: { width: 1280, height: 720 },
    actionTimeout: 15000,
    navigationTimeout: 30000,
  },
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
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
    command: 'npx serve ./bin/e2e-publish/wwwroot -l 5000 --single',
    url: 'http://localhost:5000',
    reuseExistingServer: true,
  timeout: 120000,
    stdout: 'pipe',
    stderr: 'pipe',
  },
});
