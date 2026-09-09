# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: home.spec.ts >> Hires homepage >> validates login form before submission
- Location: tests\home.spec.ts:43:7

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:4200/#/login
Call log:
  - navigating to "http://localhost:4200/#/login", waiting until "load"

```

# Test source

```ts
  1   | import { expect, test } from '@playwright/test';
  2   | 
  3   | const makeTestEmail = (suffix: string) => `playwright.${Date.now()}.${suffix}@example.com`;
  4   | 
  5   | const completeSignup = async (page: any, email: string, password: string) => {
  6   |   await page.goto('/#/signup');
  7   |   await page.locator('input[name="email"]').fill(email);
  8   |   await page.locator('input[name="password"]').fill(password);
  9   |   await page.locator('input#agreeTerms').evaluate((el: HTMLInputElement) => {
  10  |     el.checked = true;
  11  |     el.dispatchEvent(new Event('change', { bubbles: true }));
  12  |   });
  13  |   await page.locator('button:has-text("SIGN UP")').click();
  14  |   await expect(page).toHaveURL(/\/\#\/dashboard$/);
  15  | };
  16  | 
  17  | test.describe('Hires homepage', () => {
  18  |   test('loads the homepage and shows the main sections', async ({ page }) => {
  19  |     await page.goto('/#/home');
  20  | 
  21  |     await expect(page).toHaveTitle(/ChatHire/i);
  22  |     await expect(page.locator('#navbarSupportedContent').getByRole('link', { name: 'Home' })).toBeVisible();
  23  |     await expect(page.getByRole('link', { name: 'Post a Job — FREE' })).toBeVisible();
  24  |     await expect(page.getByRole('link', { name: 'Find Jobs' }).first()).toBeVisible();
  25  |     await expect(page.getByRole('link', { name: 'Find Talent' }).first()).toBeVisible();
  26  |     await expect(page.getByText('Connect with recruiters, consultants, bench sales professionals and companies through one simple hiring marketplace.')).toBeVisible();
  27  |     await expect(page.getByRole('tab', { name: 'Find Jobs' })).toBeVisible();
  28  |     await expect(page.getByRole('tab', { name: 'Find Talent' })).toBeVisible();
  29  |     await expect(page.getByRole('heading', { name: 'How it works' })).toBeVisible();
  30  |     await expect(page.getByRole('heading', { name: 'Ready to make your next hiring move?' })).toBeVisible();
  31  |   });
  32  | 
  33  |   test('shows key CTA and footer links on the home page', async ({ page }) => {
  34  |     await page.goto('/#/home');
  35  | 
  36  |     await expect(page.getByRole('link', { name: 'Sign Up' }).first()).toBeVisible();
  37  |     await expect(page.getByRole('link', { name: 'Log In' }).first()).toBeVisible();
  38  |     await expect(page.getByText('About Us')).toBeVisible();
  39  |     await expect(page.getByText('Help Center')).toBeVisible();
  40  |     await expect(page.getByText('Privacy Policy')).toBeVisible();
  41  |   });
  42  | 
  43  |   test('validates login form before submission', async ({ page }) => {
> 44  |     await page.goto('/#/login');
      |                ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:4200/#/login
  45  | 
  46  |     await page.locator('button:has-text("LOG IN")').click();
  47  | 
  48  |     await expect(page.getByText('Email is required')).toBeVisible();
  49  |     await expect(page.getByText('Password is required.')).toBeVisible();
  50  |     await expect(page.getByRole('heading', { name: 'Log In' })).toBeVisible();
  51  |   });
  52  | 
  53  |   test('validates signup form before submission', async ({ page }) => {
  54  |     await page.goto('/#/signup');
  55  | 
  56  |     await page.locator('button:has-text("SIGN UP")').click();
  57  | 
  58  |     await expect(page.getByText('Email is required')).toBeVisible();
  59  |     await expect(page.getByText('Password is required.')).toBeVisible();
  60  |     await expect(page.getByText('You must agree to the terms & conditions')).toBeVisible();
  61  |   });
  62  | 
  63  |   test('shows invalid login error for wrong credentials', async ({ page }) => {
  64  |     await page.goto('/#/login');
  65  | 
  66  |     await page.locator('input[name="email"]').fill('playwright.invalid@example.com');
  67  |     await page.locator('input[name="password"]').fill('WrongPass123!');
  68  |     await page.locator('button:has-text("LOG IN")').click();
  69  | 
  70  |     await expect(page.getByText('Invalid UserName/Password')).toBeVisible();
  71  |   });
  72  | 
  73  |   test('shows duplicate signup error for an existing email', async ({ page }) => {
  74  |     const email = makeTestEmail('duplicate-signup');
  75  |     const password = 'TestPass123!';
  76  | 
  77  |     await completeSignup(page, email, password);
  78  |     await page.getByText('Logout').click();
  79  |     await page.goto('/#/signup');
  80  |     await page.locator('input[name="email"]').fill(email);
  81  |     await page.locator('input[name="password"]').fill(password);
  82  |     await page.locator('input#agreeTerms').evaluate((el: HTMLInputElement) => {
  83  |       el.checked = true;
  84  |       el.dispatchEvent(new Event('change', { bubbles: true }));
  85  |     });
  86  |     await page.locator('button:has-text("SIGN UP")').click();
  87  | 
  88  |     await expect(page.getByText('User Already exists')).toBeVisible();
  89  |   });
  90  | 
  91  |   test('logout redirects an authenticated user back to login', async ({ page }) => {
  92  |     const email = makeTestEmail('logout-success');
  93  |     const password = 'TestPass123!';
  94  | 
  95  |     await completeSignup(page, email, password);
  96  |     await page.getByText('Logout').click();
  97  | 
  98  |     await expect(page).toHaveURL(/\/\#\/login$/);
  99  |     await expect(page.getByRole('heading', { name: 'Log In' })).toBeVisible();
  100 |   });
  101 | 
  102 |   test('search jobs form accepts a skill and location before submission', async ({ page }) => {
  103 |     await page.goto('/#/search-jobs');
  104 | 
  105 |     await page.locator('input[name="selectedSkill"]').fill('Java');
  106 |     await page.locator('input[name="searchJobLocation"]').fill('Bengaluru');
  107 |     await page.locator('input[name="searchJobLocation"]').press('Enter');
  108 | 
  109 |     await expect(page).toHaveURL(/skill=Java/i);
  110 |     await expect(page).toHaveURL(/location=Bengaluru/i);
  111 |     await expect(page.getByText(/Java - \d+ jobs found/i)).toBeVisible();
  112 |     await expect(page.getByText('Date Posted')).toBeVisible();
  113 |     await expect(page.getByText('Onsite/Remote')).toBeVisible();
  114 |   });
  115 | 
  116 |   test('signup success flow creates a user and redirects to dashboard', async ({ page }) => {
  117 |     const email = makeTestEmail('signup-success');
  118 |     const password = 'TestPass123!';
  119 | 
  120 |     await completeSignup(page, email, password);
  121 |     await expect(page.getByText('You are almost ready to start Hiring or post your jobs. Provide your information below')).toBeVisible();
  122 |   });
  123 | 
  124 |   test('login success flow authenticates an existing user', async ({ page }) => {
  125 |     const email = makeTestEmail('login-success');
  126 |     const password = 'TestPass123!';
  127 | 
  128 |     await completeSignup(page, email, password);
  129 |     await page.evaluate(() => localStorage.clear());
  130 |     await page.goto('/#/login');
  131 |     await page.locator('input[name="email"]').fill(email);
  132 |     await page.locator('input[name="password"]').fill(password);
  133 |     await page.locator('button:has-text("LOG IN")').click();
  134 | 
  135 |     await expect(page).toHaveURL(/\/\#\/dashboard$/);
  136 |     await expect(page.getByText('You are almost ready to start Hiring or post your jobs. Provide your information below')).toBeVisible();
  137 |   });
  138 | });
  139 | 
```