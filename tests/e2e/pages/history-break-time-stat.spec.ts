import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Break Time Stat', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should show non-zero break time after completing a pomodoro and break', async ({ page }) => {
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
        taskName: 'Break Time Test Task',
        taskId: null,
        completedAt: new Date(Date.now() - 300000).toISOString(),
        durationMinutes: 1,
        wasCompleted: true
      };
      const breakActivity = {
        id: crypto.randomUUID(),
        type: 1,
        taskName: '',
        taskId: null,
        completedAt: now,
        durationMinutes: 5,
        wasCompleted: true
      };
      const tx1 = db.transaction('activities', 'readwrite');
      tx1.objectStore('activities').put(pomoActivity);
      await new Promise<void>((resolve) => { tx1.oncomplete = () => resolve(); });
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

    const breakTimeLabel = page.locator('.sl').filter({ hasText: 'Break time' });
    await expect(breakTimeLabel).toBeVisible();

    const breakTimeValue = breakTimeLabel.locator('..').locator('.sv');
    const breakTimeText = await breakTimeValue.textContent();
    expect(breakTimeText).not.toBe('0m');
    expect(breakTimeText).not.toBe('0');
  });

  test('should display break time stat in daily summary stat grid', async ({ page }) => {
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
      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.totalBreakMinutes = Math.max(stats.totalBreakMinutes || 0, 5);
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

    await expect(page.locator('.stat-grid').first()).toBeVisible();
    await expect(page.locator('.sl').filter({ hasText: 'Break time' })).toBeVisible();
  });
});
