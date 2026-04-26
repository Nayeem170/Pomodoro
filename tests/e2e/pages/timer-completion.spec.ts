import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Completion', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

  test('should reset timer to full duration after pomodoro completes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.addTask('Completion Reset Test');
    await pomodoroPage.selectTask('Completion Reset Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/\d{1,2}:\d{2}/);
    expect(timerDisplay).not.toBe('00:00');
  });

  test('should show start button after pomodoro completes without auto-start', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.addTask('No Auto Test');
    await pomodoroPage.selectTask('No Auto Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    const consentModal = page.locator('.consent-modal-overlay');
    const isModalVisible = await consentModal.isVisible().catch(() => false);
    if (isModalVisible) {
      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await pomodoroPage.resetTimer();
    }

    await expect(page.locator('button[aria-label="Start timer"]')).toBeVisible({ timeout: 10000 });
  });

  test('should switch to break session after selecting option from consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');

    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isOn = await toggle.evaluate(el => el.classList.contains('on'));
    if (!isOn) {
      await toggle.click();
    }

    await pomodoroPage.setPomodoroMinutes(1);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Session Switch Test');
    await pomodoroPage.selectTask('Session Switch Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
    await page.waitForTimeout(2000);

    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('SHORT BREAK');
  });

  test('should record activity after pomodoro completes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.addTask('Activity Record Test');
    await pomodoroPage.selectTask('Activity Record Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tl-row').first()).toBeVisible({ timeout: 5000 });
  });
});
