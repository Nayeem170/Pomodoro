import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

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

async function openPicker(page: PomodoroPage['page'], taskName: string) {
  const row = page.locator('.task-row').filter({ hasText: taskName }).first();
  await row.scrollIntoViewIfNeeded();
  await row.locator('button[aria-label="Demote"]').click();
  await expect(page.locator('.demote-picker')).toHaveCount(1);
}

const activeElementText = (page: PomodoroPage['page']) =>
  page.evaluate(() => document.activeElement?.textContent?.trim() ?? '');

test.describe('Demote picker keyboard and semantics', () => {
  test('trigger exposes menu semantics, picks have no native tooltips', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 560 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');
    await addTasks(po, ['Semantics first', 'Semantics second']);

    const row = page.locator('.task-row').filter({ hasText: 'Semantics second' }).first();
    const trigger = row.locator('button[aria-label="Demote"]');
    await expect(trigger).toHaveAttribute('aria-haspopup', 'menu');
    await expect(trigger).toHaveAttribute('aria-expanded', 'false');

    await trigger.click();
    await expect(trigger).toHaveAttribute('aria-expanded', 'true');
    await expect(page.locator('.demote-picker-list')).toHaveAttribute('role', 'menu');
    await expect(page.locator('.demote-pick').first()).toHaveAttribute('role', 'menuitem');
    await expect(page.locator('.demote-picker-label')).toHaveText('Make subtask of');

    const titles = await page.locator('.demote-pick').evaluateAll(els =>
      els.map(el => el.getAttribute('title'))
    );
    expect(titles.every(t => t === null)).toBe(true);

    await page.locator('.demote-pick-cancel').click();
    await expect(page.locator('.demote-picker')).toHaveCount(0);
    await expect(trigger).toHaveAttribute('aria-expanded', 'false');
  });

  test('escape closes picker and returns focus to the demote trigger', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 560 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');
    await addTasks(po, ['Escape first', 'Escape second']);

    await openPicker(page, 'Escape second');

    await page.keyboard.press('Escape');
    await expect(page.locator('.demote-picker')).toHaveCount(0);
    await expect
      .poll(() => page.evaluate(() => document.activeElement?.getAttribute('aria-label')))
      .toBe('Demote');
  });

  test('arrow keys move focus between sibling picks in list order', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 560 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');
    await addTasks(po, ['Nav first', 'Nav second', 'Nav third']);

    await openPicker(page, 'Nav third');

    const rowNames = (await page.locator('.task-items .task-row .task-text').allTextContents())
      .map(t => t.trim())
      .filter(t => t !== 'Nav third');
    const pickNames = (await page.locator('.demote-picker .demote-pick-name').allTextContents()).map(t =>
      t.trim()
    );
    expect(pickNames, 'picker must mirror the visible sibling order').toEqual(rowNames);
    expect(pickNames.length).toBeGreaterThanOrEqual(2);

    await expect
      .poll(() => activeElementText(page), { timeout: 3000 })
      .toBe(pickNames[0]);
    await page.keyboard.press('ArrowDown');
    await expect
      .poll(() => activeElementText(page), { timeout: 3000 })
      .toBe(pickNames[1]);
    await page.keyboard.press('ArrowDown');
    await expect
      .poll(() => activeElementText(page), { timeout: 3000 })
      .toBe(pickNames[pickNames.length - 1]);
    await page.keyboard.press('ArrowUp');
    await expect
      .poll(() => activeElementText(page), { timeout: 3000 })
      .toBe(pickNames[0]);
  });

  test('long sibling list shows scroll fade and clamps names to two lines', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 560 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');
    const names = Array.from(
      { length: 9 },
      (_, i) => `Fade sibling ${i + 1} with a sufficiently long name to wrap onto a second line`
    );
    await addTasks(po, [...names, 'Fade target']);

    await openPicker(page, 'Fade target');

    const list = page.locator('.demote-picker-list');
    expect(await list.evaluate(el => el.scrollHeight > el.clientHeight)).toBe(true);

    const fade = page.locator('.demote-picker-fade');
    await expect
      .poll(() => fade.evaluate(el => getComputedStyle(el).opacity), { timeout: 3000 })
      .toBe('1');

    await list.evaluate(el => (el.scrollTop = el.scrollHeight));
    await expect
      .poll(() => fade.evaluate(el => getComputedStyle(el).opacity), { timeout: 3000 })
      .toBe('0');

    const clamp = await page
      .locator('.demote-pick-name')
      .first()
      .evaluate(el => (getComputedStyle(el) as unknown as { webkitLineClamp: string }).webkitLineClamp);
    expect(clamp).toBe('2');

    await page.locator('.demote-pick-cancel').click();
    await expect(page.locator('.demote-picker')).toHaveCount(0);
  });
});
