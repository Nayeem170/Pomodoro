import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Long Break Activity Timeline Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should render long break entry in history timeline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');

    await page.evaluate(async () => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });

      const now = new Date().toISOString();
      const today = now.split('T')[0];

      const pomodoro1 = {
        id: crypto.randomUUID(),
        type: 0,
        taskName: 'Timeline Task',
        taskId: null,
        completedAt: new Date(Date.now() - 300000).toISOString(),
        durationMinutes: 1,
        wasCompleted: true
      };

      const pomodoro2 = {
        id: crypto.randomUUID(),
        type: 0,
        taskName: 'Timeline Task',
        taskId: null,
        completedAt: new Date(Date.now() - 180000).toISOString(),
        durationMinutes: 1,
        wasCompleted: true
      };

      const longBreak = {
        id: crypto.randomUUID(),
        type: 2,
        taskName: null,
        taskId: null,
        completedAt: new Date(Date.now() - 60000).toISOString(),
        durationMinutes: 1,
        wasCompleted: true
      };

      const tx = db.transaction('activities', 'readwrite');
      const store = tx.objectStore('activities');
      store.put(pomodoro1);
      store.put(pomodoro2);
      store.put(longBreak);
      await new Promise<void>((resolve) => { tx.oncomplete = () => resolve(); });

      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.completedPomodoros = (stats.completedPomodoros || 0) + 2;
        stats.totalFocusMinutes = (stats.totalFocusMinutes || 0) + 2;
        stats.totalBreakMinutes = (stats.totalBreakMinutes || 0) + 1;
        stats.longBreaks = (stats.longBreaks || 0) + 1;
        statsStore.put(stats);
      } else {
        statsStore.put({
          date: today,
          completedPomodoros: 2,
          totalFocusMinutes: 2,
          totalBreakMinutes: 1,
          longBreaks: 1
        });
      }
      await new Promise<void>((resolve) => { statsTx.oncomplete = () => resolve(); });
      db.close();
    });

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const longBreakBadge = page.locator('.tl-badge').filter({ hasText: /Long break/i });
    await expect(longBreakBadge.first()).toBeVisible({ timeout: 5000 });
  });
});
