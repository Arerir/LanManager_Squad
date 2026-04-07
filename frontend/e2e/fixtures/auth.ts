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
import { test as base, Page } from '@playwright/test';

export type AuthFixtures = {
  authenticatedPage: Page;
};

export const test = base.extend<AuthFixtures>({
  authenticatedPage: async ({ page }, use) => {
    await page.goto('/login');
    await page.fill('input[name="email"]', process.env.TEST_USERNAME ?? 'testuser@example.com');
    await page.fill('input[name="password"]', process.env.TEST_PASSWORD ?? 'Test1234!');
    await page.click('button[type="submit"]');
    await page.waitForURL('/events');
    await use(page);
  },
});

export { expect } from '@playwright/test';
