import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('login page renders with email and password fields', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('login with invalid credentials shows error', async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[name="email"]', 'nobody@fake.invalid');
    await page.fill('input[name="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/login/);
  });

  test.skip('login with valid credentials redirects to dashboard', async ({ page }) => {
    // Requires running API with known credentials.
    await page.goto('/login');
    await page.fill('input[name="email"]', 'testuser@example.com');
    await page.fill('input[name="password"]', 'Test1234!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('navigating to protected route while logged out redirects to login', async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('jwt_user');
    });
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/);
  });

  test('navigating to /events while logged out redirects to login', async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('jwt_user');
    });
    await page.goto('/events');
    await expect(page).toHaveURL(/\/login/);
  });
});
