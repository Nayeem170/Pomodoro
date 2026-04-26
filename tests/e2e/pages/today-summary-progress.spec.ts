import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary Progress Bar', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should show progress bar fill with 0% width when no pomodoros completed', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.progress-bar')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.progress-bar-fill')).toBeAttached();

    const widthStyle = await page.locator('.progress-bar-fill').getAttribute('style');
    expect(widthStyle).toContain('width:');
    expect(widthStyle).toContain('0%');
  });

  test('should increase progress bar fill width after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const fillBefore = page.locator('.progress-bar-fill');
    const styleBefore = await fillBefore.getAttribute('style');
    const widthBeforeMatch = styleBefore?.match(/width:\s*([\d.]+)%/);
    const widthBefore = widthBeforeMatch ? parseFloat(widthBeforeMatch[1]) : 0;

    await pomodoroPage.seedHistoryViaDB('Progress Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const fillAfter = page.locator('.progress-bar-fill');
    await expect(fillAfter).toBeAttached();
    const styleAfter = await fillAfter.getAttribute('style');
    const widthAfterMatch = styleAfter?.match(/width:\s*([\d.]+)%/);
    const widthAfter = widthAfterMatch ? parseFloat(widthAfterMatch[1]) : 0;

    expect(widthAfter).toBeGreaterThan(widthBefore);
  });

  test('should calculate progress bar fill width as percentage of daily goal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await pomodoroPage.setPomodoroMinutes(1);

    const dailyGoalInput = page.locator('.step-input').nth(3);
    const currentGoal = parseInt(await dailyGoalInput.inputValue());
    const diff = 4 - currentGoal;
    if (diff !== 0) {
      const btnLabel = diff > 0 ? 'Increase' : 'Decrease';
      const btn = page.locator('.step-btn[aria-label="' + btnLabel + '"]').nth(3);
      for (let i = 0; i < Math.abs(diff); i++) {
        await btn.click();
        await page.waitForTimeout(50);
      }
    }

    await pomodoroPage.seedHistoryViaDB('Progress Calc Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const fill = page.locator('.progress-bar-fill');
    await expect(fill).toBeAttached();
    const style = await fill.getAttribute('style');
    const widthMatch = style?.match(/width:\s*([\d.]+)%/);
    const widthPercent = widthMatch ? parseFloat(widthMatch[1]) : 0;

    expect(widthPercent).toBeCloseTo(25, 0);
  });
});
