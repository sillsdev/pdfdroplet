import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/automation",
  timeout: 60_000,
  expect: {
    timeout: 5_000,
  },
  reporter: [["list"]],
  workers: 1, // Disable parallel execution to avoid .NET app concurrency issues
  use: {
    actionTimeout: 0,
    trace: "retain-on-failure",
  },
});
