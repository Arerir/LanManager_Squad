import { test as base, expect } from '@playwright/test';

const fakeUser = {
  id: '00000000-0000-0000-0000-000000000001',
  username: 'testuser',
  email: 'test@example.com',
  roles: ['Attendee'],
};

export const test = base.extend({
  page: async ({ page }, runFixture) => {
    await page.goto('/login');
    await page.evaluate((user) => {
      localStorage.setItem('jwt_token', 'fake-token-for-navigation-test');
      localStorage.setItem('jwt_user', JSON.stringify(user));
    }, fakeUser);
    await runFixture(page);
  },
});

export { expect };
