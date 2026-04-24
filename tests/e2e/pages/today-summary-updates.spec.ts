import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.fill('1');
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

test.describe('Today Summary Updates', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should show non-zero focused time after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Focus Task');

    const focusTime = page.locator('.pomo-focus-time');
    await expect(focusTime).toBeVisible();
    const text = await focusTime.textContent();
    expect(text).toBeTruthy();
    const numberMatch = text?.match(/\d+/);
    expect(numberMatch).not.toBeNull();
    expect(parseInt(numberMatch![0])).toBeGreaterThan(0);
  });

  test('should show non-zero pomodoro count after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Pomodoro Task');

    const pomodoroNum = page.locator('.pomo-num');
    await expect(pomodoroNum).toBeVisible();
    const text = await pomodoroNum.textContent();
    expect(text).toBeTruthy();
    const number = parseInt(text!);
    expect(number).toBeGreaterThan(0);
  });

  test('should show updated daily goal text after completing a pomodoro', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Goal Task');

    const pomoSub = page.locator('.pomo-sub');
    await expect(pomoSub).toBeVisible();
    await expect(pomoSub).toContainText('completed today');
  });

  test('should persist summary stats after page reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await completePomodoroFast(page, pomodoroPage, 'Summary Persist Task');

    const pomodoroNum = page.locator('.pomo-num');
    await expect(pomodoroNum).toBeVisible();
    const textBefore = await pomodoroNum.textContent();
    const numberBefore = parseInt(textBefore!);
    expect(numberBefore).toBeGreaterThan(0);

    await page.reload();
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const pomodoroNumAfter = page.locator('.pomo-num');
    await expect(pomodoroNumAfter).toBeVisible({ timeout: 10000 });
    const textAfter = await pomodoroNumAfter.textContent();
    const numberAfter = parseInt(textAfter!);
    expect(numberAfter).toBe(numberBefore);
    expect(numberAfter).toBeGreaterThan(0);
  });
});
