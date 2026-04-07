import { test, expect } from '@playwright/test';

const loginUser = {
  token: 'fake-token',
  userId: '00000000-0000-0000-0000-000000000001',
  name: 'Test User',
  email: 'test@example.com',
  roles: ['Attendee'],
};

const profileUser = {
  id: '00000000-0000-0000-0000-000000000001',
  name: 'Test User',
  userName: 'testuser',
  email: 'test@example.com',
};

test.describe('Profile Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.evaluate(({ loginUser, profileUser }) => {
      localStorage.setItem('jwt_token', loginUser.token);
      localStorage.setItem('jwt_user', JSON.stringify(loginUser));
      localStorage.setItem('currentUser', JSON.stringify(profileUser));
    }, { loginUser, profileUser });
  });

  test('profile page renders with user info', async ({ page }) => {
    await page.goto('/profile');

    await expect(page.locator('h1')).toHaveText('Profile');
    await expect(page.getByText('test@example.com')).toBeVisible();
    await expect(page.getByText('@testuser')).toBeVisible();
  });

  test('profile page is accessible from sidebar nav', async ({ page }) => {
    await page.goto('/');

    const profileLink = page.getByRole('link', { name: /test user/i });
    await expect(profileLink).toBeVisible();
    await profileLink.click();
    await expect(page).toHaveURL(/\/profile/);
  });

  test('sign out clears auth and redirects to login', async ({ page }) => {
    await page.goto('/profile');

    const signOutButton = page.locator('main').getByRole('button', { name: 'Sign Out' });
    await expect(signOutButton).toBeVisible();
    await signOutButton.click();
    await expect(page).toHaveURL(/\/login/);

    const currentUser = await page.evaluate(() => localStorage.getItem('currentUser'));
    expect(currentUser).toBeNull();
  });
});
