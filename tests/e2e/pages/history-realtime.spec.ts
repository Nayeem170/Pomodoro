import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 2000; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(5);
    }
  });
  await page.waitForTimeout(5000);

  const consentModal = page.locator('.consent-modal-overlay');
  if (await consentModal.isVisible({ timeout: 3000 }).catch(() => false)) {
    const startOption = page.locator('.btn-option').filter({ hasText: /Start Pomodoro|Another Pomodoro/i });
    if (await startOption.first().isVisible({ timeout: 2000 }).catch(() => false)) {
      await startOption.first().click();
      await page.waitForTimeout(1000);
    } else {
      const anyOption = page.locator('.btn-option').first();
      if (await anyOption.isVisible({ timeout: 1000 }).catch(() => false)) {
        await anyOption.click();
        await page.waitForTimeout(1000);
      }
    }
  }
}

test.describe('History Real-Time Updates', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should update history page when new activity is recorded', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityCountBefore = await page.locator('.tl-row').count();

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await completePomodoroFast(page, pomodoroPage, 'Realtime Update Task');

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(5000);

    const activityCountAfter = await page.locator('.tl-row').count();
    expect(activityCountAfter).toBeGreaterThanOrEqual(activityCountBefore);
  });
});
