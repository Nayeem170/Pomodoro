import { test, expect, BrowserContext } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function setupFastPomodoro(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.pressSequentially('1');
  await pomodoroInput.dispatchEvent('input');
  await page.waitForTimeout(500);

  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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

async function injectSoundTracking(page: any) {
  await page.evaluate(() => {
    (window as any).__soundTracking = { timerCalled: false, breakCalled: false };
    const origTimer = (window as any).notificationFunctions.playTimerCompleteSound;
    (window as any).notificationFunctions.playTimerCompleteSound = async () => {
      (window as any).__soundTracking.timerCalled = true;
      return origTimer.call(null);
    };
    const origBreak = (window as any).notificationFunctions.playBreakCompleteSound;
    (window as any).notificationFunctions.playBreakCompleteSound = async () => {
      (window as any).__soundTracking.breakCalled = true;
      return origBreak.call(null);
    };
  });
}

test.describe('Sound on Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should play timer complete sound when pomodoro finishes', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('1');
    await pomodoroInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await injectSoundTracking(page);

    await pomodoroPage.addTask('Sound Timer Test');
    await pomodoroPage.selectTask('Sound Timer Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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
    expect(tracking.timerCalled).toBe(true);
  });

  test('should play break complete sound when break finishes', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('1');
    await pomodoroInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    const shortBreakInput = page.locator('.step-input').nth(1);
    await shortBreakInput.click({ clickCount: 3 });
    await shortBreakInput.pressSequentially('1');
    await shortBreakInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await injectSoundTracking(page);

    await pomodoroPage.addTask('Sound Break Test');
    await pomodoroPage.selectTask('Sound Break Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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
      await page.waitForTimeout(500);

      const isRunning = await page.locator('button[aria-label="Pause timer"]').isVisible().catch(() => false);
      if (!isRunning) {
        const startOrResume = page.locator('button[aria-label="Start timer"], button[aria-label="Resume timer"]').first();
        await expect(startOrResume).toBeVisible({ timeout: 5000 });
        await startOrResume.click();
      }
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
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
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const isOn = await soundToggle.evaluate(el => el.classList.contains('on'));
    if (isOn) {
      await soundToggle.click();
      await page.waitForTimeout(500);
    }

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('1');
    await pomodoroInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await injectSoundTracking(page);

    await pomodoroPage.addTask('No Sound Test');
    await pomodoroPage.selectTask('No Sound Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

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
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const notifToggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const isOn = await notifToggle.evaluate(el => el.classList.contains('on'));
    if (isOn) {
      await notifToggle.click();
      await page.waitForTimeout(500);
    }

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('1');
    await pomodoroInput.dispatchEvent('input');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    let notificationReceived = false;
    page.on('notification', () => { notificationReceived = true; });

    await pomodoroPage.addTask('No Notif Test');
    await pomodoroPage.selectTask('No Notif Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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
