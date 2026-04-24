import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroAndBreak(page: any, pomodoroPage: PomodoroPage) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.pressSequentially('1');
  await pomodoroInput.dispatchEvent('input');
  await page.waitForTimeout(500);

  const shortBreakInput = page.locator('.step-input').nth(1);
  await shortBreakInput.click({ clickCount: 3 });
  await shortBreakInput.pressSequentially('1');
  await shortBreakInput.dispatchEvent('input');
  await page.waitForTimeout(500);

  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask('Break Time Test Task');
  await pomodoroPage.selectTask('Break Time Test Task');
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);

  const consentModal = page.locator('.consent-modal-overlay');
  const isModalVisible = await consentModal.isVisible().catch(() => false);
  if (isModalVisible) {
    await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
    await page.waitForTimeout(2000);
    await pomodoroPage.startTimer();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);

    const consentModal2 = page.locator('.consent-modal-overlay');
    const isModal2Visible = await consentModal2.isVisible().catch(() => false);
    if (isModal2Visible) {
      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await page.waitForTimeout(1000);
    }
  }
}

test.describe('History Break Time Stat', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should show non-zero break time after completing a pomodoro and break', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroAndBreak(page, pomodoroPage);

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const breakTimeLabel = page.locator('.sl').filter({ hasText: 'Break time' });
    await expect(breakTimeLabel).toBeVisible();

    const breakTimeValue = breakTimeLabel.locator('..').locator('.sv');
    const breakTimeText = await breakTimeValue.textContent();
    expect(breakTimeText).not.toBe('0m');
    expect(breakTimeText).not.toBe('0');
  });

  test('should display break time stat in daily summary stat grid', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.stat-grid').first()).toBeVisible();
    await expect(page.locator('.sl').filter({ hasText: 'Break time' })).toBeVisible();
  });
});
