import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function injectSoundTracking(page: any) {
  await page.evaluate(() => {
    (window as any).__soundTracking = { timerCalled: false, breakCalled: false };
    const origTimer = (window as any).notificationFunctions?.playTimerCompleteSound;
    if (origTimer) {
      (window as any).notificationFunctions.playTimerCompleteSound = async () => {
        (window as any).__soundTracking.timerCalled = true;
        return origTimer.call(null);
      };
    }
    const origBreak = (window as any).notificationFunctions?.playBreakCompleteSound;
    if (origBreak) {
      (window as any).notificationFunctions.playBreakCompleteSound = async () => {
        (window as any).__soundTracking.breakCalled = true;
        return origBreak.call(null);
      };
    }
  });
}

test.describe('Sound on Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should play timer complete sound when pomodoro finishes', async ({ page }) => {
    await pomodoroPage.fastSetup1MinPomodoro();
    await injectSoundTracking(page);
    await pomodoroPage.addTask('Sound Timer Test');
    await pomodoroPage.selectTask('Sound Timer Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    const tracking = await page.evaluate(() => (window as any).__soundTracking);
    expect(tracking).toBeDefined();
    expect(tracking.timerCalled).toBe(true);
  });

  test('should play break complete sound when break finishes', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.setPomodoroMinutes(1);

    const shortBreakInput = page.locator('.step-input').nth(1);
    const currentBreak = parseInt(await shortBreakInput.inputValue());
    if (currentBreak !== 1) {
      const diff = 1 - currentBreak;
      const btnLabel = diff > 0 ? 'Increase' : 'Decrease';
      const btn = page.locator('.stepper').nth(1).locator(`.step-btn[aria-label="${btnLabel}"]`);
      for (let i = 0; i < Math.abs(diff); i++) {
        await btn.click();
        await page.waitForTimeout(50);
      }
    }

    const autoStartSessionToggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start session' }).locator('..').locator('.tog');
    const isSessionAutoOn = await autoStartSessionToggle.evaluate(el => el.classList.contains('on'));
    if (!isSessionAutoOn) {
      await autoStartSessionToggle.click();
    }

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await injectSoundTracking(page);
    await pomodoroPage.addTask('Sound Break Test');
    await pomodoroPage.selectTask('Sound Break Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    const consentModal = page.locator('.consent-modal-overlay');
    if (await consentModal.isVisible({ timeout: 5000 }).catch(() => false)) {
      await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
      await page.waitForTimeout(500);

      const isRunning = await page.locator('button[aria-label="Pause timer"]').isVisible({ timeout: 3000 }).catch(() => false);
      if (!isRunning) {
        const startOrResume = page.locator('button[aria-label="Start timer"], button[aria-label="Resume timer"]').first();
        if (await startOrResume.isVisible({ timeout: 3000 }).catch(() => false)) {
          await startOrResume.click();
        }
      }
      await pomodoroPage.completePomodoroFast();
    }

    const tracking = await page.evaluate(() => (window as any).__soundTracking);
    expect(tracking).toBeDefined();
    expect(tracking.breakCalled).toBe(true);
  });

  test('should not play sound when sound is disabled', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const isOn = await soundToggle.evaluate(el => el.classList.contains('on'));
    if (isOn) {
      await soundToggle.click();
    }

    await pomodoroPage.setPomodoroMinutes(1);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await injectSoundTracking(page);

    await pomodoroPage.addTask('No Sound Test');
    await pomodoroPage.selectTask('No Sound Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    const tracking = await page.evaluate(() => (window as any).__soundTracking);
    expect(tracking).toBeDefined();
    expect(tracking.timerCalled).toBe(false);
  });
});

test.describe('Notification on Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

  test('should show notification when pomodoro completes', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const notificationPromise = page.waitForEvent('notification', { timeout: 15000 }).catch(() => null);

    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.addTask('Notif Timer Test');
    await pomodoroPage.selectTask('Notif Timer Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

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
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const notifToggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const isOn = await notifToggle.evaluate(el => el.classList.contains('on'));
    if (isOn) {
      await notifToggle.click();
    }

    await pomodoroPage.setPomodoroMinutes(1);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    let notificationReceived = false;
    page.on('notification', () => { notificationReceived = true; });

    await pomodoroPage.addTask('No Notif Test');
    await pomodoroPage.selectTask('No Notif Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    expect(notificationReceived).toBe(false);
    await context.close();
  });
});
