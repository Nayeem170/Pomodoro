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
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(1000);

    await expect(page.locator('.stat-grid').first()).toBeVisible();
    await expect(page.locator('.card-title').filter({ hasText: 'Sessions per day' })).toBeVisible();
  });

  test('should display week-over-week change stat when data exists', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Trend Task');

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(1000);

    const stats = page.locator('.sc');
    const statCount = await stats.count();
    expect(statCount).toBeGreaterThanOrEqual(3);

    const labels = page.locator('.sl');
    const labelCount = await labels.count();
    const labelTexts: string[] = [];
    for (let i = 0; i < labelCount; i++) {
      labelTexts.push((await labels.nth(i).textContent()) || '');
    }

    const hasFocus = labelTexts.some(t => /focus/i.test(t));
    const hasPomodoros = labelTexts.some(t => /pomodoros/i.test(t));
    expect(hasFocus).toBe(true);
    expect(hasPomodoros).toBe(true);
  });
});
