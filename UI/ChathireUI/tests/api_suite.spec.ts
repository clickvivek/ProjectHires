import { test, expect } from '@playwright/test';

const API_BASE = 'http://localhost:5000/api';

test.describe('ChatHire API Test Suite - Core Endpoints', () => {

  test('GET /Common/UserType returns list of user roles', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Common/UserType`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(Array.isArray(body.value)).toBeTruthy();
    expect(body.value.length).toBeGreaterThan(0);
    const names = body.value.map((x: any) => x.name || x.userTypeName);
    expect(names.some((n: string) => /admin/i.test(n) || /recruiter/i.test(n) || /candidate/i.test(n))).toBeTruthy();
  });

  test('GET /Common/Category returns category taxonomy', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Common/Category`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(Array.isArray(body.value)).toBeTruthy();
    expect(body.value.length).toBeGreaterThan(0);
  });

  test('GET /Subscription/AllSubscriptionPlans returns available pricing plans', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Subscription/AllSubscriptionPlans`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(Array.isArray(body.value)).toBeTruthy();
    expect(body.value.length).toBeGreaterThan(0);
    const plans = body.value;
    expect(plans[0]).toHaveProperty('description');
    expect(plans[0]).toHaveProperty('noOfDownloads');
  });

  test('GET /Searched/GetRecentSearches returns recent keyword query logs', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Searched/GetRecentSearches?page=1&pageSize=10`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(Array.isArray(body.value)).toBeTruthy();
  });

  test('GET /Searched/GetAnalyticsSummary returns search metrics', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Searched/GetAnalyticsSummary`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(body.value).toHaveProperty('totalSearches');
    expect(body.value).toHaveProperty('totalJobSearches');
    expect(body.value).toHaveProperty('totalHotlistSearches');
  });

  test('POST /Token fails with error for invalid credentials', async ({ request }) => {
    const res = await request.post(`${API_BASE}/Token`, {
      data: {
        eMail: 'invalid.user.nonexistent@example.com',
        pwd: 'WrongPassword123!'
      }
    });
    const body = await res.json();
    expect(res.status() >= 400 || (body.errors && body.errors.length > 0) || !body.value).toBeTruthy();
  });

  test('POST /Token succeeds for super admin and issues valid JWT token', async ({ request }) => {
    const res = await request.post(`${API_BASE}/Token`, {
      data: {
        eMail: 'vivek1234@sharklasers.com',
        pwd: 'Admin@123'
      }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(body.value).toHaveProperty('token');
    expect(body.value.token.length).toBeGreaterThan(20);
    expect(body.value.userId).toBe(4);
    expect(body.value.userTypeId).toBe(7);
  });

  test('GET /Subscription/CheckDownloadResume verifies unlimited quota for super admin', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Subscription/CheckDownloadResume?fileName=test.pdf&userId=4`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(body.value.canDownload).toBe(true);
    expect(body.value.remainingDownloads).toBeGreaterThanOrEqual(9000);
  });

  test('GET /Promocode/All returns active promocodes', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Promocode/All`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
    expect(Array.isArray(body.value)).toBeTruthy();
  });

  test('GET /Referral/Stats returns referral metrics', async ({ request }) => {
    const res = await request.get(`${API_BASE}/Referral/Stats`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
  });

  test('POST /EmailJobPosting/InboundWebhook processes inbound email dto', async ({ request }) => {
    const res = await request.post(`${API_BASE}/EmailJobPosting/InboundWebhook`, {
      data: {
        from: 'recruiter@example.com',
        to: 'jobs@hires.in',
        subject: 'Job Requirement: Senior .NET Core Developer',
        text: 'Looking for a Senior .NET Developer with 5+ years experience in C#, Azure, and SQL Server.'
      }
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.errors == null || body.errors.length === 0).toBeTruthy();
  });

  test('GET /File/get returns 400 when fileName is missing', async ({ request }) => {
    const res = await request.get(`${API_BASE}/File/get?containerName=resumes`);
    expect(res.status()).toBe(400);
  });

});
