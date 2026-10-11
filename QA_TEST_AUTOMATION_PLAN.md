# ChatHire QA Automation & Test Plan Matrix
**Author**: Lead QA Test Automation Architect (15+ Years Experience)  
**Target Platform**: ChatHire Web (Angular 16 SPA Frontend + ASP.NET Core API Backend + Azure SQL + Azure Blob Storage)

---

## 1. Executive Summary & Test Strategy
ChatHire is an enterprise recruitment marketplace and candidate management platform connecting job seekers, bench consultants, recruiters, and companies. 

The test strategy establishes three layers of automated quality assurance:
1. **API Integration & Contract Testing**: Direct HTTP validation of .NET Core endpoints across controllers (`Subscription`, `Token`, `Candidate`, `JobOpening`, `Searched`, `Referral`, `File`, `Common`, `EmailJobPosting`).
2. **End-to-End (E2E) & UI Journey Testing**: Headless browser automation via Playwright validating authentication, role-based view permissions, search filters, and Admin Setup workflows.
3. **Business Rule & Security Validation**: Enforcement of user quotas, cycle-based limits, JWT bearer tokens, and protection of candidate resumes against unauthorized downloads.

---

## 2. API Test Coverage Matrix

| Controller | Endpoint | Method | Test Scenarios | Status |
| :--- | :--- | :---: | :--- | :---: |
| **TokenController** | `/api/Token` | `POST` | Valid credentials issue JWT token; Invalid credentials return 400/error | ✅ Automated |
| **TokenController** | `/api/Token/Google` | `POST` | Google ID token verification and exchange | Ready |
| **SubscriptionController** | `/api/Subscription/AllSubscriptionPlans` | `GET` | Returns active subscription plans, pricing, download limits | ✅ Automated |
| **SubscriptionController** | `/api/Subscription/CheckDownloadResume` | `GET` | Validates resume download quota, Super Admin unlimited, billing cycle bounds | ✅ Automated |
| **CommonController** | `/api/Common/UserType` | `GET` | Validates roles (Super Admin, Recruiter, Candidate) | ✅ Automated |
| **CommonController** | `/api/Common/Category` | `GET` | Returns skill/job categories taxonomy | ✅ Automated |
| **SearchedController** | `/api/Searched/GetRecentSearches` | `GET` | Pagination and filter by searchType ('jobs', 'hotlist') | ✅ Automated |
| **SearchedController** | `/api/Searched/GetAnalyticsSummary` | `GET` | Aggregated count of total searches, job searches, hotlist searches | ✅ Automated |
| **PromocodeController** | `/api/Promocode/All` | `GET` | Returns active promocodes and discounts | ✅ Automated |
| **ReferralController** | `/api/Referral/Stats` | `GET` | Aggregated referral counts, points, and invite statuses | ✅ Automated |
| **EmailJobPostingController** | `/api/EmailJobPosting/InboundWebhook` | `POST` | Parses incoming multipart email into candidate/job queue | ✅ Automated |
| **FileController** | `/api/File/get` | `GET` | Returns 400 when missing fileName; streams resume when valid | ✅ Automated |
| **CandidateController** | `/api/Candidate/GetProfile` | `GET` | Returns composite candidate profile by userId | Planned |
| **JobOpeningController** | `/api/JobOpening/ApplyWithResume` | `POST` | Multipart upload and application mapping | Planned |

---

## 3. UI Screen & Functional Test Matrix

| Screen / Feature | Route / URL | Role | Key Verification Points | Status |
| :--- | :--- | :---: | :--- | :---: |
| **Landing & Homepage** | `/#/home` | Public | Hero banner, header CTA buttons, footer links | ✅ Automated |
| **Info Pages** | `/#/about`, `/#/contact`, `/#/faq`, `/#/terms`, `/#/privacy` | Public | Screen rendering, contact info, policy documents | ✅ Automated |
| **Job Search** | `/#/search-jobs` | Public | Skill query parameters, location input, facet filters | ✅ Automated |
| **Hotlist Search** | `/#/search-hotlist` | Public | Talent search input, filter chips, candidate summary cards | ✅ Automated |
| **Authentication (Login)** | `/#/login` | Public | Form validation (empty email/password), show/hide password toggle | ✅ Automated |
| **Authentication (Signup)** | `/#/signup` | Public | Form validation, mandatory terms checkbox, duplicate email check | ✅ Automated |
| **Admin Setup - DAU & Analytics** | `/#/Adminsetup` | Super Admin | Tab switching, live active user count, activity graphs | ✅ Automated |
| **Admin Setup - Searched Tab** | `/#/Adminsetup` | Super Admin | Real-time search log table, IP address, user location, keyword pills | ✅ Automated |
| **Admin Setup - Bulk Upload** | `/#/Adminsetup` | Super Admin | CSV upload dropzone, row parsing table, commit button | Planned |
| **Recruiter Dashboard** | `/#/dashboard` | Recruiter | Authenticated session access, profile completion wizard | ✅ Automated |
| **Resume View & Download** | Modal / Drawer | Recruiter | Resume drawer preview, quota check before download, 403 quota toast | ✅ Automated |
| **Chat & Messaging** | `/#/chat` | Authenticated | TalkJS conversation loading, message button spinner | ✅ Automated |

---

## 4. Automation Suite Execution Commands

### To run the API Automation Suite:
```bash
npx playwright test tests/api_suite.spec.ts
```

### To run the UI Screen Automation Suite:
```bash
npx playwright test tests/ui_screens.spec.ts
```

### To run all suites end-to-end:
```bash
npx playwright test
```
