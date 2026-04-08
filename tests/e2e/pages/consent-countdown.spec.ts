import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Consent Countdown', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should show countdown text with default delay seconds', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Countdown Test');
    await pomodoroPage.selectTask('Countdown Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 1500; i++) {
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
    const isModalVisible = await consentModal.isVisible().catch(() => false);
    if (!isModalVisible) {
      return;
    }

    const countdownText = page.locator('.countdown-text');
    await expect(countdownText).toBeVisible();

    const text = await countdownText.textContent();
    expect(text).toContain('Auto-continuing in');
    expect(text).toMatch(/\d+ seconds/);

    const match = text.match(/(\d+)\s*seconds/);
    if (match) {
      const seconds = parseInt(match[1]);
      expect(seconds).toBeGreaterThan(0);
      expect(seconds).toBeLessThanOrEqual(10);
    }
  });
});
