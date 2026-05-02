import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Schedule Tasks', () => {
  let page: PomodoroPage;

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
    await page.goto('/');
  });

  test('schedule task for future date shows schedule badge', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 7);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Future Task');
    await page.editTask('Future Task');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Future Task' }).locator('.task-badge.task-scheduled')).toBeVisible();
  });

  test('scheduled task for today is visible in task list', async () => {
    const todayStr = new Date().toISOString().split('T')[0];

    await page.addTask('Today Task');
    await page.editTask('Today Task');
    await page.setTaskScheduleDate(todayStr);
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Today Task' })).toBeVisible();
  });

  test('schedule date available without repeat', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 3);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Schedule Only');
    await page.editTask('Schedule Only');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Schedule Only' })).toBeVisible();
  });
});
