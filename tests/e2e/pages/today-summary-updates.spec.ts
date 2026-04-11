import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('input[type="number"]').first();
  await pomodoroInput.fill('1');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);

  await page.locator('.btn-save').click();
  await page.waitForTimeout(2000);
  await expect(page.locator('.settings-toast')).toBeVisible();

  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
}

test.describe('Today Summary Updates', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should show non-zero focused time after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Focus Task');

    const summarySection = page.locator('.summary-section');
    await expect(summarySection).toBeVisible();

    const focusStat = summarySection.locator('.stat-item').filter({ hasText: /focused/i }).locator('.stat-value');
    const statText = await focusStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should show non-zero pomodoro count after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Pomodoro Task');

    const summarySection = page.locator('.summary-section');
    await expect(summarySection).toBeVisible();

    const pomodoroStat = summarySection.locator('.stat-item').filter({ hasText: /pomodoro/i }).locator('.stat-value');
    const statText = await pomodoroStat.textContent();
    expect(statText).toBeTruthy();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should show non-zero tasks worked on after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Tasks Task');

    const summarySection = page.locator('.summary-section');
    await expect(summarySection).toBeVisible();

    const tasksStat = summarySection.locator('.stat-item').filter({ hasText: /task/i }).locator('.stat-value');
    await expect(tasksStat).toContainText(/\d+/);
    const statText = await tasksStat.textContent();
    const numberMatch = statText?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should persist summary stats after page reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Persist Task');

    const summarySection = page.locator('.summary-section');
    await expect(summarySection).toBeVisible();

    const pomodoroStat = summarySection.locator('.stat-item, .summary-stat').filter({ hasText: /pomodoro/i }).first();
    await expect(pomodoroStat).toContainText(/\d+/);
    const statBefore = await pomodoroStat.textContent();
    const numberBefore = parseInt(statBefore?.match(/\d+/)![0]);

    await page.reload();
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const summarySectionAfter = page.locator('.summary-section');
    await expect(summarySectionAfter).toBeVisible({ timeout: 10000 });

    const pomodoroStatAfter = summarySectionAfter.locator('.stat-item, .summary-stat').filter({ hasText: /pomodoro/i }).first();
    await expect(pomodoroStatAfter).toContainText(/\d+/);
    const statAfter = await pomodoroStatAfter.textContent();
    const numberAfter = parseInt(statAfter?.match(/\d+/)![0]);
    expect(numberAfter).toBe(numberBefore);
    expect(numberAfter).toBeGreaterThan(0);
  });
});
