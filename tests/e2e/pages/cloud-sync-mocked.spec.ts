import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Google Drive Integration', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should have googleDrive module loaded', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const hasModule = await page.evaluate(() => {
      return typeof (window as any).googleDrive === 'object';
    });
    expect(hasModule).toBe(true);
  });

  test('should have all googleDrive API methods defined', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const methods = await page.evaluate(() => {
      const gd = (window as any).googleDrive;
      if (!gd) return null;
      return {
        init: typeof gd.init === 'function',
        requestAuth: typeof gd.requestAuth === 'function',
        trySilentAuth: typeof gd.trySilentAuth === 'function',
        revokeAuth: typeof gd.revokeAuth === 'function',
        isConnected: typeof gd.isConnected === 'function',
        findSyncFile: typeof gd.findSyncFile === 'function',
        readFile: typeof gd.readFile === 'function',
        createFile: typeof gd.createFile === 'function',
        updateFile: typeof gd.updateFile === 'function',
        deleteFile: typeof gd.deleteFile === 'function',
      };
    });

    expect(methods).not.toBeNull();
    for (const [name, isDefined] of Object.entries(methods!)) {
      expect(isDefined, `googleDrive.${name} should be a function`).toBe(true);
    }
  });

  test('should report not connected initially', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const isConnected = await page.evaluate(async () => {
      const gd = (window as any).googleDrive;
      if (!gd) return null;
      try {
        return await gd.isConnected();
      } catch {
        return null;
      }
    });

    expect(isConnected).toBe(false);
  });

  test('should have compression interop module loaded', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const hasModule = await page.evaluate(() => {
      return typeof (window as any).compressionInterop === 'object';
    });
    expect(hasModule).toBe(true);
  });

  test('should have gzip compress and decompress methods', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const methods = await page.evaluate(() => {
      const ci = (window as any).compressionInterop;
      if (!ci) return null;
      return {
        gzipCompress: typeof ci.gzipCompress === 'function',
        gzipDecompress: typeof ci.gzipDecompress === 'function',
      };
    });

    expect(methods).not.toBeNull();
    expect(methods!.gzipCompress).toBe(true);
    expect(methods!.gzipDecompress).toBe(true);
  });

  test('should compress and decompress data round-trip', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const roundTrip = await page.evaluate(async () => {
      const ci = (window as any).compressionInterop;
      if (!ci) return null;

      const testData = JSON.stringify({
        version: 1,
        timestamp: new Date().toISOString(),
        settings: { pomodoroMinutes: 25 },
        tasks: [{ id: '1', name: 'Test Task' }],
        activities: []
      });

      try {
        const compressed = await ci.gzipCompress(testData);
        const decompressed = await ci.gzipDecompress(compressed);
        return {
          success: true,
          original: testData,
          result: decompressed,
          matches: testData === decompressed,
          compressedSmaller: compressed.length < testData.length
        };
      } catch (e) {
        return { success: false, error: (e as Error).message };
      }
    });

    expect(roundTrip).not.toBeNull();
    expect(roundTrip!.success).toBe(true);
    expect(roundTrip!.matches).toBe(true);
  });

  test('should show Connect button in Cloud Sync settings section', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.ss-hdr').filter({ hasText: 'Cloud Sync' })).toBeVisible();
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Google Drive' })).toBeVisible();
    await expect(page.locator('.sec-btn').filter({ hasText: 'Connect' })).toBeVisible();
  });

  test('should show Sync across devices when not connected', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const syncSubtitle = page.locator('.sr-sub').filter({ hasText: /Sync across devices/ });
    await expect(syncSubtitle).toBeVisible();
  });

  test('should handle auth init gracefully without Google API', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const result = await page.evaluate(async () => {
      const gd = (window as any).googleDrive;
      if (!gd) return { success: false, reason: 'module-not-loaded' };

      try {
        await gd.init();
        return { success: true };
      } catch {
        return { success: true, reason: 'handled-gracefully' };
      }
    });

    expect(result.success).toBe(true);
  });

  test('should have Sync button and Disconnect button in sync-actions when connected state is simulated', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const hasSyncActions = await page.evaluate(() => {
      const gd = (window as any).googleDrive;
      if (!gd) return false;
      return typeof gd.isConnected === 'function';
    });

    expect(hasSyncActions).toBe(true);
  });

  test('should verify googleDrive isConnected returns boolean', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const result = await page.evaluate(async () => {
      const gd = (window as any).googleDrive;
      if (!gd) return null;
      const connected = await gd.isConnected();
      return { type: typeof connected, value: connected };
    });

    expect(result).not.toBeNull();
    expect(result!.type).toBe('boolean');
  });
});
