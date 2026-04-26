import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History With Data', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display activity items after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('History Data Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tl-row').first()).toBeVisible({ timeout: 5000 });

    const firstActivity = page.locator('.tl-row').first();
    await expect(firstActivity.locator('.tl-dot')).toBeVisible();
    await expect(firstActivity.locator('.tl-time')).toBeVisible();
    await expect(firstActivity.locator('.tl-badge')).toBeVisible();
  });

  test('should display non-zero stats after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Stats Data Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const pomodoroStat = page.locator('.sc').filter({ hasText: /pomodoros/i }).locator('.sv');
    const statText = await pomodoroStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should display focused time greater than zero after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Focus Time Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const focusStat = page.locator('.sc').filter({ hasText: /focus/i }).locator('.sv');
    const statText = await focusStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should render time distribution chart after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Chart Data Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.chart-row')).toBeVisible({ timeout: 5000 });
  });

  test('should display weekly mini chart on weekly view after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Weekly Chart Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('#weekly-tab').click();
    await expect(page.locator('.weekly-chart')).toBeVisible({ timeout: 5000 });
  });
});
