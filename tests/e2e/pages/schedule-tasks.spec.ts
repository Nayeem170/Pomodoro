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

    await expect(page.page.locator('.day-item-wrap').filter({ hasText: 'Future Task' })).toBeVisible();
  });

  const now = new Date();
  const localDate = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

  test('scheduled task for today is visible in the tasks view', async () => {
    const todayStr = localDate(now);

    await page.addTask('Today Task');
    await page.editTask('Today Task');
    await page.setTaskScheduleDate(todayStr);
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Today Task' })).toBeVisible();

    await page.switchToTaskList('Schedule');
    await expect(page.page.locator('.day-item-wrap').filter({ hasText: 'Today Task' })).toBeVisible();
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

    await expect(page.page.locator('.day-item-wrap').filter({ hasText: 'Schedule Only' })).toBeVisible();
  });

  test('subtask edit sets schedule date and persists across reload', async () => {
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 5);
    const dateStr = futureDate.toISOString().split('T')[0];

    await page.addTask('Subtask Parent');
    const parentRow = page.page.locator('.task-row').filter({ hasText: 'Subtask Parent' }).first();
    await parentRow.locator('button[aria-label="Add subtask"]').click();
    await page.page.locator('.add-subtask-form textarea').fill('Scheduled Subtask');
    await page.page.locator('.add-subtask-form .btn-add').click();

    const subtaskRow = page.page.locator('.task-row').filter({ hasText: 'Scheduled Subtask' }).first();
    await expect(subtaskRow).toBeVisible();
    await expect(subtaskRow.locator('.schedule-badge')).toHaveCount(0);

    await page.editTask('Scheduled Subtask');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await expect(subtaskRow.locator('.schedule-badge')).toBeVisible();

    await page.page.reload();
    await page.page.waitForLoadState('domcontentloaded');
    await page.goto('/');
    const reloadedRow = page.page.locator('.task-row').filter({ hasText: 'Scheduled Subtask' }).first();
    await expect(reloadedRow.locator('.schedule-badge')).toBeVisible();
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

    const item = page.page.locator('.day-item-wrap').filter({ hasText: 'Editable Task' });
    await expect(item).toBeVisible();

    await item.locator('button[aria-label="Edit task"]').click();
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
