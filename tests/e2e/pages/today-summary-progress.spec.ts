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
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
}

test.describe('Today Summary Progress Bar', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should show progress bar fill with 0% width when no pomodoros completed', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.progress-bar')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.progress-bar-fill')).toBeAttached();

    const widthStyle = await page.locator('.progress-bar-fill').getAttribute('style');
    expect(widthStyle).toContain('width:');
    expect(widthStyle).toContain('0%');
  });

  test('should increase progress bar fill width after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const fillBefore = page.locator('.progress-bar-fill');
    const styleBefore = await fillBefore.getAttribute('style');
    const widthBeforeMatch = styleBefore?.match(/width:\s*([\d.]+)%/);
    const widthBefore = widthBeforeMatch ? parseFloat(widthBeforeMatch[1]) : 0;

    await completePomodoroFast(page, pomodoroPage, 'Progress Task');

    const fillAfter = page.locator('.progress-bar-fill');
    await expect(fillAfter).toBeAttached();
    const styleAfter = await fillAfter.getAttribute('style');
    const widthAfterMatch = styleAfter?.match(/width:\s*([\d.]+)%/);
    const widthAfter = widthAfterMatch ? parseFloat(widthAfterMatch[1]) : 0;

    expect(widthAfter).toBeGreaterThan(widthBefore);
  });

  test('should calculate progress bar fill width as percentage of daily goal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const dailyGoalInput = page.locator('.step-input').nth(3);
    await dailyGoalInput.click({ clickCount: 3 });
    await dailyGoalInput.fill('4');
    await dailyGoalInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await completePomodoroFast(page, pomodoroPage, 'Progress Calc Task');

    const fill = page.locator('.progress-bar-fill');
    await expect(fill).toBeAttached();
    const style = await fill.getAttribute('style');
    const widthMatch = style?.match(/width:\s*([\d.]+)%/);
    const widthPercent = widthMatch ? parseFloat(widthMatch[1]) : 0;

    expect(widthPercent).toBeCloseTo(25, 0);
  });
});
