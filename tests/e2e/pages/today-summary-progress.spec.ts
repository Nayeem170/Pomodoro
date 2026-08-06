import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary Stats', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should show zero pomodoros and zero focus initially', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const cells = page.locator('.summary-cell');
    await expect(cells.nth(0).locator('.summary-val')).toContainText('0m', { timeout: 30000 });
    await expect(cells.nth(1).locator('.summary-val')).toContainText('0', { timeout: 30000 });
  });

  test('should update pomodoro count after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomCellBefore = page.locator('.summary-cell').nth(1).locator('.summary-val');
    await expect(pomCellBefore).toContainText('0', { timeout: 30000 });

    await pomodoroPage.seedHistoryViaDB('Progress Task');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomCellAfter = page.locator('.summary-cell').nth(1).locator('.summary-val');
    await expect(pomCellAfter).toContainText('1');
  });

  test('should display daily goal in pomodoros stat', async ({ page }) => {
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

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomCell = page.locator('.summary-cell').nth(1).locator('.summary-val');
    await expect(pomCell).toContainText('/4', { timeout: 30000 });
  });
});
