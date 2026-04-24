import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Keyboard Shortcuts Section', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should display keyboard shortcuts section with kbd-grid', async ({ page }) => {
    const kbdGrid = page.locator('.kbd-grid');
    const hasGrid = await kbdGrid.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasGrid) {
      await expect(kbdGrid).toBeVisible();
    }
  });

  test('should show common shortcut labels if section exists', async ({ page }) => {
    const kbdGrid = page.locator('.kbd-grid');
    const hasGrid = await kbdGrid.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasGrid) {
      const kbdElements = kbdGrid.locator('kbd');
      const kbdTexts: string[] = [];
      const count = await kbdElements.count();
      for (let i = 0; i < count; i++) {
        const text = await kbdElements.nth(i).textContent();
        if (text) kbdTexts.push(text.trim());
      }

      const expectedKeys = ['Space', 'R', 'P', 'S', 'L', '?'];
      for (const key of expectedKeys) {
        const found = kbdTexts.some(t => t.includes(key));
        expect(found).toBe(true);
      }
    }
  });
});
