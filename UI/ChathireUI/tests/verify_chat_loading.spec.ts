import { test, expect } from '@playwright/test';

test('Verify loading button state when clicking Message button', async ({ page }) => {
  test.setTimeout(60000);
  await page.setViewportSize({ width: 1400, height: 900 });

  // 1. Login as Super Admin (vivek1234@sharklasers.com / Admin@123)
  console.log('Navigating to login...');
  await page.goto('http://localhost:4200/#/login');
  await page.waitForTimeout(2000);

  await page.locator('input#email').fill('vivek1234@sharklasers.com');
  await page.locator('input#password').fill('Admin@123');
  await page.locator('button.btn-signin').click();
  await page.waitForTimeout(3500);

  // 2. Navigate to search-hotlist
  console.log('Navigating to search-hotlist...');
  await page.goto('http://localhost:4200/#/search-hotlist?skill=oracle');
  await page.waitForTimeout(3000);

  // 3. Find a Message Me button
  const messageMeBtn = page.locator('button.chat-btn:has-text("Message Me")').first();
  await expect(messageMeBtn).toBeVisible({ timeout: 10000 });

  // 4. Click the Message Me button
  console.log('Clicking Message Me button...');
  await messageMeBtn.click();

  // 5. Instantly capture screenshot of the loading button state
  await page.waitForTimeout(100);
  await page.screenshot({
    path: 'C:/Users/vivek/.gemini/antigravity/brain/d735fcfd-67eb-4a14-9a26-08043e534df7/message_btn_loading_state.png',
    fullPage: false
  });
  console.log('Screenshot saved to message_btn_loading_state.png');

  // 6. Wait for chat window to mount/open
  await page.waitForTimeout(4000);
  await page.screenshot({
    path: 'C:/Users/vivek/.gemini/antigravity/brain/d735fcfd-67eb-4a14-9a26-08043e534df7/chat_popup_opened.png',
    fullPage: false
  });
  console.log('Screenshot saved to chat_popup_opened.png');
});
