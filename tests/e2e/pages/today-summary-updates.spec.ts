import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary Updates', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should show non-zero focused time after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Summary Focus Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const focusTime = page.locator('.pomo-focus-time');
    await expect(focusTime).toBeVisible();
    const text = await focusTime.textContent();
    expect(text).toBeTruthy();
    const numberMatch = text?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should show non-zero pomodoro count after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Summary Pomodoro Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomodoroNum = page.locator('.pomo-num');
    await expect(pomodoroNum).toBeVisible();
    const text = await pomodoroNum.textContent();
    expect(text).toBeTruthy();
    const numberMatch = text?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should update summary immediately after pomodoro completion', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomodoroNumBefore = page.locator('.pomo-num');
    const textBefore = await pomodoroNumBefore.textContent();
    const matchBefore = textBefore?.match(/\d+/);
    const countBefore = matchBefore ? parseInt(matchBefore[0]) : 0;

    await pomodoroPage.seedHistoryViaDB('Immediate Update Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomodoroNumAfter = page.locator('.pomo-num');
    const textAfter = await pomodoroNumAfter.textContent();
    const matchAfter = textAfter?.match(/\d+/);
    const countAfter = matchAfter ? parseInt(matchAfter[0]) : 0;

    expect(countAfter).toBeGreaterThan(countBefore);
  });
});
