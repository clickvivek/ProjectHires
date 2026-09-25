# 🛡️ ChatHire Comprehensive Security Audit & Vulnerability Assessment Report

**Application**: ChatHire (.NET Core Web API + Angular Frontend)  
**Date**: September 23, 2026  
**Auditor**: Automated Security Review  
**Scope**: Backend API, Frontend UI, Database Access Layer, Storage Services, and Cloud Configurations  

---

## 📊 Executive Summary & Severity Matrix

| ID | Vulnerability Title | Severity | Component | Risk Level |
| :--- | :--- | :---: | :--- | :---: |
| **SEC-01** | Hardcoded Database Password & Azure Storage Keys | 🔴 **CRITICAL** | `DependancyManager.cs`, `FileManager.cs` | High Exploitability / Data Breach |
| **SEC-02** | Unenforced Route Authorization (`[ApiAuthorize]` commented out) | 🟠 **HIGH** | Controllers (`UserController`, `ConsultancyController`, etc.) | Broken Access Control / Unauthorized Actions |
| **SEC-03** | Overly Permissive Wildcard CORS Policy (`AllowAnyOrigin`) | 🟠 **HIGH** | `Program.cs` | Cross-Origin Data Leakage / CSRF |
| **SEC-04** | Server-Side Request Forgery (SSRF) Risk in Scraper | 🟡 **MEDIUM** | `LinkedInScraperService.cs` | Cloud Metadata & Internal Port Scanning |
| **SEC-05** | File Upload Extension & MIME Type Whitelisting | 🟡 **MEDIUM** | `FileManager.cs` | Stored XSS / File Type Spoofing |
| **SEC-06** | Missing Rate Limiting on OTP & Authentication Endpoints | 🔵 **LOW** | `UserController.cs`, `TokenController.cs` | OTP Brute-Forcing & Email Quota Exhaustion |

---

## 🔍 Detailed Vulnerability Findings & Fixes

---

### 🔴 SEC-01: Hardcoded Database Password & Azure Storage Keys

#### 1. Description
Plaintext production credentials and master storage account keys are hardcoded directly in C# source files:

- **Database Credentials** in `UI/ChathireAPI/MiddleWare/Shared/DependancyManager.cs` (Line 20):
  ```csharp
  options.UseSqlServer("Server=ones.database.windows.net,1433;Initial Catalog=HIRES;Persist Security Info=False;User ID=vijisrk;Password=Thayasrk22@@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
  ```
- **Azure Storage Key** in `UI/ChathireAPI/BusinessLayer/Manager/FileManager.cs` (Line 26):
  ```csharp
  _blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=hiresblob;AccountKey=XPt9HT6xcg2SEjcClC1aYbRoDR/PO/Lv8f0IB9TgjTbXEAO286ChXwGFNMlYMNvoE3TCcNaXYIHP+AStJ0IZ5A==;EndpointSuffix=core.windows.net");
  ```

#### 2. Risk & Impact
Anyone with repository access (collaborators, contractors, build logs) possesses master administrative access to your live Azure SQL Database and all Azure Blob Storage files.

#### 3. Remediation Code
1. **Rotate Credentials**: Change the Azure SQL database password and regenerate the Azure Storage access key immediately in the Azure Portal.
2. **Refactor Code**: Load configuration values exclusively via `IConfiguration` or Azure Key Vault:
   ```csharp
   // In DependancyManager.cs:
   public static void ConfigureAPI(IServiceCollection _services, IConfiguration configuration)
   {
       _services.AddDbContext<EFContexts>(options =>
           options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                  .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
   }
   ```

---

### 🟠 SEC-02: Unenforced Route Authorization on Critical Endpoints

#### 1. Description
Several mutating controller endpoints have their authorization attributes commented out:
```csharp
// In UserController.cs:
[HttpPost]
[Route("Add")]
//[ApiAuthorize("AddUser")]
public Task<Result<UserDto>> AddUser(UserDtoForInsert User)
{
    return ExecuteAsync<UserDto>(async () =>
    {
        // Fallback allows unauthenticated requests
        return await mgr.AddUser(User, context ?? GetDummyUserContext());
    });
}
```

#### 2. Risk & Impact
Unauthenticated actors can trigger user modifications, consultancy creations, or quota overrides by calling endpoints without passing a JWT Bearer token.

#### 3. Remediation Code
Enforce `[Authorize]` attributes and reject unauthenticated requests explicitly:
```csharp
[HttpPost]
[Route("Add")]
[Authorize]
public Task<Result<UserDto>> AddUser(UserDtoForInsert User)
{
    return ExecuteAsync<UserDto>(async () =>
    {
        var context = GetUserContext();
        if (context == null || context.UserId <= 0)
        {
            throw new UnauthorizedAccessException("Authentication token is required.");
        }
        return await mgr.AddUser(User, context);
    });
}
```

---

### 🟠 SEC-03: Overly Permissive Wildcard CORS Policy

#### 1. Description
`Program.cs` configures CORS with unrestricted wildcard access:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => 
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});
```

#### 2. Risk & Impact
Any malicious webpage opened in an authenticated user's browser can perform cross-origin AJAX calls against your API.

#### 3. Remediation Code
Restrict origins strictly to your production domains and local dev environments:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("https://www.chathire.com", "https://chathire.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});
```

---

### 🟡 SEC-04: Server-Side Request Forgery (SSRF) Risk in Scraper Service

#### 1. Description
`LinkedInScraperService.cs` accepts arbitrary user-supplied URLs and issues outbound HTTP requests directly from the server without host validation.

#### 2. Risk & Impact
An attacker could submit internal network addresses (e.g., `http://169.254.169.254/` Azure IMDS metadata or `http://127.0.0.1:5000/`) to access private cloud services or port-scan internal infrastructure.

#### 3. Remediation Code
Add URL hostname verification before issuing `HttpClient` requests:
```csharp
private bool IsSafeScrapeUrl(string rawUrl)
{
    if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri)) return false;
    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

    // Disallow IP literals and internal hosts
    if (IPAddress.TryParse(uri.Host, out _)) return false;
    if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return false;

    // Restrict strictly to LinkedIn domains
    return uri.Host.EndsWith("linkedin.com", StringComparison.OrdinalIgnoreCase);
}
```

---

### 🟡 SEC-05: File Upload Extension & MIME Type Whitelisting

#### 1. Description
`FileManager.cs` derives file extensions directly from input file names without validating binary file signatures (magic bytes):
```csharp
string ext = Path.GetExtension(fileModel.ImageFile.FileName);
if (string.IsNullOrEmpty(ext)) ext = ".png";
```

#### 2. Risk & Impact
An attacker could upload executable files, `.html` files, or malicious `.svg` files with embedded scripts that execute when accessed directly in the browser.

#### 3. Remediation Code
Implement strict file extension whitelisting and image format validation:
```csharp
private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
{
    ".png", ".jpg", ".jpeg", ".webp"
};

public static bool IsValidImageExtension(string fileName)
{
    var ext = Path.GetExtension(fileName);
    return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
}
```

---

### 🔵 SEC-06: Rate Limiting on OTP & Authentication Endpoints

#### 1. Description
`/api/User/VerifyOtp`, `/api/User/ResendOtp`, and `/api/Token/Login` endpoints lack request throttling.

#### 2. Risk & Impact
- Attackers can brute-force 6-digit OTP codes.
- Automated bots can flood `/ResendOtp` to exhaust Resend email quotas.

#### 3. Remediation Code
Enable ASP.NET Core rate limiting in `Program.cs`:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("OtpPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // Max 5 attempts per minute
        opt.QueueLimit = 0;
    });
});

// Apply to controller:
[HttpPost("VerifyOtp")]
[EnableRateLimiting("OtpPolicy")]
public Task<Result<bool>> VerifyOtp(...)
```

---

## 🛠️ Security Hardening Checklist & Action Plan

- [ ] **1. Rotate Secrets**: Change Azure SQL database password & Azure Blob storage primary access key.
- [ ] **2. Clean Source Files**: Remove hardcoded connection strings from `DependancyManager.cs` and `FileManager.cs`.
- [ ] **3. Lock Down CORS**: Restrict `Program.cs` CORS policy to `chathire.com` and `localhost:4200`.
- [ ] **4. Enable Auth Guard**: Reinstate `[Authorize]` attributes across all state-mutating controller routes.
- [ ] **5. Restrict Scraper URLs**: Whitelist `*.linkedin.com` in `LinkedInScraperService.cs` to prevent SSRF.
- [ ] **6. Validate File Uploads**: Enforce strict image extension check (`.png`, `.jpg`, `.jpeg`, `.webp`).
- [ ] **7. Apply Rate Limiting**: Limit `/VerifyOtp` and `/ResendOtp` to 5 requests/minute.

---
*Report generated for ChatHire engineering team.*
