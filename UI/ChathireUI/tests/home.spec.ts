import { expect, test } from '@playwright/test';

const makeTestEmail = (suffix: string) => `playwright.${Date.now()}.${suffix}@example.com`;

const completeSignup = async (page: any, email: string, password: string) => {
  await page.goto('/#/signup');
  await page.locator('input[name="email"]').fill(email);
  await page.locator('input[name="password"]').fill(password);
  await page.locator('input#agreeTerms').evaluate((el: HTMLInputElement) => {
    el.checked = true;
    el.dispatchEvent(new Event('change', { bubbles: true }));
  });
  await page.locator('button:has-text("SIGN UP")').click();
  await expect(page).toHaveURL(/\/\#\/dashboard$/);
};

test.describe('Hires homepage', () => {
  test('loads the homepage and shows the main sections', async ({ page }) => {
    await page.goto('/#/home');

    await expect(page).toHaveTitle(/ChatHire/i);
    await expect(page.locator('#navbarSupportedContent').getByRole('link', { name: 'Home' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Post a Job — FREE' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Find Jobs' }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: 'Find Talent' }).first()).toBeVisible();
    await expect(page.getByText('Connect with recruiters, consultants, bench sales professionals and companies through one simple hiring marketplace.')).toBeVisible();
    await expect(page.getByRole('tab', { name: 'Find Jobs' })).toBeVisible();
    await expect(page.getByRole('tab', { name: 'Find Talent' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'How it works' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Ready to make your next hiring move?' })).toBeVisible();
  });

  test('shows key CTA and footer links on the home page', async ({ page }) => {
    await page.goto('/#/home');

    await expect(page.getByRole('link', { name: 'Sign Up' }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: 'Log In' }).first()).toBeVisible();
    await expect(page.getByText('About Us')).toBeVisible();
    await expect(page.getByText('Help Center')).toBeVisible();
    await expect(page.getByText('Privacy Policy')).toBeVisible();
  });

  test('validates login form before submission', async ({ page }) => {
    await page.goto('/#/login');

    await page.locator('button:has-text("LOG IN")').click();

    await expect(page.getByText('Email is required')).toBeVisible();
    await expect(page.getByText('Password is required.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Log In' })).toBeVisible();
  });

  test('validates signup form before submission', async ({ page }) => {
    await page.goto('/#/signup');

    await page.locator('button:has-text("SIGN UP")').click();

    await expect(page.getByText('Email is required')).toBeVisible();
    await expect(page.getByText('Password is required.')).toBeVisible();
    await expect(page.getByText('You must agree to the terms & conditions')).toBeVisible();
  });

  test('shows invalid login error for wrong credentials', async ({ page }) => {
    await page.goto('/#/login');

    await page.locator('input[name="email"]').fill('playwright.invalid@example.com');
    await page.locator('input[name="password"]').fill('WrongPass123!');
    await page.locator('button:has-text("LOG IN")').click();

    await expect(page.getByText('Invalid UserName/Password')).toBeVisible();
  });

  test('shows duplicate signup error for an existing email', async ({ page }) => {
    const email = makeTestEmail('duplicate-signup');
    const password = 'TestPass123!';

    await completeSignup(page, email, password);
    await page.getByText('Logout').click();
    await page.goto('/#/signup');
    await page.locator('input[name="email"]').fill(email);
    await page.locator('input[name="password"]').fill(password);
    await page.locator('input#agreeTerms').evaluate((el: HTMLInputElement) => {
      el.checked = true;
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await page.locator('button:has-text("SIGN UP")').click();

    await expect(page.getByText('User Already exists')).toBeVisible();
  });

  test('logout redirects an authenticated user back to login', async ({ page }) => {
    const email = makeTestEmail('logout-success');
    const password = 'TestPass123!';

    await completeSignup(page, email, password);
    await page.getByText('Logout').click();

    await expect(page).toHaveURL(/\/\#\/login$/);
    await expect(page.getByRole('heading', { name: 'Log In' })).toBeVisible();
  });

  test('search jobs form accepts a skill and location before submission', async ({ page }) => {
    await page.goto('/#/search-jobs');

    await page.locator('input[name="selectedSkill"]').fill('Java');
    await page.locator('input[name="searchJobLocation"]').fill('Bengaluru');
    await page.locator('input[name="searchJobLocation"]').press('Enter');

    await expect(page).toHaveURL(/skill=Java/i);
    await expect(page).toHaveURL(/location=Bengaluru/i);
    await expect(page.getByText(/Java - \d+ jobs found/i)).toBeVisible();
    await expect(page.getByText('Date Posted')).toBeVisible();
    await expect(page.getByText('Onsite/Remote')).toBeVisible();
  });

  test('signup success flow creates a user and redirects to dashboard', async ({ page }) => {
    const email = makeTestEmail('signup-success');
    const password = 'TestPass123!';

    await completeSignup(page, email, password);
    await expect(page.getByText('You are almost ready to start Hiring or post your jobs. Provide your information below')).toBeVisible();
  });

  test('login success flow authenticates an existing user', async ({ page }) => {
    const email = makeTestEmail('login-success');
    const password = 'TestPass123!';

    await completeSignup(page, email, password);
    await page.evaluate(() => localStorage.clear());
    await page.goto('/#/login');
    await page.locator('input[name="email"]').fill(email);
    await page.locator('input[name="password"]').fill(password);
    await page.locator('button:has-text("LOG IN")').click();

    await expect(page).toHaveURL(/\/\#\/dashboard$/);
    await expect(page.getByText('You are almost ready to start Hiring or post your jobs. Provide your information below')).toBeVisible();
  });
});
