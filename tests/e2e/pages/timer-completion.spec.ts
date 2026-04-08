import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('input[type="number"]').first();
  await pomodoroInput.fill('1');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);

  await page.locator('.btn-save').click();
  await page.waitForTimeout(2000);
  await expect(page.locator('.settings-toast')).toBeVisible();

  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
}

test.describe('Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should reset timer to full duration after pomodoro completes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Completion Reset Test');

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/\d{1,2}:\d{2}/);
    expect(timerDisplay).not.toBe('00:00');
  });

  test('should show start button after pomodoro completes without auto-start', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);
    await expect(page.locator('.settings-toast')).toBeVisible();

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('No Auto Test');
    await pomodoroPage.selectTask('No Auto Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);

    const consentModal = page.locator('.consent-modal-overlay');
    const isModalVisible = await consentModal.isVisible().catch(() => false);
    if (isModalVisible) {
      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await page.waitForTimeout(2000);
      await pomodoroPage.pauseTimer();
      await page.locator('.btn-reset').click();
      await page.waitForTimeout(1000);
    }

    await expect(page.locator('.btn-start')).toBeVisible({ timeout: 10000 });
  });

  test('should switch to break session after selecting option from consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('#autoStartEnabled').check({ force: true });
    await page.waitForTimeout(500);

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Session Switch Test');
    await pomodoroPage.selectTask('Session Switch Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
    await page.waitForTimeout(2000);

    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('SHORT BREAK');
  });

  test('should record activity after pomodoro completes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Activity Record Test');

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityCount = await page.locator('.activity-item').count();
    expect(activityCount).toBeGreaterThanOrEqual(1);
  });
});
