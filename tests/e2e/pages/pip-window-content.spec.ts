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

    const html = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: true,
        isStarted: true,
        showReset: true,
        taskName: 'Test Task'
      });
    });

    expect(html).toContain('25:00');
    expect(html).toContain('FOCUSING');
    expect(html).toContain('Test Task');
    expect(html).toContain('pip-container');
    expect(html).toContain('ring-area');
    expect(html).toContain('ttime');
    expect(html).toContain('tmode');
    expect(html).toContain('mode-tab');
    expect(html).toContain('active-task');
    expect(html).toContain('ctrl-row');
  });

  test('should generate correct HTML for short break session', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const html = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 1,
        remainingSeconds: 300,
        totalDurationSeconds: 300,
        isRunning: false,
        isStarted: true,
        showReset: true,
        taskName: null
      });
    });

    expect(html).toContain('05:00');
    expect(html).toContain('SHORT BREAK');
    expect(html).toContain('short-break');
    expect(html).not.toContain('active-task');
  });

  test('should generate correct HTML for long break session', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const html = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 2,
        remainingSeconds: 900,
        totalDurationSeconds: 900,
        isRunning: true,
        isStarted: true,
        showReset: false,
        taskName: 'Break Task'
      });
    });

    expect(html).toContain('15:00');
    expect(html).toContain('LONG BREAK');
    expect(html).toContain('long-break');
    expect(html).toContain('Break Task');
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

    const circumference = 2 * Math.PI * 81;
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

  test('should show play icon when timer is paused and pause icon when running', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const runningHtml = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: true,
        isStarted: true,
        showReset: true,
        taskName: null
      });
    });

    const pausedHtml = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: false,
        isStarted: true,
        showReset: true,
        taskName: null
      });
    });

    expect(runningHtml).toContain('\u23F8');
    expect(pausedHtml).toContain('\u25B6');
  });

  test('should show reset button only when showReset is true', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const withReset = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: false,
        isStarted: true,
        showReset: true,
        taskName: null
      });
    });

    const withoutReset = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      return pip.generateTimerHTML({
        sessionType: 0,
        remainingSeconds: 1500,
        totalDurationSeconds: 1500,
        isRunning: false,
        isStarted: false,
        showReset: false,
        taskName: null
      });
    });

    expect(withReset).toContain('pipResetTimer');
    expect(withoutReset).not.toContain('pipResetTimer');
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

  test('should include keyboard shortcuts in PiP window script', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const hasKeyboardShortcuts = await page.evaluate(() => {
      const pip = (window as any).pipTimer;
      if (!pip.ensurePipScript) return false;
      const scriptContent = pip.toString();
      return scriptContent.includes('pipToggleTimer') &&
        scriptContent.includes('pipResetTimer') &&
        scriptContent.includes('pipSwitchSession');
    });
    expect(hasKeyboardShortcuts).toBe(true);
  });
});
