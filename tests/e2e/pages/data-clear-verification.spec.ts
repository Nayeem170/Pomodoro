import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Data Clear Verification', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should remove all tasks after clearing data', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Task to Clear 1');
    await pomodoroPage.addTask('Task to Clear 2');
    await page.waitForTimeout(500);

    const taskCountBefore = await page.locator('.task-row').count();
    expect(taskCountBefore).toBeGreaterThanOrEqual(2);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const taskCountAfter = await page.locator('.task-row').count();
    expect(taskCountAfter).toBe(0);
  });

  test('should reset settings to defaults after clearing data', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('30');
    await pomodoroInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await pomodoroPage.addTask('Clear Settings Task');
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await expect(pomodoroInput).toHaveValue('25');
  });

  test('should show toast after data is cleared', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(1000);

    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);
    await expect(page.locator('.settings-toast')).toContainText('cleared');
  });
});
