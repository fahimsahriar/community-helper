import { expect, test } from '@playwright/test';

import { expectNoSecurityViolations, watchSecurity } from './helpers/security';

const WEB = process.env['WEB_URL'] ?? 'http://localhost:4200';

function uniqueEmail(prefix: string): string {
  return `${prefix}-${Date.now()}-${Math.floor(Math.random() * 10000)}@example.com`;
}

/**
 * Ticket 05 core journey: register → login → view profile.
 * Logout is simulated by clearing `localStorage` (the app's token store);
 * the route guard must then bounce deep links back to `/login`.
 */
test('volunteer register → login → view profile', async ({ page }) => {
  const { violations } = watchSecurity(page);
  const email = uniqueEmail('loginflow');

  await page.goto(`${WEB}/register`);
  await page.getByTestId('register-email').fill(email);
  await page.getByTestId('register-password').fill('password123');
  await page.getByTestId('register-submit').click();
  await expect(page).not.toHaveURL(/\/register$/);

  await page.evaluate(() => localStorage.clear());
  await page.goto(`${WEB}/profile`);
  await expect(page).toHaveURL(/\/login(\?|$)/);

  await page.getByTestId('login-email').fill(email);
  await page.getByTestId('login-password').fill('password123');
  await page.getByTestId('login-submit').click();

  await expect(page).toHaveURL(/\/profile$/);
  await expect(page.getByTestId('profile-title')).toBeVisible();

  expectNoSecurityViolations(violations);
});
