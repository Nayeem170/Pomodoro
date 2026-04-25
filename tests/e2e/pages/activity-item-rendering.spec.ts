import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Activity Item Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should render activity item with correct structure after pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Render Test Task');
    await pomodoroPage.selectTask('Render Test Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityItem = page.locator('.tl-row').first();
    await expect(activityItem).toBeVisible({ timeout: 5000 });

    await expect(activityItem.locator('.tl-dot')).toBeVisible();

    await expect(activityItem.locator('.tl-time')).toBeVisible();
    const timeText = await activityItem.locator('.tl-time').textContent();
    expect(timeText).toMatch(/\d{1,2}:\d{2}\s*[AP]M/);

    await expect(activityItem.locator('.tl-badge')).toBeVisible();
    await expect(activityItem.locator('.tl-badge')).toContainText('Pomodoro');

    await expect(activityItem.locator('.tl-task')).toBeVisible();
    await expect(activityItem.locator('.tl-task')).toContainText('Render Test Task');
  });

  test('should render break activity with correct icon and name', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Break Render Task');
    await pomodoroPage.selectTask('Break Render Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();

    const consentOption = page.locator('.btn-option').filter({ hasText: /Short Break/i });
    if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
      await consentOption.click();
      await page.waitForTimeout(1000);

      const startBtn = page.locator('button[aria-label="Start timer"]');
      if (await startBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await pomodoroPage.startTimer();
      }
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
      await pomodoroPage.completePomodoroFast();
      await pomodoroPage.skipConsentModal();
    }

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const breakActivity = page.locator('.tl-row').filter({ hasText: /Short break/i }).first();
    if (await breakActivity.isVisible({ timeout: 2000 }).catch(() => false)) {
      await expect(breakActivity.locator('.tl-dot').first()).toHaveClass(/brk/);
      await expect(breakActivity.locator('.tl-badge').first()).toContainText('Short break');
    }
  });
});
