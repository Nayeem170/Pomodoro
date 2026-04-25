import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History With Data', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should display activity items after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('History Data Task');
    await pomodoroPage.selectTask('History Data Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityItems = page.locator('.tl-row');
    const count = await activityItems.count();
    expect(count).toBeGreaterThanOrEqual(1);

    const firstActivity = activityItems.first();
    await expect(firstActivity.locator('.tl-dot')).toBeVisible();
    await expect(firstActivity.locator('.tl-time')).toBeVisible();
    await expect(firstActivity.locator('.tl-badge')).toBeVisible();
  });

  test('should display non-zero stats after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Stats Data Task');
    await pomodoroPage.selectTask('Stats Data Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const pomodoroStat = page.locator('.sc').filter({ hasText: /pomodoros/i }).locator('.sv');
    const statText = await pomodoroStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should display focused time greater than zero after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Focus Time Task');
    await pomodoroPage.selectTask('Focus Time Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const focusStat = page.locator('.sc').filter({ hasText: /focus/i }).locator('.sv');
    const statText = await focusStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should render time distribution chart after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Chart Data Task');
    await pomodoroPage.selectTask('Chart Data Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    await expect(page.locator('.chart-row')).toBeVisible();
  });

  test('should display weekly mini chart on weekly view after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Weekly Chart Task');
    await pomodoroPage.selectTask('Weekly Chart Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(1000);

    await expect(page.locator('.weekly-chart')).toBeVisible();
  });
});
