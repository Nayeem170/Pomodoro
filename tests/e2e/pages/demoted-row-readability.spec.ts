import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

const READABLE_TEXT_WIDTH = 120;

async function demoteUnder(po: PomodoroPage, taskName: string, targetName: string) {
  const row = po.page.locator('.task-row').filter({ hasText: taskName }).first();
  await row.locator('button[aria-label="Demote"]').click();
  const pick = po.page.locator('.demote-picker .demote-pick').filter({ hasText: targetName });
  await expect(pick).toBeVisible({ timeout: 5000 });
  await pick.click();
  await expect(po.page.locator('.demote-picker')).toHaveCount(0, { timeout: 5000 });
  await po.page.waitForTimeout(300);
}

async function measureText(page: PomodoroPage['page'], taskName: string) {
  const row = page.locator('.task-row').filter({ hasText: taskName }).first();
  await expect(row).toBeVisible({ timeout: 8000 });
  return await row.evaluate((el) => {
    const text = el.querySelector('.task-text');
    if (!text) return null;
    const actions = el.querySelector('.task-actions');
    const rect = text.getBoundingClientRect();
    return {
      textClientWidth: text.clientWidth,
      textHeight: Math.round(rect.height),
      rowClientWidth: el.clientWidth,
      rowScrollWidth: el.scrollWidth,
      actionsFlexShrink: actions ? getComputedStyle(actions).flexShrink : null,
      textMinWidth: getComputedStyle(text).minWidth,
    };
  });
}

test.describe('Demoted task row readability', () => {
  test('demoted task text stays readable at mobile width (depth 1)', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');

    await po.addTask('Repro parent');
    await po.addTask('Repro child item');
    await demoteUnder(po, 'Repro child item', 'Repro parent');

    const m = await measureText(page, 'Repro child item');
    expect(m, 'demoted row must render a .task-text element').not.toBeNull();
    expect(m!.rowScrollWidth, 'row must not overflow horizontally').toBeLessThanOrEqual(m!.rowClientWidth + 1);
    expect(
      m!.textClientWidth,
      `demoted task text column must be readable (>= ${READABLE_TEXT_WIDTH}px), got ${m!.textClientWidth}px; ` +
      `row ${m!.rowClientWidth}px, actions flex-shrink ${m!.actionsFlexShrink}, text min-width ${m!.textMinWidth}`
    ).toBeGreaterThanOrEqual(READABLE_TEXT_WIDTH);
  });

  test('demoted task text stays readable at depth 2', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    const po = new PomodoroPage(page);
    await po.goto('http://localhost:5000/');

    await po.addTask('Root A');
    await po.addTask('Mid B');
    await po.addTask('Leaf C');
    await demoteUnder(po, 'Mid B', 'Root A');
    await demoteUnder(po, 'Leaf C', 'Root A');
    await demoteUnder(po, 'Leaf C', 'Mid B');

    const m = await measureText(page, 'Leaf C');
    expect(m, 'depth-2 row must render a .task-text element').not.toBeNull();
    expect(m!.rowScrollWidth, 'row must not overflow horizontally').toBeLessThanOrEqual(m!.rowClientWidth + 1);
    expect(
      m!.textClientWidth,
      `depth-2 task text column must be readable (>= ${READABLE_TEXT_WIDTH}px), got ${m!.textClientWidth}px`
    ).toBeGreaterThanOrEqual(READABLE_TEXT_WIDTH);
  });
});
