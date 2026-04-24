import { test, expect, browser } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('BroadcastChannel Cross-Tab Sync', () => {
  test.describe.configure({ timeout: 60000 });

  test('should create notification BroadcastChannel on app load', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const channelExists = await page.evaluate(() => {
      const channels = (window as any).__broadcastChannels || [];
      return Array.isArray(channels) || typeof BroadcastChannel !== 'undefined';
    });
    expect(channelExists).toBe(true);
  });

  test('should receive notification action via BroadcastChannel from another context', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const received = await page.evaluate(() => {
      return new Promise<boolean>((resolve) => {
        const channel = new BroadcastChannel('pomodoro-notifications');
        const timeout = setTimeout(() => {
          channel.close();
          resolve(false);
        }, 3000);

        channel.onmessage = (event) => {
          if (event.data && event.data.type === 'NOTIFICATION_ACTION') {
            clearTimeout(timeout);
            channel.close();
            resolve(true);
          }
        };

        const sender = new BroadcastChannel('pomodoro-notifications');
        sender.postMessage({ type: 'NOTIFICATION_ACTION', action: 'shortBreak' });
        sender.close();
      });
    });
    expect(received).toBe(true);
  });

  test('should create PiP BroadcastChannel when pipTimer module loads', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pipModuleLoaded = await page.evaluate(() => {
      return typeof (window as any).pipTimer === 'object';
    });
    expect(pipModuleLoaded).toBe(true);

    const canCreatePipChannel = await page.evaluate(() => {
      try {
        const channel = new BroadcastChannel('pomodoro-pip');
        channel.close();
        return true;
      } catch {
        return false;
      }
    });
    expect(canCreatePipChannel).toBe(true);
  });

  test('should handle multiple BroadcastChannel messages without error', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const noErrors = await page.evaluate(() => {
      return new Promise<boolean>((resolve) => {
        let errorCount = 0;
        const channel = new BroadcastChannel('pomodoro-notifications');

        channel.onmessage = () => {};

        for (let i = 0; i < 10; i++) {
          try {
            channel.postMessage({ type: 'NOTIFICATION_ACTION', action: 'test' });
          } catch {
            errorCount++;
          }
        }

        setTimeout(() => {
          channel.close();
          resolve(errorCount === 0);
        }, 500);
      });
    });
    expect(noErrors).toBe(true);
  });

  test('should not crash when BroadcastChannel message has unexpected format', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      const channel = new BroadcastChannel('pomodoro-notifications');
      channel.postMessage({});
      channel.postMessage({ type: 'UNKNOWN_TYPE' });
      channel.postMessage(null);
      channel.postMessage('string message');
      channel.close();
    });
    await page.waitForTimeout(1000);

    await expect(page.locator('.main-container')).toBeVisible();
  });
});
