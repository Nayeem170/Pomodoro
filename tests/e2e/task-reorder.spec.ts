import { test, expect } from '@playwright/test';
import { PomodoroPage } from './fixtures/pomodoro.page';

test.describe('task drag reorder', () => {
  test('dragging a task below another reorders it and persists across reload', async ({ page }) => {
    const po = new PomodoroPage(page);
    await po.goto('/');

    await po.addTask('Reorder Alpha');
    await po.addTask('Reorder Beta');
    await po.addTask('Reorder Gamma');

    const rows = page.locator('.task-row');
    await expect(rows).toHaveCount(3);

    const rowOf = (name: string) => rows.filter({ hasText: name });
    const orderNames = async () => {
      const names: string[] = [];
      const count = await rows.count();
      for (let i = 0; i < count; i++) {
        names.push((await rows.nth(i).textContent()) || '');
      }
      return names;
    };

    const before = await orderNames();
    const alphaBefore = before.findIndex(n => n.includes('Reorder Alpha'));
    const gammaBefore = before.findIndex(n => n.includes('Reorder Gamma'));
    expect(alphaBefore).toBeGreaterThanOrEqual(0);
    expect(gammaBefore).toBeGreaterThanOrEqual(0);
    expect(alphaBefore).not.toBe(gammaBefore);

    await rowOf('Reorder Alpha').first().dragTo(rowOf('Reorder Gamma').first(), {
      targetPosition: { x: 100, y: 28 },
    });

    await page.waitForTimeout(1000);
    const after = await orderNames();
    const alphaAfter = after.findIndex(n => n.includes('Reorder Alpha'));
    const gammaAfter = after.findIndex(n => n.includes('Reorder Gamma'));
    expect(alphaAfter).toBeGreaterThan(gammaAfter,
      'Alpha must render after Gamma after being dragged below it');

    await page.reload({ waitUntil: 'domcontentloaded' });
    await page.waitForFunction(() =>
      document.querySelectorAll('.task-row').length >= 3, { timeout: 30000 });

    const reloaded = await orderNames();
    const alphaReloaded = reloaded.findIndex(n => n.includes('Reorder Alpha'));
    const gammaReloaded = reloaded.findIndex(n => n.includes('Reorder Gamma'));
    expect(alphaReloaded).toBeGreaterThan(gammaReloaded,
      'reordered position must persist across reload');
  });
});
