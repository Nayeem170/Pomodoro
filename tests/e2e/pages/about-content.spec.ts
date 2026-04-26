import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('About Page Content', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/about');
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });
  });

  test.describe.configure({ timeout: 60000 });

  test('should expand "What is Pomodoro?" section and show content', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'What is Pomodoro' });
    await toggle.click();
    await page.waitForTimeout(300);

    const collapseBody = page.locator('.collapse-body').first();
    await expect(collapseBody).toBeVisible();
    await expect(collapseBody).toContainText('25');
  });

  test('should expand "How It Works" section and show 5 step cards with numbers', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'How It Works' });
    await toggle.click();
    await page.waitForTimeout(300);

    await expect(page.locator('.step-card')).toHaveCount(5);

    const stepNumbers = page.locator('.step-number');
    await expect(stepNumbers.nth(0)).toContainText('1');
    await expect(stepNumbers.nth(1)).toContainText('2');
    await expect(stepNumbers.nth(2)).toContainText('3');
    await expect(stepNumbers.nth(3)).toContainText('4');
    await expect(stepNumbers.nth(4)).toContainText('5');
  });

  test('should expand "Benefits" section and show 6 benefit cards', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Benefits' });
    await toggle.click();
    await page.waitForTimeout(300);

    await expect(page.locator('.benefit-card')).toHaveCount(6);
  });

  test('should expand "Tips" section and show tips list items', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Tips' });
    await toggle.click();
    await page.waitForTimeout(300);

    const tipsList = page.locator('.tips-list');
    await expect(tipsList).toBeVisible();
    const tipItems = tipsList.locator('li');
    await expect(tipItems).toHaveCount(6);
  });

  test('should expand "Default Timer Settings" section and show 3 time cards', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Default Timer' });
    await toggle.click();
    await page.waitForTimeout(300);

    await expect(page.locator('.time-card')).toHaveCount(3);
    await expect(page.locator('.time-card.pomodoro .time-value')).toContainText('25');
    await expect(page.locator('.time-card.short-break .time-value')).toContainText('5');
    await expect(page.locator('.time-card.long-break .time-value')).toContainText('15');
  });

  test('should collapse section when toggle is clicked again', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'What is Pomodoro' });
    await toggle.click();
    await page.waitForTimeout(300);

    const collapseBody = page.locator('.collapse-body').first();
    await expect(collapseBody).toBeVisible();

    await toggle.click();
    await page.waitForTimeout(300);

    await expect(collapseBody).not.toBeVisible();
  });

  test('should collapse "How It Works" section when clicked again', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'How It Works' });
    await toggle.click();
    await page.waitForTimeout(300);
    await expect(page.locator('.step-card')).toHaveCount(5);

    await toggle.click();
    await page.waitForTimeout(300);
    await expect(page.locator('.step-card')).toHaveCount(0);
  });

  test('should show arrow rotation when section is toggled', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Benefits' });
    const arrow = toggle.locator('.collapse-arrow');

    const classBefore = await arrow.getAttribute('class');
    expect(classBefore).not.toContain('open');

    await toggle.click();
    await page.waitForTimeout(300);

    const classAfter = await arrow.getAttribute('class');
    expect(classAfter).toContain('open');

    await toggle.click();
    await page.waitForTimeout(300);

    const classFinal = await arrow.getAttribute('class');
    expect(classFinal).not.toContain('open');
  });
});
