import { test, expect } from '@playwright/test';

const eventId = '00000000-0000-0000-0000-000000000001';

test.describe('Attendance Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.setItem('jwt_token', 'fake-token');
      localStorage.setItem('jwt_user', JSON.stringify({
        id: '00000000-0000-0000-0000-000000000001',
        username: 'testuser',
        email: 'test@example.com',
        roles: ['Attendee'],
      }));
    });
  });

  test('attendance page renders without crashing when no eventId provided', async ({ page }) => {
    await page.goto('/attendance');
    await expect(page.getByText('Select an event to view attendance.')).toBeVisible();
    await expect(page).not.toHaveURL(/\/login/);
  });

  test('attendance page renders with eventId param', async ({ page }) => {
    await page.route('**/api/events/**/attendance', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: '[]' }),
    );
    await page.goto(`/attendance?eventId=${eventId}`);
    await expect(page.getByRole('heading', { name: 'Live Attendance' })).toBeVisible();
    await expect(page.getByText('0 people checked in.')).toBeVisible();
  });

  test('attendance page shows empty outside state when no users outside', async ({ page }) => {
    await page.route('**/api/events/**/outside', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: '[]' }),
    );
    await page.goto(`/attendance?eventId=${eventId}&tab=outside`);
    await expect(page.getByRole('heading', { name: 'Outside Now' })).toBeVisible();
    await expect(page.getByText('Everyone is inside.')).toBeVisible();
  });
});
