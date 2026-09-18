import { defineConfig } from '@playwright/test';

/**
 * Phase 1 auth e2e. The dev web server is started automatically unless one
 * is already listening. The backend must be running separately WITH the
 * `http` launch profile (it enables the Development CORS origin for
 * http://localhost:4200 — without it the browser cannot reach the API):
 *
 *   cd ../backend && dotnet run --project src/CommunityHelper.API --launch-profile http
 *   npx playwright test
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: process.env['CI'] ? 2 : 0,
  use: {
    baseURL: process.env['WEB_URL'] ?? 'http://localhost:4200',
    trace: 'on-first-retry',
  },
  webServer: {
    command: 'npm start -- --port 4200',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env['CI'],
    timeout: 180_000,
  },
});
