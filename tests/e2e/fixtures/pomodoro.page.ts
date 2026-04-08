import { Page, expect } from '@playwright/test';

export class PomodoroPage {
  readonly page: Page;
  
  constructor(page: Page) {
    this.page = page;
  }

  async goto(path: string = '/') {
    // Set up console error logging before navigation
    const consoleErrors: string[] = [];
    this.page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
        console.error('Browser Console Error:', msg.text());
      }
    });

    await this.page.goto(path);
    await this.page.waitForLoadState('domcontentloaded');
    
    // Wait for Blazor WebAssembly to fully initialize
    // This waits for the WASM runtime to download, compile, and render components
    try {
      // First wait for the blazor script to load
      await this.page.waitForFunction(() => {
        // Check if Blazor has started by looking for the blazor-loaded marker
        // or if our app content is rendered
        const appLoaded = document.querySelector('.main-container') !== null ||
                          document.querySelector('.timer-section') !== null ||
                          document.querySelector('.tasks-section') !== null ||
                          document.querySelector('#app > div') !== null;
        return appLoaded;
      }, { timeout: 30000 }); // 30 second timeout for WASM initialization
      
      // Additional wait for components to fully render
      await this.page.waitForTimeout(2000);
      
      // Log all console errors for debugging but don't fail the test
      if (consoleErrors.length > 0) {
        console.log('Console errors detected (ignoring):', consoleErrors.join(', '));
      }
    } catch (error) {
      // Log error but continue - let individual tests fail with clearer error messages
      console.log('Warning: Blazor initialization may not have completed:', error);
      
      // Take a screenshot for debugging
      await this.page.screenshot({ path: 'test-results/blazor-init-failure.png' });
    }
  }

  async addTask(taskName: string) {
    // Wait for and click the "Add Task" button to show the input field
    // Increased timeout to 30 seconds for Blazor WASM initialization
    await this.page.locator('.btn-add-task').waitFor({ state: 'visible', timeout: 30000 });
    await this.page.locator('.btn-add-task').click();
    await this.page.waitForTimeout(500); // Increased wait after click
    
    // Fill in the task name
    await this.page.locator('.task-input').waitFor({ state: 'visible', timeout: 5000 });
    await this.page.locator('.task-input').fill(taskName);
    await this.page.waitForTimeout(300);
    
    // Click the add button
    await this.page.locator('.btn-icon-small.btn-add').click();
    await this.page.waitForTimeout(500);
  }

  async completeTask(taskName: string) {
    const taskItem = this.page.locator('.task-item').filter({ hasText: taskName }).first();
    await taskItem.locator('button:has-text("✓")').click();
    await this.page.waitForTimeout(300);
  }

  async uncompleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-item').filter({ hasText: taskName }).first();
    await taskItem.locator('button:has-text("↩")').click();
    await this.page.waitForTimeout(300);
  }

  async deleteTask(taskName: string) {
    const taskItem = this.page.locator('.task-item').filter({ hasText: taskName }).first();
    await taskItem.locator('button:has-text("🗑")').click();
    await this.page.waitForTimeout(500);
  }

  async selectTask(taskName: string) {
    const taskItem = this.page.locator('.task-item').filter({ hasText: taskName }).first();
    await taskItem.click();
    await this.page.waitForTimeout(300);
  }

  async getTimerDisplay(): Promise<string> {
    const element = this.page.locator('.timer-time');
    return await element.textContent() || '';
  }

  async getTimerType(): Promise<string> {
    const element = this.page.locator('.timer-type');
    return await element.textContent() || '';
  }

  async getTaskCount(): Promise<number> {
    const count = await this.page.locator('.task-item').count();
    return count;
  }

  async isTimerRunning(): Promise<boolean> {
    // Check if pause button is visible (timer is running)
    const pauseButton = this.page.locator('.btn-pause');
    return await pauseButton.isVisible();
  }

  async isTimerPaused(): Promise<boolean> {
    // Check if resume button is visible (timer is paused)
    const resumeButton = this.page.locator('.btn-resume');
    return await resumeButton.isVisible();
  }

  async isTimerStarted(): Promise<boolean> {
    // Check if reset button is visible (timer was started)
    const resetButton = this.page.locator('.btn-reset');
    return await resetButton.isVisible();
  }

  async startTimer() {
    const startButton = this.page.locator('.btn-start');
    await startButton.click();
    await this.page.waitForTimeout(500);
  }

  async pauseTimer() {
    const pauseButton = this.page.locator('.btn-pause');
    await pauseButton.click();
    await this.page.waitForTimeout(500);
  }

  async resumeTimer() {
    const resumeButton = this.page.locator('.btn-resume');
    await resumeButton.click();
    await this.page.waitForTimeout(500);
  }

  async resetTimer() {
    const resetButton = this.page.locator('.btn-reset');
    await resetButton.click();
    await this.page.waitForTimeout(500);
  }

  async switchToPomodoro() {
    const pomodoroButton = this.page.locator('button:has-text("Pomodoro")');
    await pomodoroButton.click();
    await this.page.waitForTimeout(300);
  }

  async switchToShortBreak() {
    const shortBreakButton = this.page.locator('button:has-text("Short Break")');
    await shortBreakButton.click();
    await this.page.waitForTimeout(300);
  }

  async switchToLongBreak() {
    const longBreakButton = this.page.locator('button:has-text("Long Break")');
    await longBreakButton.click();
    await this.page.waitForTimeout(300);
  }

  async openSettings() {
    await this.goto('/settings');
  }

  async saveSettings() {
    const saveButton = this.page.locator('.btn-save');
    await saveButton.click();
    await this.page.waitForTimeout(500);
  }

  async resetToDefaults() {
    const resetButton = this.page.locator('.btn-reset-defaults');
    await resetButton.click();
    await this.page.waitForTimeout(500);
  }

  async setPomodoroMinutes(minutes: number) {
    const input = this.page.locator('input[type="number"]').filter({ hasText: '' }).first();
    await input.fill(minutes.toString());
    await this.page.waitForTimeout(200);
  }

  async toggleSound() {
    const toggle = this.page.locator('label[for="soundToggle"]');
    await toggle.click();
    await this.page.waitForTimeout(200);
  }

  async toggleNotifications() {
    const toggle = this.page.locator('label[for="notifToggle"]');
    await toggle.click();
    await this.page.waitForTimeout(200);
  }

  async exportData() {
    const exportButton = this.page.locator('.btn-export');
    await exportButton.click();
    await this.page.waitForTimeout(1000);
  }

  async openHistory() {
    await this.goto('/history');
  }

  async switchToDailyTab() {
    const dailyTab = this.page.locator('button:has-text("Daily")');
    if (await dailyTab.isVisible()) {
      await dailyTab.click();
      await this.page.waitForTimeout(300);
    }
  }

  async switchToWeeklyTab() {
    const weeklyTab = this.page.locator('button:has-text("Weekly")');
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
    const helpButton = this.page.locator('button:has-text("?")');
    await helpButton.click();
    await this.page.waitForTimeout(300);
  }

  async closeKeyboardHelp() {
    const closeButton = this.page.locator('button:has-text("Close"), .modal-close');
    if (await closeButton.isVisible()) {
      await closeButton.click();
      await this.page.waitForTimeout(300);
    }
  }

  async togglePipTimer() {
    const pipButton = this.page.locator('button:has-text("⧉")');
    await pipButton.click();
    await this.page.waitForTimeout(500);
  }

  async navigateTo(route: string) {
    await this.page.locator(`.header-nav a[href="${route}"]`).click();
    await this.page.waitForLoadState('domcontentloaded');
    await this.page.waitForTimeout(2000);
  }

  async getWeeklyStatValue(label: string): Promise<string> {
    const statItem = this.page.locator('.stat-item, .stat').filter({ hasText: label });
    const valueEl = statItem.locator('.stat-value');
    return await valueEl.textContent() || '';
  }
}
