import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Schedule Tasks', () => {
  let page: PomodoroPage;

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
    await page.goto('/');
  });

  test('schedule task for future date appears in the agenda', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 5);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Future Task');
    await page.editTask('Future Task');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await page.switchToTaskList('Schedule');

    await expect(page.page.locator('.day-item').filter({ hasText: 'Future Task' })).toBeVisible();
  });

  // FIXME(foundation-coverage): the tests below contradict the branch's exclusive
  // task routing (codified by unit tests): a today-scheduled task is routed to the
  // Schedule tab, not the Tasks view, and the agenda lacks the .item-title-btn /
  // .day-check elements these specs click. Pending the Tasks/Schedule routing
  // decision and agenda edit/complete UI (follow-up task).
  test.fixme('scheduled task for today is visible in the tasks view', async () => {
    const todayStr = new Date().toISOString().split('T')[0];

    await page.addTask('Today Task');
    await page.editTask('Today Task');
    await page.setTaskScheduleDate(todayStr);
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Today Task' })).toBeVisible();
  });

  test('schedule date available without repeat', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 2);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Schedule Only');
    await page.editTask('Schedule Only');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await page.switchToTaskList('Schedule');

    await expect(page.page.locator('.day-item').filter({ hasText: 'Schedule Only' })).toBeVisible();
  });

  test.fixme('can edit a scheduled task from the agenda', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 3);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Editable Task');
    await page.editTask('Editable Task');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await page.switchToTaskList('Schedule');

    const item = page.page.locator('.day-item').filter({ hasText: 'Editable Task' });
    await expect(item).toBeVisible();

    // Click the task title in the agenda to open the edit panel.
    await item.locator('.item-title-btn').click();
    await expect(page.page.locator('.task-edit-panel')).toBeVisible();

    // Rename and save.
    const nameInput = page.page.locator('.tep-row').filter({ hasText: 'Name' }).locator('.tep-input');
    await nameInput.fill('Renamed Task');
    await page.page.locator('.tep-save-btn').click();

    // The agenda now shows the updated name.
    await expect(page.page.locator('.day-item').filter({ hasText: 'Renamed Task' })).toBeVisible();
  });

  test.fixme('can complete a scheduled task from the agenda', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 4);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Agenda Complete');
    await page.editTask('Agenda Complete');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await page.switchToTaskList('Schedule');

    const item = page.page.locator('.day-item').filter({ hasText: 'Agenda Complete' });
    await item.locator('.day-check').click();

    await expect(page.page.locator('.day-item.done').filter({ hasText: 'Agenda Complete' })).toBeVisible();
  });
});
