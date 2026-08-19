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

test.describe('task keyboard reorder', () => {
  test('Alt+ArrowUp moves the focused task up, announces, keeps focus, persists', async ({ page }) => {
    const po = new PomodoroPage(page);
    await po.goto('/');

    await po.addTask('Key Alpha');
    await po.addTask('Key Beta');
    await po.addTask('Key Gamma');

    const rows = page.locator('.task-row');
    await expect(rows).toHaveCount(3);

    const orderNames = async () => {
      const names: string[] = [];
      const count = await rows.count();
      for (let i = 0; i < count; i++) {
        names.push((await rows.nth(i).textContent()) || '');
      }
      return names;
    };
    const indexOfName = (names: string[], name: string) =>
      names.findIndex(n => n.includes(name));

    const before = await orderNames();
    const betaBefore = indexOfName(before, 'Key Beta');
    const gammaBefore = indexOfName(before, 'Key Gamma');
    expect(betaBefore).toBeGreaterThanOrEqual(0);
    expect(gammaBefore).toBeGreaterThanOrEqual(0);

    await rows.nth(betaBefore).focus();
    await page.keyboard.press('Alt+ArrowUp');
    await page.waitForTimeout(1000);

    const after = await orderNames();
    const betaAfter = indexOfName(after, 'Key Beta');
    expect(betaAfter).toBeLessThan(betaBefore,
      'Beta must render one slot higher after Alt+ArrowUp');

    await expect(rows.nth(betaAfter)).toBeFocused(
      'focus stays on the moved row');

    await expect(page.locator('[aria-live="polite"]')).toContainText(
      /position \d+ of \d+/,
      'the move is announced to screen readers');

    await page.reload({ waitUntil: 'domcontentloaded' });
    await page.waitForFunction(() =>
      document.querySelectorAll('.task-row').length >= 3, { timeout: 30000 });

    const reloaded = await orderNames();
    const betaReloaded = indexOfName(reloaded, 'Key Beta');
    expect(betaReloaded).toBeLessThan(betaBefore,
      'keyboard-reordered position must persist across reload');
  });

  test('Alt+ArrowDown on the last task is a no-op and keeps focus', async ({ page }) => {
    const po = new PomodoroPage(page);
    await po.goto('/');

    await po.addTask('Edge One');
    await po.addTask('Edge Two');

    const rows = page.locator('.task-row');
    await expect(rows).toHaveCount(2);

    const orderNames = async () => {
      const names: string[] = [];
      const count = await rows.count();
      for (let i = 0; i < count; i++) {
        names.push((await rows.nth(i).textContent()) || '');
      }
      return names;
    };

    const before = await orderNames();
    const lastName = before[before.length - 1];
    const lastBefore = before.findIndex(n => n === lastName);
    expect(lastBefore).toBe(before.length - 1,
      'sanity: the last rendered row is the last sibling in its group');

    await rows.last().focus();
    await page.keyboard.press('Alt+ArrowDown');
    await page.waitForTimeout(600);

    const after = await orderNames();
    const lastAfter = after.findIndex(n => n === lastName);
    expect(lastAfter).toBe(lastBefore,
      'Alt+ArrowDown on the last rendered task must not move it');
    await expect(rows.nth(lastAfter)).toBeFocused(
      'focus stays on the row after the no-op');
  });
});
