import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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

test.describe('Consent Auto-Continue', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should auto-start default session when consent countdown reaches zero', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Auto Continue Test');
    await pomodoroPage.selectTask('Auto Continue Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
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
      const countdownText = page.locator('.auto-continue-row span');
      const text = await countdownText.textContent();
      const match = text?.match(/(\d+)\s*seconds/);

      if (match) {
        const seconds = parseInt(match[1]);
        await page.waitForTimeout((seconds + 3) * 1000);
      } else {
        await page.waitForTimeout(15000);
      }
    }

    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 20000 });
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('POMODORO');
  });
});
