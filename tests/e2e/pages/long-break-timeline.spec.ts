import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Long Break Activity Timeline Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 600000 });

  test('should render long break entry in history timeline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.setSettingViaIndexedDB('longBreakInterval', 2);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.setPomodoroMinutes(1);

    const longBreakInput = page.locator('.step-input').nth(2);
    const currentLongBreak = parseInt(await longBreakInput.inputValue());
    if (currentLongBreak !== 1) {
      const diff = 1 - currentLongBreak;
      const btnLabel = diff > 0 ? 'Increase' : 'Decrease';
      const btn = page.locator('.stepper').nth(2).locator(`.step-btn[aria-label="${btnLabel}"]`);
      for (let i = 0; i < Math.abs(diff); i++) {
        await btn.click();
        await page.waitForTimeout(50);
      }
    }

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Timeline Task 1');
    await pomodoroPage.selectTask('Timeline Task 1');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    const consentModal1 = page.locator('.consent-modal-overlay');
    if (await consentModal1.isVisible({ timeout: 5000 }).catch(() => false)) {
      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await page.waitForTimeout(1000);
    }

    const isAlreadyRunning = await page.locator('button[aria-label="Pause timer"]').isVisible({ timeout: 3000 }).catch(() => false);
    if (!isAlreadyRunning) {
      await pomodoroPage.startTimer();
    }
    await pomodoroPage.completePomodoroFast();

    const consentModal2 = page.locator('.consent-modal-overlay');
    if (await consentModal2.isVisible({ timeout: 5000 }).catch(() => false)) {
      const longBreakOption = page.locator('.btn-option').filter({ hasText: /Long Break/i });
      if (await longBreakOption.isVisible({ timeout: 2000 }).catch(() => false)) {
        await longBreakOption.click();
        await page.waitForTimeout(500);

        const isBreakRunning = await page.locator('button[aria-label="Pause timer"]').isVisible({ timeout: 3000 }).catch(() => false);
        if (!isBreakRunning) {
          const startOrResume = page.locator('button[aria-label="Start timer"], button[aria-label="Resume timer"]').first();
          if (await startOrResume.isVisible({ timeout: 3000 }).catch(() => false)) {
            await startOrResume.click();
          }
        }
        await pomodoroPage.completePomodoroFast();

        const consentModal3 = page.locator('.consent-modal-overlay');
        if (await consentModal3.isVisible({ timeout: 5000 }).catch(() => false)) {
          await pomodoroPage.skipConsentModal();
        }
      }
    }

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const longBreakBadge = page.locator('.tl-badge').filter({ hasText: /Long break/i });
    await expect(longBreakBadge.first()).toBeVisible({ timeout: 5000 });
  });
});
