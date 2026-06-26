import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

const MOCK_LISTS = JSON.stringify({
  items: [
    { id: 'list-1', title: 'Personal' },
    { id: 'list-2', title: 'Work' }
  ]
});

const MOCK_TASKS_LIST1 = JSON.stringify([
  { id: 'task-1', title: 'Buy groceries', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-1' },
  { id: 'task-2', title: 'Write code', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-2' }
]);

const MOCK_TASKS_LIST2 = JSON.stringify([
  { id: 'task-3', title: 'Team standup', status: 'needsAction', updated: '2026-01-01T00:00:00.000Z', etag: 'etag-3' }
]);

function injectGoogleTasksMocks() {
  window.googleTasks = {
    listTaskLists: function () {
      return Promise.resolve(MOCK_LISTS);
    },
    listTasks: function (accessToken, listId) {
      if (listId === 'list-1') return Promise.resolve(MOCK_TASKS_LIST1);
      if (listId === 'list-2') return Promise.resolve(MOCK_TASKS_LIST2);
      return Promise.resolve('[]');
    },
    insertTask: function () {
      return Promise.resolve(JSON.stringify({ id: 'new-task-1', title: 'New Task', status: 'needsAction', etag: 'etag-new' }));
    },
    patchTask: function () {
      return Promise.resolve(JSON.stringify({ id: 'task-1', title: 'Updated', status: 'needsAction', etag: 'etag-updated' }));
    },
    deleteTask: function () {
      return Promise.resolve();
    }
  };
}

function mockGoogleDriveConnected() {
  const originalInit = window.googleDrive.init;
  const originalIsConnected = window.googleDrive.isConnected;
  const originalGetAccessToken = window.googleDrive.getAccessToken;

  window.googleDrive.init = function () {
    return Promise.resolve();
  };
  window.googleDrive.isConnected = function () {
    return true;
  };
  window.googleDrive.getAccessToken = function () {
    return Promise.resolve('mock-access-token');
  };
  window.googleDrive.trySilentAuth = function () {
    return Promise.resolve(true);
  };
}

test.describe('Google Tasks Connect Flow', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      window.addEventListener('load', () => {
        injectGoogleTasksMocks();
        mockGoogleDriveConnected();
      });
    });
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should show Google list tabs after connecting', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.ltabs button.lt')).toHaveCount(3);
    await expect(page.locator('.ltabs button.lt').filter({ hasText: 'Personal' })).toBeVisible();
    await expect(page.locator('.ltabs button.lt').filter({ hasText: 'Work' })).toBeVisible();
  });

  test('should default to Tasks (local) tab as active', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.ltabs button.lt.act')).toContainText('Tasks');
  });

  test('should switch to Google list tab and show tasks', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await page.locator('.ltabs button.lt').filter({ hasText: 'Personal' }).click();

    await expect(page.locator('.ltabs button.lt.act')).toContainText('Personal');
    await expect(page.locator('.task-row')).toContainText('Buy groceries');
    await expect(page.locator('.task-row')).toContainText('Write code');
  });

  test('should show sync strip for Google list', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await page.locator('.ltabs button.lt').filter({ hasText: 'Personal' }).click();

    await expect(page.locator('.sync-strip')).toBeVisible();
    await expect(page.locator('.sync-label')).toContainText('Google Tasks');
    await expect(page.locator('.sync-list-name')).toContainText('Personal');
  });

  test('should not show sync strip for local Tasks tab', async ({ page }) => {
    await expect(page.locator('.ltabs')).toBeVisible({ timeout: 15000 });
    await expect(page.locator('.sync-strip')).not.toBeVisible();
  });
});
