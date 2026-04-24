import { Page, expect } from '@playwright/test';

export class PomodoroPage {
  readonly page: Page;
  
  constructor(page: Page) {
    this.page = page;
  }

  async goto(path: string = '/') {
    const consoleErrors: string[] = [];
    this.page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
        console.error('Browser Console Error:', msg.text());
      }
    });

    await this.page.goto(path);
    await this.page.waitForLoadState('domcontentloaded');
    
    try {
      await this.page.waitForFunction(() => {
        const appLoaded = document.querySelector('.main-container') !== null ||
                          document.querySelector('.ring-area') !== null ||
                          document.querySelector('.task-card') !== null ||
                          document.querySelector('#app > div') !== null;
        return appLoaded;
      }, { timeout: 30000 });
      
      await this.page.waitForTimeout(2000);
      
      if (consoleErrors.length > 0) {
        console.log('Console errors detected (ignoring):', consoleErrors.join(', '));
      }
    } catch (error) {
      console.log('Warning: Blazor initialization may not have completed:', error);
      await this.page.screenshot({ path: 'test-results/blazor-init-failure.png' });
    }
  }

  async addTask(taskName: string) {
    await this.page.locator('.task-add-btn').waitFor({ state: 'visible', timeout: 30000 });
    await this.page.locator('.task-add-btn').click();
    await this.page.waitForTimeout(500);
    
    await this.page.locator('.task-input').waitFor({ state: 'visible', timeout: 5000 });
    await this.page.locator('.task-input').pressSequentially(taskName);
    await this.page.waitForTimeout(300);
    
    await this.page.locator('.btn-icon-small.btn-add').click();
    await this.page.waitForTimeout(500);
  }

  async completeTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Complete"]').click();
    await this.page.waitForTimeout(300);
  }

  async uncompleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Undo"]').click();
    await this.page.waitForTimeout(300);
  }

  async deleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.locator('button[aria-label="Delete"]').click();
    await this.page.waitForTimeout(500);
  }

  async selectTask(taskName: string) {
    const taskItem = this.page.locator('.task-row').filter({ hasText: taskName }).first();
    await taskItem.click();
    await this.page.waitForTimeout(300);
  }

  async getTimerDisplay(): Promise<string> {
    const element = this.page.locator('.timer-time');
    return await element.textContent() || '';
  }

  async getTimerType(): Promise<string> {
    const element = this.page.locator('.timer-mode-label');
    return await element.textContent() || '';
  }

  async getTaskCount(): Promise<number> {
    const count = await this.page.locator('.task-row').count();
    return count;
  }

  async isTimerRunning(): Promise<boolean> {
    const pauseButton = this.page.locator('button[aria-label="Pause timer"]');
    return await pauseButton.isVisible();
  }

  async isTimerPaused(): Promise<boolean> {
    const resumeButton = this.page.locator('button[aria-label="Resume timer"]');
    return await resumeButton.isVisible();
  }

  async isTimerStarted(): Promise<boolean> {
    const resetButton = this.page.locator('button[aria-label="Reset timer"]');
    return await resetButton.isVisible();
  }

  async startTimer() {
    const startButton = this.page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await this.page.waitForTimeout(500);
  }

  async pauseTimer() {
    const pauseButton = this.page.locator('button[aria-label="Pause timer"]');
    await pauseButton.click();
    await this.page.waitForTimeout(500);
  }

  async resumeTimer() {
    const resumeButton = this.page.locator('button[aria-label="Resume timer"]');
    await resumeButton.click();
    await this.page.waitForTimeout(500);
  }

  async resetTimer() {
    const resetButton = this.page.locator('button[aria-label="Reset timer"]');
    await resetButton.click();
    await this.page.waitForTimeout(500);
  }

  async switchToPomodoro() {
    const pomodoroButton = this.page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await pomodoroButton.click();
    await this.page.waitForTimeout(300);
  }

  async switchToShortBreak() {
    const shortBreakButton = this.page.locator('.mode-tabs button').filter({ hasText: 'Short break' });
    await shortBreakButton.click();
    await this.page.waitForTimeout(300);
  }

  async switchToLongBreak() {
    const longBreakButton = this.page.locator('.mode-tabs button').filter({ hasText: 'Long break' });
    await longBreakButton.click();
    await this.page.waitForTimeout(300);
  }

  async openSettings() {
    await this.goto('/settings');
  }

  async saveSettings() {
    const saveButton = this.page.locator('.sec-btn').filter({ hasText: 'Save' });
    if (await saveButton.isVisible()) {
      await saveButton.click();
      await this.page.waitForTimeout(500);
    }
  }

  async resetToDefaults() {
    const resetButton = this.page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' });
    await resetButton.click();
    await this.page.waitForTimeout(500);
  }

  async setPomodoroMinutes(minutes: number) {
    const input = this.page.locator('.step-input').first();
    await input.click({ clickCount: 3 });
    await input.pressSequentially(minutes.toString());
    await input.dispatchEvent('input');
    await this.page.waitForTimeout(300);
  }

  async toggleSound() {
    const soundToggle = this.page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    await soundToggle.click();
    await this.page.waitForTimeout(200);
  }

  async toggleNotifications() {
    const notifToggle = this.page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    await notifToggle.click();
    await this.page.waitForTimeout(200);
  }

  async openHistory() {
    await this.goto('/history');
  }

  async switchToDailyTab() {
    const dailyTab = this.page.locator('#daily-tab');
    if (await dailyTab.isVisible()) {
      await dailyTab.click();
      await this.page.waitForTimeout(300);
    }
  }

  async switchToWeeklyTab() {
    const weeklyTab = this.page.locator('#weekly-tab');
    if (await weeklyTab.isVisible()) {
      await weeklyTab.click();
      await this.page.waitForTimeout(300);
    }
  }

  async verifyToast(expectedMessage: string) {
    const toast = this.page.locator('.settings-toast');
    await expect(toast).toBeVisible();
    await expect(toast).toContainText(expectedMessage);
    await this.page.waitForTimeout(2000);
    await expect(toast).not.toBeVisible();
  }

  async openKeyboardHelp() {
    const helpButton = this.page.locator('button[aria-label="Keyboard shortcuts"]');
    await helpButton.click();
    await this.page.waitForTimeout(300);
  }

  async closeKeyboardHelp() {
    const closeButton = this.page.locator('.modal-close');
    if (await closeButton.isVisible()) {
      await closeButton.click();
      await this.page.waitForTimeout(300);
    }
  }

  async togglePipTimer() {
    const pipButton = this.page.locator('button[aria-label="Picture in Picture"]');
    await pipButton.click();
    await this.page.waitForTimeout(500);
  }

  async navigateTo(route: string) {
    await this.page.locator(`.header-nav a[href="${route}"]`).click();
    await this.page.waitForLoadState('domcontentloaded');
    await this.page.waitForTimeout(2000);
  }

  async toggleAutoStartPomodoros() {
    const toggle = this.page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    await toggle.click();
    await this.page.waitForTimeout(200);
  }

  async toggleAutoStartBreaks() {
    const toggle = this.page.locator('.sr-lbl').filter({ hasText: 'Auto-start breaks' }).locator('..').locator('.tog');
    await toggle.click();
    await this.page.waitForTimeout(200);
  }

  async getWeeklyStatValue(label: string): Promise<string> {
    const statItem = this.page.locator('.sc').filter({ hasText: label });
    const valueEl = statItem.locator('.sv');
    return await valueEl.textContent() || '';
  }
}
