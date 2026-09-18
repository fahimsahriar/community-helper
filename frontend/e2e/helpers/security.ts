import { expect, type Page } from '@playwright/test';

/**
 * Fails the test when the page shows signs of a CSP or XSS problem:
 * console errors mentioning Content-Security-Policy / refused loads, or
 * any uncaught page error during the journey.
 *
 * Angular auto-escapes interpolations (the XSS control); the CSP meta tag
 * in `src/index.html` is the second layer. This helper proves both hold
 * on every covered journey — attach it at the start of each spec.
 */
const VIOLATION_PATTERN =
  /content[-\s]?security[-\s]?policy|refused to (load|execute|apply|connect)|blocked by csp|unsafe-inline|unsafe-eval|xss/i;

export function watchSecurity(page: Page): { violations: string[] } {
  const violations: string[] = [];

  page.on('console', (message) => {
    if (message.type() === 'error' && VIOLATION_PATTERN.test(message.text())) {
      violations.push(`console: ${message.text()}`);
    }
  });
  page.on('pageerror', (error) => {
    violations.push(`pageerror: ${String(error)}`);
  });

  return { violations };
}

export function expectNoSecurityViolations(violations: string[]): void {
  expect(
    violations,
    violations.length > 0
      ? `Security violations detected:\n${violations.join('\n')}`
      : 'No CSP/XSS violations',
  ).toEqual([]);
}
