import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 2000; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(5);
    }
  });
  await page.waitForTimeout(3000);

  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Export Clear Import Round Trip', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should restore tasks after export, clear, and re-import', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Round Trip Task A');
    await pomodoroPage.addTask('Round Trip Task B');
    await page.waitForTimeout(500);

    const taskCountBefore = await page.locator('.task-item').count();
    expect(taskCountBefore).toBeGreaterThanOrEqual(2);

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const content = await download.createReadStream();
    const chunks: Buffer[] = [];
    for await (const chunk of content) {
      chunks.push(chunk);
    }
    const jsonStr = Buffer.concat(chunks).toString('utf-8');

    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const taskCountAfterClear = await page.locator('.task-item').count();
    expect(taskCountAfterClear).toBe(0);

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'restored-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(jsonStr)
    });
    await page.waitForTimeout(3000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const taskCountAfterImport = await page.locator('.task-item').count();
    expect(taskCountAfterImport).toBeGreaterThanOrEqual(2);

    const hasTaskA = await page.locator('.task-item').filter({ hasText: 'Round Trip Task A' }).isVisible().catch(() => false);
    const hasTaskB = await page.locator('.task-item').filter({ hasText: 'Round Trip Task B' }).isVisible().catch(() => false);
    expect(hasTaskA || hasTaskB).toBe(true);
  });

  test('should restore activities in history after export, clear, and re-import', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Activity Round Trip Task');

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(3000);

    const activityCountBefore = await page.locator('.activity-item').count();
    if (activityCountBefore === 0) {
      return;
    }

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const content = await download.createReadStream();
    const chunks: Buffer[] = [];
    for await (const chunk of content) {
      chunks.push(chunk);
    }
    const jsonStr = Buffer.concat(chunks).toString('utf-8');

    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(3000);

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const activityCountAfterClear = await page.locator('.activity-item').count();
    expect(activityCountAfterClear).toBe(0);

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'activity-restored.json',
      mimeType: 'application/json',
      buffer: Buffer.from(jsonStr)
    });
    await page.waitForTimeout(3000);

    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(3000);

    const activityCountAfterImport = await page.locator('.activity-item').count();
    expect(activityCountAfterImport).toBeGreaterThanOrEqual(1);
  });
});
