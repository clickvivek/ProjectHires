import { test, expect } from '@playwright/test';

test('Verify Searched tab in Admin Setup', async ({ page }) => {
  test.setTimeout(60000);
  await page.setViewportSize({ width: 1440, height: 950 });

  // 1. Login via /#/login with Super Admin
  console.log('Navigating to login...');
  await page.goto('http://localhost:4200/#/login');
  await page.waitForTimeout(2000);

  await page.locator('input#email').fill('vivek1234@sharklasers.com');
  await page.locator('input#password').fill('Admin@123');
  await page.locator('button.btn-signin').click();
  await page.waitForTimeout(3000);

  // 2. Navigate to Admin Setup
  console.log('Navigating to Admin Setup...');
  await page.goto('http://localhost:4200/#/Adminsetup');
  await page.waitForTimeout(3000);

  // 3. Find and click "Searched" tab button
  console.log('Looking for Searched tab...');
  const searchedTab = page.locator('button:has-text("Searched")');
  await expect(searchedTab).toBeVisible({ timeout: 10000 });
  await searchedTab.click();
  await page.waitForTimeout(3500);

  // 4. Capture screenshot of the full dashboard and table
  await page.screenshot({
    path: 'C:/Users/vivek/.gemini/antigravity/brain/d735fcfd-67eb-4a14-9a26-08043e534df7/searched_tab_dashboard.png',
    fullPage: false
  });
  console.log('Successfully captured screenshot of Searched tab!');

  // 5. Scroll down to capture the table rows
  await page.evaluate(() => window.scrollBy(0, 450));
  await page.waitForTimeout(1000);
  await page.screenshot({
    path: 'C:/Users/vivek/.gemini/antigravity/brain/d735fcfd-67eb-4a14-9a26-08043e534df7/searched_tab_table.png',
    fullPage: false
  });
  console.log('Successfully captured screenshot of Searched table!');
});
