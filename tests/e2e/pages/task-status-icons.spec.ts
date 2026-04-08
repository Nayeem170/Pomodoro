import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Status Icons', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should show default icon for new task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Icon Test Task');

    const statusIcon = page.locator('.task-item').filter({ hasText: 'Icon Test Task' }).locator('.task-status');
    await expect(statusIcon).toContainText('📝');
  });

  test('should show completed icon for completed task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Complete Icon Task');
    await pomodoroPage.completeTask('Complete Icon Task');
    await page.waitForTimeout(500);

    const statusIcon = page.locator('.completed-section .task-item').filter({ hasText: 'Complete Icon Task' }).locator('.task-status');
    await expect(statusIcon).toContainText('✅');
  });

  test('should show has-pomodoros icon after completing a pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Pomo Icon Task');
    await pomodoroPage.selectTask('Pomo Icon Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
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

    const statusIcon = page.locator('.task-item').filter({ hasText: 'Pomo Icon Task' }).locator('.task-status');
    await expect(statusIcon).toContainText('📋');
  });
});
