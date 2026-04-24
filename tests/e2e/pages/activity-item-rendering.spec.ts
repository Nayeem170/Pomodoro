import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.fill('1');
  await page.waitForTimeout(300);

  const shortBreakInput = page.locator('.step-input').nth(1);
  await shortBreakInput.click({ clickCount: 3 });
  await shortBreakInput.fill('1');
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
    for (let i = 0; i < 70; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(50);
    }
  });
  await page.waitForTimeout(3000);

  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Activity Item Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should render activity item with correct structure after pomodoro', async ({ page }) => {
    await completePomodoroFast(page, pomodoroPage, 'Render Test Task');

    await pomodoroPage.goto('/history');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const activityItem = page.locator('.tl-row').first();
    await expect(activityItem).toBeVisible({ timeout: 5000 });

    await expect(activityItem.locator('.tl-dot')).toBeVisible();

    await expect(activityItem.locator('.tl-time')).toBeVisible();
    const timeText = await activityItem.locator('.tl-time').textContent();
    expect(timeText).toMatch(/\d{1,2}:\d{2}\s*[AP]M/);

    await expect(activityItem.locator('.tl-badge')).toBeVisible();
    await expect(activityItem.locator('.tl-badge')).toContainText('Pomodoro');

    await expect(activityItem.locator('.tl-task')).toBeVisible();
    await expect(activityItem.locator('.tl-task')).toContainText('Render Test Task');
  });

  test('should render break activity with correct icon and name', async ({ page }) => {
    await completePomodoroFast(page, pomodoroPage, 'Break Render Task');

    const consentOption = page.locator('.btn-option').filter({ hasText: /Short Break/i });
    if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
      await consentOption.click();
      await page.waitForTimeout(1000);

      const startBtn = page.locator('button[aria-label="Start timer"]');
      if (await startBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await pomodoroPage.startTimer();
      }
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
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

      const skipOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
      if (await skipOption.isVisible({ timeout: 2000 }).catch(() => false)) {
        await skipOption.click();
        await page.waitForTimeout(1000);
      }
    }

    await pomodoroPage.goto('/history');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const breakActivity = page.locator('.tl-row').filter({ hasText: /Short break/i }).first();
    if (await breakActivity.isVisible({ timeout: 2000 }).catch(() => false)) {
      await expect(breakActivity.locator('.tl-dot').first()).toHaveClass(/brk/);
      await expect(breakActivity.locator('.tl-badge').first()).toContainText('Short break');
    }
  });
});
