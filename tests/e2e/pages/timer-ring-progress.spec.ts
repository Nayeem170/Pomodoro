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

  test('should render ring-fill SVG element with stroke-dasharray', async ({ page }) => {
    const ringArea = page.locator('.ring-area');
    await expect(ringArea).toBeVisible();

    const ringFill = ringArea.locator('.ring-fill');
    await expect(ringFill).toBeVisible();

    const dashArray = await ringFill.getAttribute('stroke-dasharray');
    expect(dashArray).not.toBeNull();
  });

  test('should update stroke-dashoffset when timer is running', async ({ page }) => {
    const ringFill = page.locator('.ring-area .ring-fill');
    await expect(ringFill).toBeVisible();

    const offsetBefore = await ringFill.getAttribute('stroke-dashoffset');
    const numericOffsetBefore = offsetBefore ? parseFloat(offsetBefore) : 0;

    await pomodoroPage.addTask('Ring Task');
    await pomodoroPage.selectTask('Ring Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await page.waitForTimeout(2000);

    const offsetAfter = await ringFill.getAttribute('stroke-dashoffset');
    const numericOffsetAfter = offsetAfter ? parseFloat(offsetAfter) : 0;

    expect(numericOffsetAfter).toBeGreaterThan(numericOffsetBefore);
  });
});
