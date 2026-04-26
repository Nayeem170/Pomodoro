import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  await page.waitForTimeout(500);

  await pomodoroPage.completePomodoroFast();

  await pomodoroPage.skipConsentModal();
}

test.describe('Consent Auto-Continue', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should auto-start default session when consent countdown reaches zero', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Auto Continue Test');
    await pomodoroPage.selectTask('Auto Continue Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await page.waitForTimeout(500);

    await pomodoroPage.completePomodoroFast();

    const consentModal = page.locator('.consent-modal-overlay');
    const isModalVisible = await consentModal.isVisible({ timeout: 5000 }).catch(() => false);

    if (isModalVisible) {
      const countdownText = page.locator('.auto-continue-row strong');
      const text = await countdownText.textContent();
      const seconds = parseInt(text || '10');

      await page.waitForTimeout((seconds + 3) * 1000);
    }

    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 20000 });
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('FOCUSING');
  });
});
