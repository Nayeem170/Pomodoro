import { test, expect } from '@playwright/test';
import { PomodoroPage } from './fixtures/pomodoro.page';

test.describe('task drag reorder', () => {
  test('dragging a task below another reorders it and persists across reload', async ({ page }) => {
    const po = new PomodoroPage(page);
    await po.goto('/');

    await po.addTask('Reorder A');
    await po.addTask('Reorder B');
    await po.addTask('Reorder C');

    const rows = page.locator('.task-row');
    await expect(rows).toHaveCount(3);
    await expect(rows.nth(0)).toContainText('Reorder A');

    const source = rows.nth(0);
    await source.dragTo(rows.nth(2), { targetPosition: { x: 100, y: 28 } });

    await expect(rows.nth(0)).toContainText('Reorder B', { timeout: 10000 });
    await expect(rows.nth(2)).toContainText('Reorder A');

    await page.reload({ waitUntil: 'domcontentloaded' });
    await page.waitForFunction(() =>
      document.querySelectorAll('.task-row').length >= 3, { timeout: 30000 });

    const reloaded = page.locator('.task-row');
    await expect(reloaded.nth(0)).toContainText('Reorder B');
    await expect(reloaded.nth(2)).toContainText('Reorder A');
  });
});
