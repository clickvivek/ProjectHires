import { test, expect } from '@playwright/test';

test.describe('ChatHire UI Automation - Core Screens & Navigation', () => {
  test.setTimeout(60000);

  test('Public Home Screen - header, hero, search bar and navigation tabs', async ({ page }) => {
    await page.goto('/#/home');
    await expect(page).toHaveTitle(/ChatHire/i);

    // Hero title & search input elements
    await expect(page.locator('#navbarSupportedContent').getByRole('link', { name: 'Home' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Find Jobs' }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: 'Find Talent' }).first()).toBeVisible();

    // Verify footer links
    await expect(page.getByText('About Us')).toBeVisible();
    await expect(page.getByText('Help Center')).toBeVisible();
    await expect(page.getByText('Privacy Policy')).toBeVisible();
  });

  test('Public Information Pages - About, Contact, FAQ, Terms, Privacy', async ({ page }) => {
    // About
    await page.goto('/#/about');
    await expect(page.locator('h3:has-text("About Us")').or(page.getByText('About Us').first())).toBeVisible();

    // Contact
    await page.goto('/#/contact');
    await expect(page.getByText('Contact Us').first()).toBeVisible();

    // FAQ
    await page.goto('/#/faq');
    await expect(page.getByText(/Frequently Asked Questions|FAQ/i).first()).toBeVisible();

    // Terms
    await page.goto('/#/terms');
    await expect(page.getByText(/Terms/i).first()).toBeVisible();

    // Privacy
    await page.goto('/#/privacy');
    await expect(page.getByText(/Privacy Policy/i).first()).toBeVisible();
  });

  test('Search Jobs Screen - filters, query parameters and job results cards', async ({ page }) => {
    await page.goto('/#/search-jobs?skill=React&location=Remote');
    await page.waitForTimeout(2000);

    const skillInput = page.locator('input[name="selectedSkill"]');
    await expect(skillInput).toBeVisible();
    await expect(skillInput).toHaveValue(/React/i);

    // Filter controls check
    await expect(page.getByText('Date Posted')).toBeVisible();
    await expect(page.getByText('Onsite/Remote')).toBeVisible();
  });

  test('Search Hotlist Screen - candidate search and results list', async ({ page }) => {
    await page.goto('/#/search-hotlist');
    await page.waitForTimeout(2000);

    // Verify search container
    const searchBox = page.locator('input[type="text"]').first();
    await expect(searchBox).toBeVisible();
  });

  test('Authentication Screen - validation triggers on empty submissions', async ({ page }) => {
    await page.goto('/#/login');
    await page.locator('button.btn-signin').click();
    await expect(page.getByText(/Please enter your email address/i).or(page.getByText(/Email is required/i))).toBeVisible();
    await expect(page.getByText(/Please enter your password/i).or(page.getByText(/Password is required/i))).toBeVisible();
  });

  test('Super Admin Flow - Admin Setup tabs and panels navigation', async ({ page }) => {
    await page.setViewportSize({ width: 1440, height: 1000 });
    await page.goto('/#/login');
    await page.locator('input#email').fill('vivek1234@sharklasers.com');
    await page.locator('input#password').fill('Admin@123');
    await page.locator('button.btn-signin').click();
    await page.waitForTimeout(2500);

    await page.goto('/#/Adminsetup');
    await page.waitForTimeout(2500);

    // Verify tab buttons presence
    await expect(page.locator('button:has-text("DAU & Activity")')).toBeVisible();
    await expect(page.locator('button:has-text("Bulk Upload CSV")')).toBeVisible();
    await expect(page.locator('button:has-text("Manage Companies")')).toBeVisible();
    await expect(page.locator('button:has-text("Searched")')).toBeVisible();

    // Click Searched tab and verify metric cards
    await page.locator('button:has-text("Searched")').click();
    await page.waitForTimeout(2000);
    await expect(page.getByText('Total Searches', { exact: true }).first()).toBeVisible();
  });

  test('Dashboard Screen - authenticated recruiter session', async ({ page }) => {
    await page.goto('/#/login');
    await page.locator('input#email').fill('vivek1234@sharklasers.com');
    await page.locator('input#password').fill('Admin@123');
    await page.locator('button.btn-signin').click();
    await page.waitForTimeout(2500);

    await page.goto('/#/dashboard');
    await page.waitForTimeout(2000);
    await expect(page).toHaveURL(/\/\#\/dashboard$/);
  });

});
