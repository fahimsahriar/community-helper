import { expect, test } from '@playwright/test';

import { expectNoSecurityViolations, watchSecurity } from './helpers/security';

const WEB = process.env['WEB_URL'] ?? 'http://localhost:4200';

function uniqueEmail(prefix: string): string {
  return `${prefix}-${Date.now()}-${Math.floor(Math.random() * 10000)}@example.com`;
}

async function register(page: import('@playwright/test').Page, email: string, role: 'volunteer' | 'org_admin'): Promise<void> {
  await page.goto(`${WEB}/register`);
  await page.getByTestId('register-email').fill(email);
  await page.getByTestId('register-password').fill('password123');
  if (role === 'org_admin') {
    await page.getByTestId('register-role').click();
    await page.getByTestId('register-role-org_admin').click();
  }
  await page.getByTestId('register-submit').click();
  await expect(page).not.toHaveURL(/\/register$/);
}

test('volunteer register → profile setup → edit persists', async ({ page }) => {
  const { violations } = watchSecurity(page);
  await register(page, uniqueEmail('volunteer'), 'volunteer');

  await page.goto(`${WEB}/profile`);
  await expect(page.getByTestId('profile-title')).toBeVisible();
  await expect(page.getByTestId('profile-hours-placeholder')).toBeVisible();
  await expect(page.getByTestId('profile-setup-heading')).toBeVisible();

  await page.getByTestId('volunteer-skills').fill('teaching, first-aid');
  await page.getByTestId('volunteer-availability').fill('Weekends');
  await page.getByTestId('volunteer-causes').fill('education');
  await page.getByTestId('volunteer-location').fill('Dhaka');
  await page.getByTestId('volunteer-bio').fill('Happy to help.');
  await page.getByTestId('volunteer-save').click();

  await expect(page.getByTestId('profile-saved')).toContainText('Profile saved.');
  await expect(page.getByTestId('profile-edit-heading')).toBeVisible();

  await page.reload();
  await expect(page.getByTestId('profile-edit-heading')).toBeVisible();
  await expect(page.getByTestId('volunteer-location')).toHaveValue('Dhaka');

  expectNoSecurityViolations(violations);
});

test('org admin register → dashboard shows pending badge after registration', async ({
  page,
}) => {
  const { violations } = watchSecurity(page);
  await register(page, uniqueEmail('orgadmin'), 'org_admin');

  await page.goto(`${WEB}/org/dashboard`);
  await expect(page.getByTestId('org-dashboard-title')).toBeVisible();
  await expect(page.getByTestId('org-setup-heading')).toBeVisible();

  await page.getByTestId('org-name').fill('River Cleaners');
  await page.getByTestId('org-type').fill('Nonprofit');
  await page.getByTestId('org-cause-tags').fill('environment');
  await page.getByTestId('org-location').fill('Dhaka');
  await page.getByTestId('org-description').fill('Cleaning rivers every weekend.');
  await page.getByTestId('org-save').click();

  await expect(page.getByTestId('org-saved')).toContainText('Organization saved.');
  await expect(page.getByTestId('org-pending-badge')).toContainText('Pending verification');

  expectNoSecurityViolations(violations);
});
