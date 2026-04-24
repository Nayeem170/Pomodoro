import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.pressSequentially('1');
  await pomodoroInput.dispatchEvent('input');
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
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
}

async function setupAndEnableAutoStart(page: any, pomodoroPage: PomodoroPage) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
  const isOn = await toggle.evaluate(el => el.classList.contains('on'));
  if (!isOn) {
    await toggle.click();
    await page.waitForTimeout(500);
  }

  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.pressSequentially('1');
  await pomodoroInput.dispatchEvent('input');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);

  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
}

async function runTimerTicks(page: any) {
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
}

test.describe('Long Break Count', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 300000 });

  test('should offer Long Break as default after completing N pomodoros', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupAndEnableAutoStart(page, pomodoroPage);
    await pomodoroPage.addTask('Long Break Test');
    await pomodoroPage.selectTask('Long Break Test');

    for (let i = 1; i <= 3; i++) {
      await pomodoroPage.startTimer();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
      await page.waitForTimeout(500);
      await runTimerTicks(page);

      await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
      const defaultOption = page.locator('.btn-option.default');
      await expect(defaultOption).toContainText('Another Pomodoro');

      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await page.waitForTimeout(2000);
    }

    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await page.waitForTimeout(500);
    await runTimerTicks(page);

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    const defaultOption = page.locator('.btn-option.default');
    await expect(defaultOption).toBeVisible();
    await expect(defaultOption).toContainText('Long Break');

    await expect(page.locator('.btn-option').filter({ hasText: 'Long Break' })).toBeVisible();
  });
});
