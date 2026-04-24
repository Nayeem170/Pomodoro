import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('About Page Defaults Match Settings', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display default pomodoro time matching settings default of 25 minutes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const pomodoroDefault = await inputs.nth(0).inputValue();

    await pomodoroPage.goto('/about');
    await expect(page.locator('.about-body')).toBeVisible();

    const timeCardValue = await page.locator('.time-card.pomodoro .time-value').textContent();
    expect(timeCardValue).toContain(pomodoroDefault);
  });

  test('should display default short break time matching settings default of 5 minutes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const shortBreakDefault = await inputs.nth(1).inputValue();

    await pomodoroPage.goto('/about');
    await expect(page.locator('.about-body')).toBeVisible();

    const timeCardValue = await page.locator('.time-card.short-break .time-value').textContent();
    expect(timeCardValue).toContain(shortBreakDefault);
  });

  test('should display default long break time matching settings default of 15 minutes', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const longBreakDefault = await inputs.nth(2).inputValue();

    await pomodoroPage.goto('/about');
    await expect(page.locator('.about-body')).toBeVisible();

    const timeCardValue = await page.locator('.time-card.long-break .time-value').textContent();
    expect(timeCardValue).toContain(longBreakDefault);
  });
});
