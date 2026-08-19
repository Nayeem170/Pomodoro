import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Input Styling', () => {
  test('should serve app.css with the tep number input spinner reset', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');

    const css = await page.evaluate(async () => {
      const res = await fetch('/css/app.css');
      return await res.text();
    });

    expect(css).toContain('.tep-input-sm::-webkit-inner-spin-button');
    expect(css).toContain('-webkit-appearance: none');
    expect(css).toContain('appearance: textfield');
  });
});
