import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('PiP Window Content and Communication', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should have pipTimer module loaded with all required methods', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pipModule = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      if (!pip) return null;
      return {
        isSupported: typeof pip.isSupported === 'function',
        isOpen: typeof pip.isOpen === 'function',
        open: typeof pip.open === 'function',
        close: typeof pip.close === 'function',
        update: typeof pip.update === 'function',
        toggleTimer: typeof pip.toggleTimer === 'function',
        resetTimer: typeof pip.resetTimer === 'function',
        switchSession: typeof pip.switchSession === 'function',
        registerDotNetRef: typeof pip.registerDotNetRef === 'function',
        unregisterDotNetRef: typeof pip.unregisterDotNetRef === 'function',
        getBroadcastChannelName: typeof pip.getBroadcastChannelName === 'function',
        getMessageType: typeof pip.getMessageType === 'function',
      };
    });

    expect(pipModule).not.toBeNull();
    expect(pipModule!.isSupported).toBe(true);
    expect(pipModule!.isOpen).toBe(true);
    expect(pipModule!.open).toBe(true);
    expect(pipModule!.close).toBe(true);
    expect(pipModule!.update).toBe(true);
    expect(pipModule!.toggleTimer).toBe(true);
    expect(pipModule!.resetTimer).toBe(true);
    expect(pipModule!.switchSession).toBe(true);
  });

  test('should generate correct timer HTML for PiP window', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const checks = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      const html = pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: true,
        isStarted: true,
        showReset: true,
        taskName: 'Test Task',
        endsAt: '10:47 AM'
      });
      const doc = new DOMParser().parseFromString(html, 'text/html');
      return {
        ringWrap: !!doc.querySelector('.ring-wrap'),
        ringTime: doc.querySelector('.ring-time')?.textContent ?? '',
        ringLabel: doc.querySelector('.ring-label')?.textContent ?? '',
        pipTabs: doc.querySelectorAll('.pip-tab').length,
        pipCtrl: !!doc.querySelector('.pip-ctrl'),
        toggleBtn: !!doc.querySelector('[onclick="window.pipToggleTimer()"]'),
        resetBtn: !!doc.querySelector('[onclick="window.pipResetTimer()"]'),
        pipTask: !!doc.querySelector('.pip-task'),
        taskName: doc.querySelector('.pip-task-name')?.textContent ?? '',
        pipFooter: !!doc.querySelector('.pip-footer'),
        footerText: doc.querySelector('.pip-footer')?.textContent ?? '',
        hasActiveTask: !!doc.querySelector('.active-task'),
        hasCtrlRow: !!doc.querySelector('.ctrl-row'),
      };
    });

    expect(checks.ringWrap).toBe(true);
    expect(checks.ringTime).toBe('25:00');
    expect(checks.ringLabel).toBe('FOCUSING');
    expect(checks.pipTabs).toBe(3);
    expect(checks.pipCtrl).toBe(true);
    expect(checks.toggleBtn).toBe(true);
    expect(checks.resetBtn).toBe(true);
    expect(checks.pipTask).toBe(true);
    expect(checks.taskName).toBe('Test Task');
    expect(checks.pipFooter).toBe(true);
    expect(checks.footerText).toContain('Ends at');
    expect(checks.hasActiveTask).toBe(false);
    expect(checks.hasCtrlRow).toBe(false);
  });

  test('should generate correct HTML for short break session', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const checks = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      const html = pip.generateTimerHTML({
        sessionType: 1,
        remainingSeconds: 300,
        totalDurationSeconds: 300,
        isRunning: false,
        isStarted: true,
        showReset: true,
        taskName: null
      });
      const doc = new DOMParser().parseFromString(html, 'text/html');
      return {
        ringTime: doc.querySelector('.ring-time')?.textContent ?? '',
        ringLabel: doc.querySelector('.ring-label')?.textContent ?? '',
        ringFill: !!doc.querySelector('.ring-fill.short-break'),
        pipCtrl: !!doc.querySelector('.pip-ctrl'),
        pipTask: !!doc.querySelector('.pip-task'),
        hasActiveTask: !!doc.querySelector('.active-task'),
      };
    });

    expect(checks.ringTime).toBe('05:00');
    expect(checks.ringLabel).toBe('SHORT BREAK');
    expect(checks.ringFill).toBe(true);
    expect(checks.pipCtrl).toBe(true);
    expect(checks.pipTask).toBe(false);
    expect(checks.hasActiveTask).toBe(false);
  });

  test('should generate correct HTML for long break session', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const checks = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      const html = pip.generateTimerHTML({
        sessionType: 2,
        remainingSeconds: 900,
        totalDurationSeconds: 900,
        isRunning: true,
        isStarted: true,
        showReset: false,
        taskName: 'Break Task'
      });
      const doc = new DOMParser().parseFromString(html, 'text/html');
      return {
        ringTime: doc.querySelector('.ring-time')?.textContent ?? '',
        ringLabel: doc.querySelector('.ring-label')?.textContent ?? '',
        ringFill: !!doc.querySelector('.ring-fill.long-break'),
        pipTask: !!doc.querySelector('.pip-task'),
      };
    });

    expect(checks.ringTime).toBe('15:00');
    expect(checks.ringLabel).toBe('LONG BREAK');
    expect(checks.ringFill).toBe(true);
    expect(checks.pipTask).toBe(false);
  });

  test('should generate correct ring progress for partially elapsed timer', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const html = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 750,
        totalDurationSeconds: 1500,
        isRunning: true,
        isStarted: true,
        showReset: true,
        taskName: null
      });
    });

    const dashOffsetMatch = html.match(/stroke-dashoffset:\s*([\d.]+)/);
    expect(dashOffsetMatch).not.toBeNull();

    const circumference = 2 * Math.PI * 88;
    const expectedOffset = circumference * 0.5;
    expect(parseFloat(dashOffsetMatch![1])).toBeCloseTo(expectedOffset, 1);
  });

  test('should use correct BroadcastChannel name for PiP communication', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const channelName = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.getBroadcastChannelName();
    });

    expect(channelName).toBe('pomodoro-pip');
  });

  test('should have correct message types defined', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const messageTypes = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      const constants = (window as any).pomodoroConstants;
      return {
        timerUpdate: pip.getMessageType('timerUpdate'),
        toggleTimer: pip.getMessageType('toggleTimer'),
        resetTimer: pip.getMessageType('resetTimer'),
        switchSession: pip.getMessageType('switchSession'),
        hasPipConstants: !!constants?.pip?.messages,
        hasPipCallbacks: !!constants?.pip?.callbacks,
      };
    });

    expect(messageTypes.timerUpdate).toBeTruthy();
    expect(messageTypes.toggleTimer).toBeTruthy();
    expect(messageTypes.resetTimer).toBeTruthy();
    expect(messageTypes.switchSession).toBeTruthy();
    expect(messageTypes.hasPipConstants).toBe(true);
    expect(messageTypes.hasPipCallbacks).toBe(true);
  });

  test('should apply correct theme class for each session type', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const themes = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return {
        pomodoro: pip.getThemeClass(0),
        shortBreak: pip.getThemeClass(1),
        longBreak: pip.getThemeClass(2),
        unknown: pip.getThemeClass(99),
      };
    });

    expect(themes.pomodoro).toBe('pomodoro-theme');
    expect(themes.shortBreak).toBe('short-break-theme');
    expect(themes.longBreak).toBe('long-break-theme');
    expect(themes.unknown).toBe('pomodoro-theme');
  });

  test('should contain interactive controls in redesigned PiP', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const checks = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      const html = pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: true,
        isStarted: true,
        taskName: 'Test Task',
        endsAt: '10:47 AM'
      });
      const doc = new DOMParser().parseFromString(html, 'text/html');
      return {
        toggleBtn: !!doc.querySelector('[onclick="window.pipToggleTimer()"]'),
        resetBtn: !!doc.querySelector('[onclick="window.pipResetTimer()"]'),
        pipCtrl: !!doc.querySelector('.pip-ctrl'),
        pipTask: !!doc.querySelector('.pip-task'),
        taskName: doc.querySelector('.pip-task-name')?.textContent ?? '',
        pipFooter: !!doc.querySelector('.pip-footer'),
        footerText: doc.querySelector('.pip-footer')?.textContent ?? '',
        hasActiveTask: !!doc.querySelector('.active-task'),
        hasCtrlRow: !!doc.querySelector('.ctrl-row'),
        hasCardFooter: !!doc.querySelector('.card-footer'),
      };
    });

    expect(checks.toggleBtn).toBe(true);
    expect(checks.resetBtn).toBe(true);
    expect(checks.pipCtrl).toBe(true);
    expect(checks.pipTask).toBe(true);
    expect(checks.taskName).toBe('Test Task');
    expect(checks.pipFooter).toBe(true);
    expect(checks.footerText).toContain('Ends at');
    expect(checks.footerText).toContain('10:47 AM');
    expect(checks.hasActiveTask).toBe(false);
    expect(checks.hasCtrlRow).toBe(false);
    expect(checks.hasCardFooter).toBe(false);
  });

  test('should handle PiP toggle timer callback without error', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const noError = await page.evaluate(() => {
      return new Promise<boolean>((resolve) => {
        const pip = (window as any).pipTimer;
        try {
          pip.toggleTimer();
        } catch {
          resolve(false);
          return;
        }
        setTimeout(() => resolve(true), 500);
      });
    });
    expect(noError).toBe(true);
  });

  test('should handle PiP reset timer callback without error', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const noError = await page.evaluate(() => {
      return new Promise<boolean>((resolve) => {
        const pip = (window as any).pipTimer;
        try {
          pip.resetTimer();
        } catch {
          resolve(false);
          return;
        }
        setTimeout(() => resolve(true), 500);
      });
    });
    expect(noError).toBe(true);
  });

  test('should handle PiP switch session callback without error', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const noError = await page.evaluate(() => {
      return new Promise<boolean>((resolve) => {
        const pip = (window as any).pipTimer;
        try {
          pip.switchSession(1);
        } catch {
          resolve(false);
          return;
        }
        setTimeout(() => resolve(true), 500);
      });
    });
    expect(noError).toBe(true);
  });

  test('should include session switch keyboard shortcuts in PiP window script', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const scriptContent = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      if (!pip.ensurePipScript) return null;
      return pip.ensurePipScript.toString();
    });
    expect(scriptContent).not.toBeNull();
    expect(scriptContent).toContain('pipSwitchSession');
    expect(scriptContent).toContain('keydown');
    expect(scriptContent).toContain('BroadcastChannel');
  });
});
