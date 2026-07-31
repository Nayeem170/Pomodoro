import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('@T-003 Task/Schedule redesign', () => {
  let page: PomodoroPage;

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
    await page.goto('/');
  });

  test('@T-003-AC15 shows exactly Tasks and Schedule tabs (no google tabs)', async () => {
    await expect(page.page.locator('.ltabs button.lt')).toHaveCount(2);
    await expect(page.page.locator('.ltabs button.lt').filter({ hasText: 'Tasks' })).toBeVisible();
    await expect(page.page.locator('.ltabs button.lt').filter({ hasText: 'Schedule' })).toBeVisible();
  });

  test('@T-003-AC8 schedule tab shows 7 day-grouped rows with disabled prev nav', async () => {
    await page.selectListTab('Schedule');

    await expect(page.page.locator('.schedule-agenda')).toBeVisible();
    await expect(page.page.locator('.sched-day')).toHaveCount(7);
    await expect(page.page.locator('button[aria-label="Previous week"]')).toBeDisabled();
    await expect(page.page.locator('button[aria-label="Next week"]')).toBeEnabled();
  });

  test('@T-003-AC9 scheduled task appears on its day in the agenda', async () => {
    const future = new Date();
    future.setDate(future.getDate() + 3);
    const dateStr = future.toISOString().split('T')[0];

    await page.addTask('Agenda Task');
    await page.editTask('Agenda Task');
    await page.setTaskScheduleDate(dateStr);
    await page.saveTaskEdit();

    await page.selectListTab('Schedule');
    await expect(page.page.locator('.day-item').filter({ hasText: 'Agenda Task' })).toBeVisible();
  });

  test('@T-003-AC8 next week navigates the window and enables prev', async () => {
    await page.selectListTab('Schedule');
    const firstLabel = (await page.page.locator('.sched-window').textContent()) || '';

    await page.page.locator('button[aria-label="Next week"]').click();
    await page.page.waitForTimeout(400);

    const nextLabel = (await page.page.locator('.sched-window').textContent()) || '';
    expect(nextLabel).not.toEqual(firstLabel);
    await expect(page.page.locator('button[aria-label="Previous week"]')).toBeEnabled();
  });

  test('@T-003 repeat task shows interval capsule in the tasks view', async () => {
    await page.addTask('Capsule Task');
    await page.editTask('Capsule Task');
    await page.setTaskRepeat('Daily');
    await page.saveTaskEdit();

    await expect(page.page.locator('.task-row').filter({ hasText: 'Capsule Task' }).locator('.capsule')).toBeVisible();
  });
});
