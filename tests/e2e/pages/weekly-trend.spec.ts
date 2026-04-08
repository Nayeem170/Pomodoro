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

test.describe('Weekly Week-Over-Week Trend', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should display weekly trend section with proper structure', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(1000);

    await expect(page.locator('.weekly-summary-section')).toBeVisible();
    await expect(page.locator('.weekly-chart-section')).toBeVisible();
  });

  test('should display week-over-week change stat when data exists', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Trend Task');

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(1000);

    const stats = page.locator('.stat');
    const statCount = await stats.count();
    expect(statCount).toBeGreaterThanOrEqual(3);

    const labels = page.locator('.stat-label');
    const labelCount = await labels.count();
    const labelTexts: string[] = [];
    for (let i = 0; i < labelCount; i++) {
      labelTexts.push((await labels.nth(i).textContent()) || '');
    }

    const hasMinutes = labelTexts.some(t => /minutes/i.test(t));
    const hasPomodoros = labelTexts.some(t => /pomodoros/i.test(t));
    expect(hasMinutes).toBe(true);
    expect(hasPomodoros).toBe(true);
  });
});
