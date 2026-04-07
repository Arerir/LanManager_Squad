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
