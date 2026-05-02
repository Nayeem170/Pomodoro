import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Repeat Tasks', () => {
  let page: PomodoroPage;

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
    await page.goto('/');
  });

  test('set daily repeat on task shows badge', async () => {
    await page.addTask('Daily Task');
    await page.editTask('Daily Task');
    await page.setTaskRepeat('Daily');
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Daily Task' }).locator('.task-badge.task-repeat')).toBeVisible();
  });

  test('set weekly repeat with day selection', async () => {
    await page.addTask('Weekly Task');
    await page.editTask('Weekly Task');
    await page.setTaskRepeat('Weekly');
    await page.page.locator('.tep-weekday-btn').filter({ hasText: 'Mo' }).click();
    await page.page.locator('.tep-weekday-btn').filter({ hasText: 'We' }).click();
    await page.page.locator('.tep-weekday-btn').filter({ hasText: 'Fr' }).click();
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Weekly Task' }).locator('.task-badge.task-repeat')).toBeVisible();
  });

  test('set custom repeat with N days', async () => {
    await page.addTask('Custom Task');
    await page.editTask('Custom Task');
    await page.setTaskRepeat('Custom');
    await page.page.locator('.tep-input-sm').fill('3');
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Custom Task' }).locator('.task-badge.task-repeat')).toBeVisible();
  });

  test('set monthly repeat', async () => {
    await page.addTask('Monthly Task');
    await page.editTask('Monthly Task');
    await page.setTaskRepeat('Monthly');
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Monthly Task' }).locator('.task-badge.task-repeat')).toBeVisible();
  });

  test('pause recurring task shows paused badge', async () => {
    await page.addTask('Paused Task');
    await page.editTask('Paused Task');
    await page.setTaskRepeat('Daily');
    await page.toggleTaskPause();
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Paused Task' }).locator('.task-badge.repeat-paused')).toBeVisible();
  });

  test('cancel edit does not save repeat', async () => {
    await page.addTask('No Repeat Task');
    await page.editTask('No Repeat Task');
    await page.setTaskRepeat('Daily');
    await page.cancelTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'No Repeat Task' }).locator('.task-badge.task-repeat')).not.toBeVisible();
  });

  test('recurring task stays in list after completion', async () => {
    await page.addTask('Recurring Complete');
    await page.editTask('Recurring Complete');
    await page.setTaskRepeat('Daily');
    await page.saveTaskEdit();
    await page.selectTask('Recurring Complete');
    await page.fastSetup1MinPomodoro();
    await page.startTimer();
    await page.completePomodoroFast();
    await page.skipConsentModal();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Recurring Complete' })).toBeVisible();
    await expect(page.page.locator('.task-row').filter({ hasText: 'Recurring Complete' }).locator('.task-checkbox')).not.toHaveClass('completed');
  });
});
