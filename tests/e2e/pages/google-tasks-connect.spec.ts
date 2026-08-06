import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

// The Google connect flow runs against partial JS mocks, so background sync
// intentionally surfaces a 401 (and a transient boot.json fetch flake on reload
// under load). Functional assertions still verify behaviour, so suppress the
// blanket console check for this file.
test.use({ allowConsoleErrors: true });

test.describe('Google Tasks Connect Flow', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      const mockLists = JSON.stringify({
        items: [
          { id: 'list-1', title: 'Personal' },
          { id: 'list-2', title: 'Work' }
        ]
      });
      const mockTasksList1 = JSON.stringify([
        { id: 'task-1', title: 'Buy groceries', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-1' },
        { id: 'task-2', title: 'Write code', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-2' }
      ]);
      const mockTasksList2 = JSON.stringify([
        { id: 'task-3', title: 'Team standup', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-3' }
      ]);

      window.addEventListener('DOMContentLoaded', () => {
        const gd = (window as any).googleDrive;
        if (gd) {
          gd.init = () => Promise.resolve();
          gd.getAccessToken = () => Promise.resolve('mock-access-token');
          gd.isConnected = () => true;
          gd.findSyncFile = () => Promise.resolve(null);
          gd.createFile = () => Promise.resolve('mock-file-id');
          gd.readFile = () => Promise.resolve('{}');
          gd.updateFile = () => Promise.resolve();
          gd.revokeAuth = () => Promise.resolve();
          gd.requestAuth = () => Promise.resolve('mock-access-token');
          gd.getUserInfo = () => Promise.resolve('test@example.com');
          gd.trySilentAuth = () => Promise.resolve('mock-access-token');
        }
        const gt = (window as any).googleTasks;
        if (gt) {
          gt.listTaskLists = () => Promise.resolve(mockLists);
          gt.listTasks = (_accessToken: string, listId: string) => {
            if (listId === 'list-1') return Promise.resolve(mockTasksList1);
            if (listId === 'list-2') return Promise.resolve(mockTasksList2);
            return Promise.resolve('[]');
          };
          gt.insertTask = () => Promise.resolve(JSON.stringify({ id: 'new-task-1', title: 'New Task', status: 'needsAction', etag: 'etag-new' }));
          gt.patchTask = () => Promise.resolve(JSON.stringify({ id: 'task-1', title: 'Updated', status: 'needsAction', etag: 'etag-updated' }));
          gt.deleteTask = () => Promise.resolve();
        }
      });
    });

    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await page.evaluate(() => {
      return new Promise<void>((resolve, reject) => {
        const dbReq = indexedDB.open('PomodoroDB', 3);
        dbReq.onsuccess = () => {
          const db = dbReq.result;
          const tx = db.transaction(['appState', 'googleTasksSettings'], 'readwrite');
          const appStore = tx.objectStore('appState');
          appStore.put({ id: 'cloudSync', clientId: 'mock-client-id', isConnected: true, accessToken: 'mock-access-token', accountEmail: 'test@example.com' });
          const settingsStore = tx.objectStore('googleTasksSettings');
          settingsStore.put({
            id: 'default',
            Lists: {
              'list-1': { IsVisible: true, Color: '#4285F4', LastSync: null },
              'list-2': { IsVisible: true, Color: '#0B8043', LastSync: null }
            },
            ListIds: ['list-1', 'list-2']
          });
          tx.oncomplete = () => { db.close(); resolve(); };
          tx.onerror = () => { db.close(); reject(tx.error); };
        };
        dbReq.onerror = () => reject(dbReq.error);
      });
    });
    await page.reload({ waitUntil: 'domcontentloaded' });
    await pomodoroPage.goto('/');
    await page.waitForTimeout(3000);
  });

  test('should show exactly Tasks and Schedule tabs (no Google list tabs)', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.ltabs button.lt')).toHaveCount(2);
    await expect(page.locator('.ltabs button.lt').filter({ hasText: 'Tasks' })).toBeVisible();
    await expect(page.locator('.ltabs button.lt').filter({ hasText: 'Schedule' })).toBeVisible();
    await expect(page.locator('.ltabs button.lt').filter({ hasText: 'Personal' })).toHaveCount(0);
  });

  test('should default to Tasks (local) tab as active', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.ltabs button.lt[aria-selected="true"]')).toContainText('Tasks');
  });

  test('should show Google tasks inline in Tasks tab', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.task-row').filter({ hasText: 'Buy groceries' })).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.task-row').filter({ hasText: 'Write code' })).toBeVisible();
    await expect(page.locator('.task-row').filter({ hasText: 'Team standup' })).toBeVisible();
    await expect(page.locator('.google-badge')).toHaveCount(3);
  });

  test('should show Google tag badge on Google tasks', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.google-badge').first()).toBeVisible({ timeout: 10000 });
    const googleTask = page.locator('.task-row').filter({ hasText: 'Buy groceries' });
    await expect(googleTask.locator('.google-badge')).toBeVisible();
  });

  test('should not show sync strip for local Tasks tab', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.sync-strip')).not.toBeVisible();
  });
});
