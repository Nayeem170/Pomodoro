import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Activity Item Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should render activity item with correct structure after pomodoro', async ({ page }) => {
    await pomodoroPage.seedHistoryViaDB('Render Test Task');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tl-row').first()).toBeVisible({ timeout: 5000 });

    const activityItem = page.locator('.tl-row').first();
    await expect(activityItem.locator('.tl-dot')).toBeVisible();
    await expect(activityItem.locator('.tl-time')).toBeVisible();
    const timeText = await activityItem.locator('.tl-time').textContent();
    expect(timeText).toMatch(/\d{1,2}:\d{2}\s*[AP]M/);
    await expect(activityItem.locator('.tl-badge')).toBeVisible();
    await expect(activityItem.locator('.tl-badge')).toContainText('Pomodoro');
    await expect(activityItem.locator('.tl-task')).toBeVisible();
    await expect(activityItem.locator('.tl-task')).toContainText('Render Test Task');
  });

  test('should render break activity with correct icon and name', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await pomodoroPage.page.evaluate(async () => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });
      const now = new Date().toISOString();
      const today = now.split('T')[0];

      const pomoActivity = {
        id: crypto.randomUUID(),
        type: 0,
        taskName: 'Break Render Task',
        taskId: null,
        completedAt: new Date(Date.now() - 300000).toISOString(),
        durationMinutes: 1,
        wasCompleted: true
      };
      const tx1 = db.transaction('activities', 'readwrite');
      tx1.objectStore('activities').put(pomoActivity);
      await new Promise<void>((resolve) => { tx1.oncomplete = () => resolve(); });

      const breakActivity = {
        id: crypto.randomUUID(),
        type: 1,
        taskName: '',
        taskId: null,
        completedAt: now,
        durationMinutes: 5,
        wasCompleted: true
      };
      const tx2 = db.transaction('activities', 'readwrite');
      tx2.objectStore('activities').put(breakActivity);
      await new Promise<void>((resolve) => { tx2.oncomplete = () => resolve(); });

      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.completedPomodoros = (stats.completedPomodoros || 0) + 1;
        stats.totalFocusMinutes = (stats.totalFocusMinutes || 0) + 1;
        stats.totalBreakMinutes = (stats.totalBreakMinutes || 0) + 5;
        statsStore.put(stats);
      } else {
        statsStore.put({
          date: today,
          completedPomodoros: 1,
          totalFocusMinutes: 1,
          totalBreakMinutes: 5,
          longBreaks: 0
        });
      }
      await new Promise<void>((resolve) => { statsTx.oncomplete = () => resolve(); });
      db.close();
    });

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const breakActivity = page.locator('.tl-row').filter({ hasText: /Short break/i }).first();
    await expect(breakActivity).toBeVisible({ timeout: 5000 });
    await expect(breakActivity.locator('.tl-dot').first()).toHaveClass(/brk/);
    await expect(breakActivity.locator('.tl-badge').first()).toContainText('Short break');
  });
});
