import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.fill('1');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);
  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  await page.waitForTimeout(500);
  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try { await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs'); } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

async function completeBreakFast(page: any) {
  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try { await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs'); } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Long Break Activity Timeline Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 300000 });

  test('should render long break entry in history timeline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);

    for (let i = 1; i <= 4; i++) {
      await completePomodoroFast(page, pomodoroPage, `Timeline Task ${i}`);
      if (i < 4) {
        await completeBreakFast(page);
      }
    }

    await page.waitForTimeout(1000);
    const longBreakOption = page.locator('.btn-option').filter({ hasText: /Long Break/i });
    if (await longBreakOption.isVisible({ timeout: 5000 }).catch(() => false)) {
      await longBreakOption.click();
      await page.waitForTimeout(500);

      await pomodoroPage.startTimer();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
      await page.waitForTimeout(500);
      await page.evaluate(async () => {
        const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
        for (let i = 0; i < 60; i++) {
          if ((window as any).timerFunctions?.dotNetRef) {
            try { await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs'); } catch { break; }
          }
          await delay(30);
        }
      });
      await page.waitForTimeout(3000);
      const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
      if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
        await consentOption.click();
        await page.waitForTimeout(1000);
      }
    }

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const longBreakBadge = page.locator('.tl-badge').filter({ hasText: /Long break/i });
    const hasLongBreak = await longBreakBadge.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasLongBreak).toBe(true);
  });
});
