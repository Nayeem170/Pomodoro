import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Pomo Count Display', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should show pomo count for new task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Pomo Count Task');

    const pomoCount = page.locator('.task-row').filter({ hasText: 'Pomo Count Task' }).locator('.task-pomo-count');
    await expect(pomoCount).toBeVisible();
  });

  test('should show pomo count for completed task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Completed Pomo Task');
    await pomodoroPage.completeTask('Completed Pomo Task');
    await page.waitForTimeout(500);

    const pomoCount = page.locator('.completed-section .task-row').filter({ hasText: 'Completed Pomo Task' }).locator('.task-pomo-count');
    await expect(pomoCount).toBeVisible();
  });

  test('should update pomo count after completing a pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Pomo Update Task');
    await pomodoroPage.selectTask('Pomo Update Task');
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
    if (await consentOption.isVisible()) {
      await consentOption.click();
      await page.waitForTimeout(1000);
    }

    const pomoCount = page.locator('.task-row').filter({ hasText: 'Pomo Update Task' }).locator('.task-pomo-count');
    await expect(pomoCount).toBeVisible();
    const countText = await pomoCount.textContent();
    expect(countText).toMatch(/\d+m/);
  });
});
