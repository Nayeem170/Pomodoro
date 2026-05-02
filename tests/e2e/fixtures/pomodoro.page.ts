import { Page, expect } from '@playwright/test';

export class PomodoroPage {
  readonly page: Page;
  private _clockInstalled = false;

  constructor(page: Page) {
    this.page = page;
  }

  async goto(path: string = '/') {
    await this.page.goto(path, { waitUntil: 'domcontentloaded' });

    try {
      await this.page.waitForFunction(() => {
        return document.querySelector('.main-container') !== null ||
               document.querySelector('.sett-body') !== null ||
               document.querySelector('.hist-body') !== null ||
               document.querySelector('.about-body') !== null ||
               document.querySelector('.error-container') !== null ||
               document.querySelector('p[role="alert"]') !== null;
      }, { timeout: 30000 });
    } catch (error) {
      await this.page.screenshot({ path: 'test-results/blazor-init-failure.png' });
      throw new Error('Blazor failed to initialize within 30s for path: ' + path);
    }
  }

  async addTask(taskName: string) {
    await this.page.locator('.task-add-btn').waitFor({ state: 'visible', timeout: 30000 });
    await this.page.locator('.task-add-btn').click();
    await this.page.locator('.task-input').waitFor({ state: 'visible', timeout: 5000 });
    await this.page.locator('.task-input').pressSequentially(taskName);
    await this.page.locator('.btn-icon-small.btn-add').click();
  }

  async completeTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Complete"]').click();
  }

  async uncompleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('.task-action-btn[aria-label="Undo"]').click();
  }

  async deleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Delete"]').click();
  }

  async selectTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.click();
  }

  async getTimerDisplay(): Promise<string> {
    return await this.page.locator('.timer-time').textContent() || '';
  }

  async getTimerType(): Promise<string> {
    return await this.page.locator('.timer-mode-label').textContent() || '';
  }

  async getTaskCount(): Promise<number> {
    return await this.page.locator('.task-row').count();
  }

  async isTimerRunning(): Promise<boolean> {
    return await this.page.locator('button[aria-label="Pause timer"]').isVisible();
  }

  async isTimerPaused(): Promise<boolean> {
    return await this.page.locator('button[aria-label="Resume timer"]').isVisible();
  }

  async isTimerStarted(): Promise<boolean> {
    return await this.page.locator('button[aria-label="Reset timer"]').isVisible();
  }

  async startTimer() {
    if (!this._clockInstalled) {
      await this.page.clock.install({ time: Date.now() });
      this._clockInstalled = true;
    }
    await this.page.locator('button[aria-label="Start timer"]').click();
    await expect(this.page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
  }

  async pauseTimer() {
    await this.page.locator('button[aria-label="Pause timer"]').click();
    await expect(this.page.locator('button[aria-label="Resume timer"]')).toBeVisible({ timeout: 5000 });
  }

  async resumeTimer() {
    await this.page.locator('button[aria-label="Resume timer"]').click();
    await expect(this.page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
  }

  async resetTimer() {
    await this.page.locator('button[aria-label="Reset timer"]').click();
  }

  async switchToPomodoro() {
    await this.page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' }).click();
  }

  async switchToShortBreak() {
    await this.page.locator('.mode-tabs button').filter({ hasText: 'Short break' }).click();
  }

  async switchToLongBreak() {
    await this.page.locator('.mode-tabs button').filter({ hasText: 'Long break' }).click();
  }

  async openSettings() {
    await this.goto('/settings');
  }

  async saveSettings() {
    const saveButton = this.page.locator('.sec-btn').filter({ hasText: 'Save' });
    if (await saveButton.isVisible()) {
      await saveButton.click();
    }
  }

  async resetToDefaults() {
    const resetButton = this.page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' });
    if (await resetButton.isEnabled({ timeout: 2000 }).catch(() => false)) {
      await resetButton.click();
      await this.page.waitForTimeout(3000);
    }
  }

  async setPomodoroMinutes(minutes: number) {
    const input = this.page.locator('.step-input').first();
    const currentValue = parseInt(await input.inputValue());
    const diff = minutes - currentValue;
    if (diff === 0) return;

    const btnLabel = diff > 0 ? 'Increase' : 'Decrease';
    const btn = this.page.locator('.step-btn[aria-label="' + btnLabel + '"]').first();

    for (let i = 0; i < Math.abs(diff); i++) {
      await btn.click();
      await this.page.waitForTimeout(50);
    }
  }

  async setSettingViaIndexedDB(key: string, value: any) {
    await this.page.evaluate(async ({ key: k, value: v }) => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });
      const tx = db.transaction('settings', 'readwrite');
      const store = tx.objectStore('settings');
      const getReq = store.get('default');
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const settings = getReq.result;
      if (settings) {
        settings[k] = v;
        store.put(settings);
      }
      await new Promise<void>((resolve) => { tx.oncomplete = () => resolve(); });
      db.close();
    }, { key, value });
  }

  async fastSetup1MinPomodoro() {
    await this.goto('/settings');
    await this.setPomodoroMinutes(1);
    await this.goto('/');
    await expect(this.page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  }

  async completePomodoroFast() {
    await this.page.evaluate(async () => {
      const ref = (window as any).timerFunctions?.dotNetRef;
      if (!ref) return false;
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 65; i++) {
        try {
          await ref.invokeMethodAsync('OnTimerTickJs');
        } catch { return true; }
        await delay(1000);
      }
      return true;
    });
    await this.page.waitForTimeout(500);
  }

  async completePomodoroViaDB(taskName: string) {
    await this.page.evaluate(async (name) => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });

      const now = new Date().toISOString();
      const activity = {
        id: crypto.randomUUID(),
        type: 0,
        taskName: name,
        taskId: null,
        completedAt: now,
        durationMinutes: 1,
        wasCompleted: true
      };

      const tx = db.transaction('activities', 'readwrite');
      tx.objectStore('activities').put(activity);
      await new Promise<void>((resolve) => { tx.oncomplete = () => resolve(); });

      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const today = now.split('T')[0];
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.completedPomodoros = (stats.completedPomodoros || 0) + 1;
        stats.totalFocusMinutes = (stats.totalFocusMinutes || 0) + 1;
        statsStore.put(stats);
      } else {
        statsStore.put({
          date: today,
          completedPomodoros: 1,
          totalFocusMinutes: 1,
          totalBreakMinutes: 0,
          longBreaks: 0
        });
      }
      await new Promise<void>((resolve) => { statsTx.oncomplete = () => resolve(); });
      db.close();
    }, taskName);
  }

  async completePomodoroViaIndexedDB(taskName: string) {
    await this.page.evaluate(async (name) => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });

      const now = new Date().toISOString();

      const activity = {
        id: crypto.randomUUID(),
        type: 0,
        taskName: name,
        taskId: null,
        completedAt: now,
        durationMinutes: 1,
        wasCompleted: true
      };

      const tx = db.transaction('activities', 'readwrite');
      tx.objectStore('activities').put(activity);
      await new Promise<void>((resolve) => { tx.oncomplete = () => resolve(); });

      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const today = now.split('T')[0];
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.completedPomodoros = (stats.completedPomodoros || 0) + 1;
        stats.totalFocusMinutes = (stats.totalFocusMinutes || 0) + 1;
        statsStore.put(stats);
      } else {
        statsStore.put({
          date: today,
          completedPomodoros: 1,
          totalFocusMinutes: 1,
          totalBreakMinutes: 0,
          longBreaks: 0
        });
      }
      await new Promise<void>((resolve) => { statsTx.oncomplete = () => resolve(); });
      db.close();
    }, taskName);
  }

  async completePomodoroFast1Min() {
    await this.fastSetup1MinPomodoro();
    await this.addTask('Test Task');
    await this.selectTask('Test Task');
    await this.startTimer();
    await this.completePomodoroFast();
  }

  async seedHistoryViaDB(taskName: string, count: number = 1) {
    await this.goto('/');
    await this.page.evaluate(async ({ name, count: n }) => {
      const db = await new Promise<IDBDatabase>((resolve, reject) => {
        const req = indexedDB.open('PomodoroDB', 1);
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
      });

      const now = new Date().toISOString();
      const today = now.split('T')[0];

      for (let i = 0; i < n; i++) {
        const offset = (n - i) * 120000;
        const startedAt = new Date(Date.now() - 120000 - offset).toISOString();
        const completedAt = new Date(Date.now() - offset).toISOString();

        const activity = {
          id: crypto.randomUUID(),
          type: 0,
          taskName: name,
          taskId: null,
          completedAt: completedAt,
          durationMinutes: 1,
          wasCompleted: true
        };

        const tx = db.transaction('activities', 'readwrite');
        tx.objectStore('activities').put(activity);
        await new Promise<void>((resolve) => { tx.oncomplete = () => resolve(); });
      }

      const statsTx = db.transaction('dailyStats', 'readwrite');
      const statsStore = statsTx.objectStore('dailyStats');
      const getReq = statsStore.get(today);
      await new Promise<void>((resolve) => { getReq.onsuccess = () => resolve(); });
      const stats = getReq.result;
      if (stats) {
        stats.completedPomodoros = (stats.completedPomodoros || 0) + n;
        stats.totalFocusMinutes = (stats.totalFocusMinutes || 0) + n;
        statsStore.put(stats);
      } else {
        statsStore.put({
          date: today,
          completedPomodoros: n,
          totalFocusMinutes: n,
          totalBreakMinutes: 0,
          longBreaks: 0
        });
      }
      await new Promise<void>((resolve) => { statsTx.oncomplete = () => resolve(); });
      db.close();
    }, { name: taskName, count });
    await this.page.reload({ waitUntil: 'domcontentloaded' });
    await this.page.waitForFunction(() => {
      return document.querySelector('.main-container') !== null;
    }, { timeout: 30000 });
  }

  async toggleSound() {
    const soundToggle = this.page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    await soundToggle.click();
  }

  async toggleNotifications() {
    const notifToggle = this.page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    await notifToggle.click();
  }

  async openHistory() {
    await this.goto('/history');
  }

  async switchToDailyTab() {
    const dailyTab = this.page.locator('#daily-tab');
    if (await dailyTab.isVisible()) {
      await dailyTab.click();
    }
  }

  async switchToWeeklyTab() {
    const weeklyTab = this.page.locator('#weekly-tab');
    if (await weeklyTab.isVisible()) {
      await weeklyTab.click();
    }
  }

  async verifyToast(expectedMessage: string) {
    const toast = this.page.locator('.settings-toast');
    await expect(toast).toBeVisible();
    await expect(toast).toContainText(expectedMessage);
    await expect(toast).not.toBeVisible({ timeout: 5000 });
  }

  async openKeyboardHelp() {
    await this.page.locator('button[aria-label="Keyboard shortcuts"]').click();
    await expect(this.page.locator('.modal-close')).toBeVisible({ timeout: 5000 });
  }

  async closeKeyboardHelp() {
    const closeButton = this.page.locator('.modal-close');
    if (await closeButton.isVisible()) {
      await closeButton.click();
    }
  }

  async togglePipTimer() {
    await this.page.locator('button[aria-label="Picture in Picture"]').click();
  }

  async navigateTo(route: string) {
    await this.page.locator(`.header-nav a[href="${route}"]`).click();
    await this.page.waitForLoadState('domcontentloaded');
  }

  async toggleAutoStartSession() {
    const toggle = this.page.locator('.sr-lbl').filter({ hasText: 'Auto-start session' }).locator('..').locator('.tog');
    await toggle.click();
  }

  async getWeeklyStatValue(label: string): Promise<string> {
    const statItem = this.page.locator('.sc').filter({ hasText: label });
    return await statItem.locator('.sv').textContent() || '';
  }

  async skipConsentModal() {
    const skipOption = this.page.locator('.btn-option').filter({ hasText: /Skip/i });
    if (await skipOption.isVisible({ timeout: 2000 }).catch(() => false)) {
      await skipOption.click();
      await expect(skipOption).not.toBeVisible({ timeout: 3000 });
    }
  }

  async editTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Edit task"]').click();
    await this.page.locator('.task-edit-panel').waitFor({ state: 'visible', timeout: 5000 });
  }

  async saveTaskEdit() {
    await this.page.locator('.tep-save-btn').click();
    await this.page.locator('.task-edit-panel').waitFor({ state: 'hidden', timeout: 5000 });
  }

  async cancelTaskEdit() {
    await this.page.locator('.tep-cancel-btn').click();
    await this.page.locator('.task-edit-panel').waitFor({ state: 'hidden', timeout: 5000 });
  }

  async setTaskRepeat(type: string) {
    await this.page.locator('.tep-select').selectOption(type);
  }

  async setTaskScheduleDate(dateStr: string) {
    await this.page.locator('.tep-row').filter({ hasText: 'Schedule' }).locator('input[type="date"]').fill(dateStr);
  }

  async toggleTaskPause() {
    await this.page.locator('.tep-toggle').click();
  }

  async hasRepeatBadge(taskName: string): Promise<boolean> {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    return await taskItem.locator('.task-badge.task-repeat').isVisible().catch(() => false);
  }

  async hasScheduleBadge(taskName: string): Promise<boolean> {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    return await taskItem.locator('.task-badge.task-scheduled').isVisible().catch(() => false);
  }

  async hasPausedBadge(taskName: string): Promise<boolean> {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    return await taskItem.locator('.task-badge.repeat-paused').isVisible().catch(() => false);
  }
}
