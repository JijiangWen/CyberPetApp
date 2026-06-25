import { defineConfig, devices } from '@playwright/test';

const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? 'http://127.0.0.1:5237';
const dbConnection =
  process.env.ConnectionStrings__DefaultConnection ??
  'Host=localhost;Port=5432;Database=cyberpet_test;Username=admin;Password=secret';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: process.env.PLAYWRIGHT_BASE_URL
    ? undefined
    : {
      command:
        process.platform === 'win32'
          ? 'powershell -ExecutionPolicy Bypass -File scripts/run-e2e-server.ps1'
          : 'bash scripts/run-e2e-server.sh',
      url: `${baseURL}/login`,
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
      env: {
        ConnectionStrings__DefaultConnection: dbConnection,
        ASPNETCORE_ENVIRONMENT: 'Development',
      },
    },
});
