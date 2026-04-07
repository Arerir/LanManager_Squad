import { Page } from '@playwright/test';

export async function navigateTo(page: Page, path: string, eventId?: string) {
  const url = eventId ? `${path}?eventId=${eventId}` : path;
  await page.goto(url);
}

export async function getNavLink(page: Page, label: string) {
  return page.getByRole('link', { name: label });
}
