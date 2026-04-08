import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
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
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Task Behavior', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should auto-select newly added task as current', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('First Task');
    await pomodoroPage.selectTask('First Task');

    const selectedCountBefore = await page.locator('.task-item.selected').count();
    expect(selectedCountBefore).toBe(1);
    await expect(page.locator('.task-item.selected')).toContainText('First Task');

    await pomodoroPage.addTask('Second Task');

    const selectedCountAfter = await page.locator('.task-item.selected').count();
    expect(selectedCountAfter).toBe(1);
    await expect(page.locator('.task-item.selected')).toContainText('Second Task');
  });

  test('should move completed task to Completed section with undo button', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Complete Test Task');
    await pomodoroPage.selectTask('Complete Test Task');

    await pomodoroPage.completeTask('Complete Test Task');
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section')).toBeVisible();
    await expect(page.locator('.completed-section h4')).toContainText('Completed');

    const completedTask = page.locator('.completed-section .task-item');
    await expect(completedTask).toContainText('Complete Test Task');
    await expect(completedTask).toHaveClass(/completed/);

    const undoButton = completedTask.locator('button.btn-icon[title="Undo"]');
    await expect(undoButton).toBeVisible();

    await undoButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section')).not.toBeVisible();
    const activeTask = page.locator('.task-items').locator('.task-item').filter({ hasText: 'Complete Test Task' });
    await expect(activeTask).toBeVisible();
    await expect(activeTask).not.toHaveClass(/completed/);
  });

  test('should display task stats after completing a pomodoro', async ({ page }) => {
    await completePomodoroFast(page, pomodoroPage, 'Stats Task');

    const taskItem = page.locator('.task-item').filter({ hasText: 'Stats Task' });
    await expect(taskItem).toBeVisible();

    const stats = taskItem.locator('.task-stats');
    await expect(stats).toBeVisible();

    const pomodoroStat = stats.locator('.stat').filter({ hasText: /🍅/ });
    await expect(pomodoroStat).toBeVisible();
    const pomodoroText = await pomodoroStat.textContent();
    const pomodoroCount = parseInt(pomodoroText?.match(/\d+/)?.[0] ?? '0');
    expect(pomodoroCount).toBeGreaterThanOrEqual(1);

    const timeStat = stats.locator('.stat').filter({ hasText: /⏱️/ });
    await expect(timeStat).toBeVisible();
    const timeText = await timeStat.textContent();
    expect(timeText).toMatch(/\d+m/);
  });

  test('should show selected badge only on selected task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Badge Task A');
    await pomodoroPage.addTask('Badge Task B');
    await pomodoroPage.selectTask('Badge Task A');

    const selectedBadge = page.locator('.selected-badge');
    await expect(selectedBadge).toHaveCount(1);
    await expect(page.locator('.task-item.selected')).toContainText('Badge Task A');
  });
});
