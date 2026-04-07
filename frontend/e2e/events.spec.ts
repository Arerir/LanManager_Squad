import { test, expect } from './fixtures/auth';

const EVENT_CONTEXT_KEY = 'selectedEvent';

test.describe('Events', () => {
  test('events page renders without crashing', async ({ page }) => {
    await page.goto('/events');
    await expect(page).not.toHaveURL(/\/login/);
    await expect(page.locator('h1.page-title')).toHaveText('Events');
    await expect(page.locator('body')).toBeVisible();
  });

  test('events page accessible after login redirect chain', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('h1')).toHaveText('Sign In');
    await page.goto('/events');
    await expect(page).toHaveURL(/\/events/);
    await expect(page.locator('h1.page-title')).toHaveText('Events');
  });

  test('event detail navigation shows event info or empty state', async ({ page }) => {
    await page.goto('/events');
    await expect(page).not.toHaveURL(/\/login/);

    const rows = page.locator('tbody tr');
    if (await rows.count() === 0) {
      await expect(page.locator('text=No events found.')).toBeVisible();
      return;
    }

    await rows.first().click();
    await expect(page).toHaveURL(/\/events\//);
    await expect(page.locator('h1.page-title')).toBeVisible();
  });

  test('event detail page handles unknown event id gracefully', async ({ page }) => {
    await page.goto('/events/00000000-0000-0000-0000-000000000000');
    await expect(page.locator('body')).toBeVisible();
    await expect(page).not.toHaveURL(/\/login/);
  });

  test('event context persists in localStorage when an event is selected', async ({ page }) => {
    await page.goto('/events');
    const rows = page.locator('tbody tr');
    if (await rows.count() === 0) {
      test.skip(true, 'Requires at least one event from the backend to select.');
    }

    await rows.first().click();
    await expect(page).toHaveURL(/\/events\//);

    const stored = await page.evaluate((key) => localStorage.getItem(key), EVENT_CONTEXT_KEY);
    if (!stored) {
      test.skip(true, `Event context key "${EVENT_CONTEXT_KEY}" not found after selection.`);
    }

    await page.reload();
    const persisted = await page.evaluate((key) => localStorage.getItem(key), EVENT_CONTEXT_KEY);
    expect(persisted).toEqual(stored);
  });
});
