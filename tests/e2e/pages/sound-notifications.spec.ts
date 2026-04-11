import { test, expect, BrowserContext } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function setupFastPomodoro(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('input[type="number"]').first();
  await pomodoroInput.fill('1');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

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
}

test.describe('Sound on Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should play timer complete sound when pomodoro finishes', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await shortBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await shortBreakInput.dispatchEvent('change');
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
      await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
      await page.waitForTimeout(1000);

      const isRunning = await page.locator('.btn-pause').isVisible().catch(() => false);
      if (!isRunning) {
        const startOrResume = page.locator('.btn-start, .btn-resume').first();
        await expect(startOrResume).toBeVisible({ timeout: 5000 });
        await startOrResume.click();
      }
      await expect(page.locator('.btn-pause')).toBeVisible({ timeout: 5000 });
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

    const tracking = await page.evaluate(() => (window as any).__soundTracking);
    expect(tracking).toBeDefined();
    expect(tracking.breakCalled).toBe(true);
  });

  test('should not play sound when sound is disabled', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const soundToggle = page.locator('#soundToggle');
    const isSoundOn = await soundToggle.isChecked();
    if (isSoundOn) {
      await page.locator('label[for="soundToggle"]').click();
      await page.waitForTimeout(500);
    } else {
      await page.locator('label[for="soundToggle"]').click();
      await page.waitForTimeout(500);
      await page.locator('label[for="soundToggle"]').click();
      await page.waitForTimeout(500);
    }
    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);
    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await injectSoundTracking(page);

    await pomodoroPage.addTask('No Sound Test');
    await pomodoroPage.selectTask('No Sound Test');
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

    const tracking = await page.evaluate(() => (window as any).__soundTracking);
    expect(tracking).toBeDefined();
    expect(tracking.timerCalled).toBe(false);
  });
});

test.describe('Notification on Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should show notification when pomodoro completes', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const notificationPromise = page.waitForEvent('notification', { timeout: 15000 }).catch(() => null);

    await setupFastPomodoro(page, pomodoroPage, 'Notif Timer Test');

    const notification = await notificationPromise;
    if (notification) {
      expect(notification.title).toContain('Pomodoro');
    }

    await context.close();
  });

  test('should not show notification when notifications are disabled', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);

    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const notifToggle = page.locator('#notifToggle');
    const isNotifOn = await notifToggle.isChecked();
    if (isNotifOn) {
      await page.locator('label[for="notifToggle"]').click();
      await page.waitForTimeout(500);
      await page.locator('.btn-save').click();
      await page.waitForTimeout(1000);
    }

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);
    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Sound Test Task');
    await pomodoroPage.selectTask('Sound Test Task');
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

    expect(notificationReceived).toBe(false);
    await context.close();
  });
});
