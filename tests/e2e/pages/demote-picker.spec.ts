import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

const localDate = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

async function addTasks(po: PomodoroPage, names: string[]) {
  for (const name of names) {
    const before = await po.getTaskCount();
    for (let attempt = 0; attempt < 3; attempt++) {
      await po.page.locator('.task-input').fill('');
      await po.page.locator('.task-input').fill(name);
      const addButton = po.page.locator('.btn-add-text');
      await expect(addButton).toBeEnabled({ timeout: 5000 });
      await addButton.click();
      const added = await expect
        .poll(() => po.getTaskCount(), { timeout: 8000 })
        .toBe(before + 1)
        .then(() => true)
        .catch(() => false);
      if (added) break;
      if (attempt === 2) throw new Error('addTasks failed for ' + name);
    }
  }
}

async function expectPickerFullyVisible(page: PomodoroPage['page'], containerSelector: string) {
  const picker = page.locator('.demote-picker');
  await expect(picker).toHaveCount(1);
  await expect(picker.locator('.demote-pick-cancel')).toBeVisible();
  await expect(picker.locator('.demote-pick').first()).toBeVisible();

  await expect
    .poll(async () => {
      const containerBox = await page.locator(containerSelector).boundingBox();
      const pickerBox = await picker.boundingBox();
      if (!containerBox || !pickerBox) return -1;
      if (pickerBox.y < containerBox.y) return -1;
      return pickerBox.y + pickerBox.height - (containerBox.y + containerBox.height);
    }, { timeout: 5000 })
    .toBeLessThanOrEqual(0);
}

test.describe('Demote picker visibility', () => {
  test('picker opened on bottom task row is fully visible in tasks view', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 560 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');

    const names = ['Regular Activities', 'Ongoing Tasks'];
    for (let i = 1; i <= 8; i++) names.push(`Filler task ${i}`);
    await addTasks(po, names);

    const bottomRow = page.locator('.task-row').filter({ hasText: 'Regular Activities' }).first();
    await bottomRow.scrollIntoViewIfNeeded();
    await bottomRow.locator('button[aria-label="Demote"]').click();

    await expectPickerFullyVisible(page, '.task-items');

    await page.locator('.demote-pick-cancel').click();
    await expect(page.locator('.demote-picker')).toHaveCount(0);
  });

  test('picker opened near agenda bottom is fully visible in schedule view', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');

    const names = ['Regular Activities', 'Ongoing Tasks'];
    for (let i = 1; i <= 6; i++) names.push(`Sched filler ${i}`);
    await addTasks(po, names);

    const future = new Date();
    future.setDate(future.getDate() + 6);
    const dateStr = localDate(future);
    for (const name of names) {
      await po.editTask(name);
      await po.setTaskScheduleDate(dateStr);
      await po.saveTaskEdit();
    }

    await po.switchToTaskList('Schedule');
    const target = page.locator('.day-item-wrap').filter({ hasText: 'Regular Activities' }).first();
    await expect(target).toBeVisible({ timeout: 10000 });
    await target.scrollIntoViewIfNeeded();
    await target.locator('button[aria-label="Demote"]').click();

    await expectPickerFullyVisible(page, '.sched-days');

    await page.locator('.demote-pick-cancel').click();
    await expect(page.locator('.demote-picker')).toHaveCount(0);
  });

  test('picker keeps full height in overflowing task list (no flex crush)', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 600 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');

    const names = Array.from({ length: 22 }, (_, i) => `test ${i + 1}`);
    await addTasks(po, names);

    const midRow = page.locator('.task-row').filter({ hasText: 'test 21' }).first();
    await midRow.scrollIntoViewIfNeeded();
    await midRow.locator('button[aria-label="Demote"]').click();

    const picker = page.locator('.demote-picker');
    await expect(picker).toHaveCount(1);

    const pickerHeight = await picker.evaluate(el => el.clientHeight);
    expect(pickerHeight, 'picker box must not be crushed by flex-shrink below its max-height budget').toBeGreaterThanOrEqual(100);

    await expect(picker.locator('.demote-pick-cancel')).toBeVisible();
    await page.locator('.demote-pick-cancel').click();
    await expect(picker).toHaveCount(0);
  });
});
