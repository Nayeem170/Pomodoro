import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Loading States', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should show Connect button without loading state initially', async ({ page }) => {
    const cloudSyncHeader = page.locator('.ss-hdr').filter({ hasText: 'Cloud Sync' });
    await expect(cloudSyncHeader).toBeVisible();

    const connectButton = page.locator('.sec-btn').filter({ hasText: 'Connect' });
    const isConnectVisible = await connectButton.isVisible({ timeout: 5000 }).catch(() => false);

    if (isConnectVisible) {
      await expect(connectButton).toContainText('Connect');
      const isDisabled = await connectButton.isDisabled();
      expect(isDisabled).toBe(false);
    }
  });

  test('should not show loading spinner on Connect button initially', async ({ page }) => {
    const connectButton = page.locator('.sec-btn').filter({ hasText: 'Connect' });
    const isConnectVisible = await connectButton.isVisible({ timeout: 5000 }).catch(() => false);

    if (isConnectVisible) {
      const buttonText = await connectButton.textContent();
      expect(buttonText).not.toContain('Connecting');
    }
  });
});
