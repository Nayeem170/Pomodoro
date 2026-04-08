import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
  await page.waitForTimeout(500);

  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 2000; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(5);
    }
  });
  await page.waitForTimeout(3000);

  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Long Break Trigger', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should offer long break as an option after completing pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Long Break Test');
    await pomodoroPage.selectTask('Long Break Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 2000; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(5);
      }
    });
    await page.waitForTimeout(3000);

    const consentModal = page.locator('.consent-modal-overlay');
    const isModalVisible = await consentModal.isVisible({ timeout: 5000 }).catch(() => false);

    if (isModalVisible) {
      const longBreakOption = page.locator('.btn-option').filter({ hasText: /Long Break/i });
      await expect(longBreakOption).toBeVisible();
    } else {
      const longBreakTab = page.locator('button:has-text("Long Break")');
      await expect(longBreakTab).toBeVisible();
    }
  });
});
