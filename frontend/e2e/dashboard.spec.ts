import { test, expect } from '@playwright/test';

const testUser = {
  id: '00000000-0000-0000-0000-000000000001',
  username: 'testuser',
  name: 'Test User',
  email: 'test@example.com',
  roles: ['Attendee'],
};

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate((user) => {
      localStorage.setItem('jwt_token', 'fake-token');
      localStorage.setItem('jwt_user', JSON.stringify(user));
    }, testUser);
  });

  test('dashboard page renders after login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).not.toHaveURL(/\/login/);
    await expect(page.locator('h1.page-title')).toHaveText('Dashboard');
  });

  test('dashboard shows welcome guidance', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page.getByText('Welcome to LanManager. Select a section from the sidebar.')).toBeVisible();
  });

  test('sidebar nav links are present with correct routes', async ({ page }) => {
    await page.goto('/dashboard');
    const nav = page.locator('nav');

    await expect(nav.getByRole('link', { name: 'Dashboard' })).toHaveAttribute('href', '/');
    await expect(nav.getByRole('link', { name: 'Events' })).toHaveAttribute('href', '/events');
    await expect(nav.getByRole('link', { name: 'Users' })).toHaveAttribute('href', '/users');
    await expect(nav.getByRole('link', { name: 'Attendance' })).toHaveAttribute('href', '/attendance');
    await expect(nav.getByRole('link', { name: 'Tournaments' })).toHaveAttribute('href', '/tournaments');
    await expect(nav.getByRole('link', { name: 'Seating' })).toHaveAttribute('href', '/seating');
    await expect(nav.getByRole('link', { name: 'Equipment' })).toHaveAttribute('href', '/equipment');
    await expect(nav.getByRole('link', { name: testUser.name })).toHaveAttribute('href', '/profile');
  });
});
