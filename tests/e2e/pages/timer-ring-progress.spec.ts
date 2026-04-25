import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Ring SVG Progress', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should render ring-fill SVG element with radius', async ({ page }) => {
    const ringArea = page.locator('.ring-area');
    await expect(ringArea).toBeVisible();

    const ringFill = ringArea.locator('.ring-fill');
    await expect(ringFill).toBeVisible();

    const radius = await ringFill.getAttribute('r');
    expect(parseInt(radius!)).toBeGreaterThan(0);
  });

  test('should have stroke-dasharray via computed style', async ({ page }) => {
    const ringFill = page.locator('.ring-area .ring-fill');
    await expect(ringFill).toBeVisible();

    const dashArray = await ringFill.evaluate(el => getComputedStyle(el).strokeDasharray);
    expect(dashArray).not.toBe('none');
  });

  test('should update stroke-dashoffset when timer is running', async ({ page }) => {
    const ringFill = page.locator('.ring-area .ring-fill');
    await expect(ringFill).toBeVisible();

    await pomodoroPage.addTask('Ring Task');
    await pomodoroPage.selectTask('Ring Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await page.waitForTimeout(2000);

    const offset = await ringFill.evaluate(el => parseFloat(getComputedStyle(el).strokeDashoffset));
    expect(offset).toBeGreaterThan(0);
  });
});
