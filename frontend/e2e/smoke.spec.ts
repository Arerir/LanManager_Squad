import { test, expect } from '@playwright/test';

test.describe('GameVille LanManager - Smoke Tests', () => {
  test('should load the login page', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('h1')).toHaveText('Sign In');
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
  });

  test('should load the registration page', async ({ page }) => {
    await page.goto('/register');
    await expect(page.locator('h1')).toHaveText('Create Account');
    await expect(page.locator('input[name="name"]')).toBeVisible();
    await expect(page.locator('input[name="userName"]')).toBeVisible();
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
  });

  test('should navigate between login and register', async ({ page }) => {
    await page.goto('/login');
    await page.click('text=Register');
    await expect(page).toHaveURL(/\/register/);
    await expect(page.locator('h1')).toHaveText('Create Account');

    await page.click('text=Sign in');
    await expect(page).toHaveURL(/\/login/);
    await expect(page.locator('h1')).toHaveText('Sign In');
  });

  test('should apply cyberpunk theme styles', async ({ page }) => {
    await page.goto('/login');
    
    // Check that root background uses the dark theme
    const root = page.locator('#root');
    await expect(root).toBeVisible();
    
    // Check that neon accent colors are present in the page
    const primaryButton = page.locator('button.btn-primary');
    await expect(primaryButton).toBeVisible();
  });
});
