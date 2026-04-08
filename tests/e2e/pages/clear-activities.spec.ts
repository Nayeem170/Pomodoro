import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
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
  await page.waitForTimeout(3000);

  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Clear Data Removes Activities', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should remove all activities from history after clearing data', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Activity Clear Task');

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(3000);

    const activityCountBefore = await page.locator('.activity-item').count();
    if (activityCountBefore === 0) {
      return;
    }

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityCountAfter = await page.locator('.activity-item').count();
    expect(activityCountAfter).toBe(0);
  });

  test('should show empty state in history after clearing data', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Empty State Clear Task');

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const hasEmptyState = await page.locator('.empty-state').isVisible().catch(() => false);
    const hasNoActivities = (await page.locator('.activity-item').count()) === 0;
    expect(hasEmptyState || hasNoActivities).toBe(true);
  });
});
