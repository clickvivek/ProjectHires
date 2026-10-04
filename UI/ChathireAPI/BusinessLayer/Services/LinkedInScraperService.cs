using Azure.Storage.Blobs;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class LinkedInScraperService : ILinkedInScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly EFContexts _context;
        private readonly ILogger<LinkedInScraperService> _logger;
        private readonly string _azureConnectionString;

        public LinkedInScraperService(
            HttpClient httpClient,
            EFContexts context,
            IConfiguration configuration,
            ILogger<LinkedInScraperService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _azureConnectionString = configuration.GetConnectionString("AzureStorage")
                ?? "DefaultEndpointsProtocol=https;AccountName=hiresblob;AccountKey=XPt9HT6xcg2SEjcClC1aYbRoDR/PO/Lv8f0IB9TgjTbXEAO286ChXwGFNMlYMNvoE3TCcNaXYIHP+AStJ0IZ5A==;EndpointSuffix=core.windows.net";
        }

        public async Task<LinkedInCompanyScrapedDto> ScrapeCompanyAsync(string rawUrl)
        {
            var result = new LinkedInCompanyScrapedDto
            {
                InputUrl = rawUrl?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(result.InputUrl))
            {
                result.Status = "Failed";
                result.StatusMessage = "Empty URL provided.";
                return result;
            }

            try
            {
                // Normalize URL
                string url = result.InputUrl;
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                // Extract company or school slug from URL
                // Formats:
                // https://www.linkedin.com/company/atria-consulting-llc/about/
                // https://www.linkedin.com/company/atria-consulting-llc/
                // https://www.linkedin.com/company/atria-consulting-llc
                // https://www.linkedin.com/school/stanford-university/about/
                var slugMatch = Regex.Match(url, @"linkedin\.com\/(company|school)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
                string entityType = "company";
                string slug = string.Empty;

                if (slugMatch.Success)
                {
                    entityType = slugMatch.Groups[1].Value.ToLowerInvariant();
                    slug = slugMatch.Groups[2].Value.Trim();
                }
                else
                {
                    var genericMatch = Regex.Match(url, @"linkedin\.com\/(?:[a-zA-Z0-9\-_%]+\/)?([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
                    if (genericMatch.Success)
                    {
                        slug = genericMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        var uri = new Uri(url);
                        slug = uri.Segments.LastOrDefault()?.Trim('/') ?? string.Empty;
                    }
                }

                result.CompanySlug = slug;

                // Canonical URL: LinkedIn guest endpoint for company/school profile
                // NOTE: Requesting sub-paths like /about/ or /jobs/ redirects unauthenticated requests (HTTP 302) to the LinkedIn Login wall!
                // The canonical URL https://www.linkedin.com/{entityType}/{slug} responds with 200 OK and contains full schema, about details, logo, and website.
                string canonicalUrl = !string.IsNullOrEmpty(slug) ? $"https://www.linkedin.com/{entityType}/{slug}" : url;
                result.NormalizedLinkedinUrl = canonicalUrl;

                // Prepare fallback default name from slug
                string fallbackName = HumanizeSlug(slug);
                result.CompanyName = fallbackName;

                // Fetch HTML from LinkedIn public URL using clean headers and system curl fallback
                string html = await FetchLinkedInHtmlAsync(canonicalUrl);

                if (!string.IsNullOrEmpty(html))
                {
                    bool isLoginWall = html.Contains("<title>LinkedIn Login", StringComparison.OrdinalIgnoreCase) ||
                                       html.Contains("Login to LinkedIn", StringComparison.OrdinalIgnoreCase);

                    if (!isLoginWall)
                    {
                        // 1. Parse og:title
                        var ogTitleMatch = Regex.Match(html, @"<meta\s+property=[""']og:title[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        if (!ogTitleMatch.Success)
                        {
                            ogTitleMatch = Regex.Match(html, @"<meta\s+content=[""']([^""']+)[""']\s+property=[""']og:title[""']", RegexOptions.IgnoreCase);
                        }
                        if (ogTitleMatch.Success)
                        {
                            string rawTitle = WebUtility.HtmlDecode(ogTitleMatch.Groups[1].Value);
                            string cleanedTitle = CleanCompanyName(rawTitle);
                            if (!string.IsNullOrWhiteSpace(cleanedTitle) && !cleanedTitle.Contains("LinkedIn Login", StringComparison.OrdinalIgnoreCase))
                            {
                                result.CompanyName = cleanedTitle;
                            }
                        }

                        // 2. Parse og:image (Logo)
                        var ogImageMatch = Regex.Match(html, @"<meta\s+property=[""']og:image[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        if (!ogImageMatch.Success)
                        {
                            ogImageMatch = Regex.Match(html, @"<meta\s+content=[""']([^""']+)[""']\s+property=[""']og:image[""']", RegexOptions.IgnoreCase);
                        }
                        if (ogImageMatch.Success)
                        {
                            string candidateLogo = WebUtility.HtmlDecode(ogImageMatch.Groups[1].Value);
                            if (!candidateLogo.Contains("static.licdn.com/scds/common/u/images/logos/favicons", StringComparison.OrdinalIgnoreCase))
                            {
                                result.OriginalLogoUrl = candidateLogo;
                            }
                        }

                        // 3. Parse og:description
                        var ogDescMatch = Regex.Match(html, @"<meta\s+property=[""']og:description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        if (!ogDescMatch.Success)
                        {
                            ogDescMatch = Regex.Match(html, @"<meta\s+name=[""']description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                        }
                        if (ogDescMatch.Success)
                        {
                            string desc = WebUtility.HtmlDecode(ogDescMatch.Groups[1].Value);
                            if (!desc.Contains("Login to LinkedIn", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Description = desc;
                            }
                        }

                        // 4. Parse JSON-LD Schema (including @graph structures)
                        var jsonLdMatches = Regex.Matches(html, @"<script\s+type=[""']application\/ld\+json[""'][^>]*>(.*?)<\/script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        foreach (Match m in jsonLdMatches)
                        {
                            try
                            {
                                string jsonContent = m.Groups[1].Value.Trim();
                                if (!string.IsNullOrEmpty(jsonContent))
                                {
                                    var rootToken = JToken.Parse(jsonContent);
                                    void TraverseTokens(JToken token)
                                    {
                                        if (token is JObject obj)
                                        {
                                            string? type = obj["@type"]?.ToString();
                                            bool isOrg = string.Equals(type, "Organization", StringComparison.OrdinalIgnoreCase) || obj["sameAs"] != null;
                                            if (isOrg)
                                            {
                                                if (obj["name"] != null && (string.IsNullOrWhiteSpace(result.CompanyName) || result.CompanyName == fallbackName))
                                                {
                                                    string nameVal = obj["name"]?.ToString() ?? string.Empty;
                                                    if (!string.IsNullOrWhiteSpace(nameVal))
                                                    {
                                                        result.CompanyName = CleanCompanyName(nameVal);
                                                    }
                                                }
                                                if (obj["sameAs"] != null && string.IsNullOrWhiteSpace(result.Website))
                                                {
                                                    string sameAs = obj["sameAs"]?.ToString()?.Trim() ?? string.Empty;
                                                    if (IsValidCompanyWebsite(sameAs))
                                                    {
                                                        result.Website = EnsureScheme(sameAs);
                                                    }
                                                }
                                                if (obj["logo"] != null && string.IsNullOrWhiteSpace(result.OriginalLogoUrl))
                                                {
                                                    var logoToken = obj["logo"];
                                                    string? logoUrl = logoToken is JObject lo ? (lo["contentUrl"]?.ToString() ?? lo["url"]?.ToString()) : logoToken?.ToString();
                                                    if (!string.IsNullOrWhiteSpace(logoUrl) && !logoUrl.Contains("favicons", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        result.OriginalLogoUrl = logoUrl;
                                                    }
                                                }
                                                if (obj["description"] != null && string.IsNullOrWhiteSpace(result.Description))
                                                {
                                                    result.Description = obj["description"]?.ToString();
                                                }
                                            }

                                            foreach (var prop in obj.Properties())
                                            {
                                                TraverseTokens(prop.Value);
                                            }
                                        }
                                        else if (token is JArray arr)
                                        {
                                            foreach (var item in arr)
                                            {
                                                TraverseTokens(item);
                                            }
                                        }
                                    }

                                    TraverseTokens(rootToken);
                                }
                            }
                            catch { }
                        }

                        // 5. Parse LinkedIn Outbound Redirect Links for website (e.g. trk=about_website or about_website tracking)
                        if (string.IsNullOrWhiteSpace(result.Website))
                        {
                            var aboutRedirMatch = Regex.Match(html, @"redir\/redirect\?url=([^&""'>\s]+)[^""'>]*(?:trk=about_website|about_website)", RegexOptions.IgnoreCase);
                            if (!aboutRedirMatch.Success)
                            {
                                aboutRedirMatch = Regex.Match(html, @"(?:trk=about_website|about_website)[^""'>]*redir\/redirect\?url=([^&""'>\s]+)", RegexOptions.IgnoreCase);
                            }
                            if (aboutRedirMatch.Success)
                            {
                                string decoded = WebUtility.UrlDecode(aboutRedirMatch.Groups[1].Value);
                                if (IsValidCompanyWebsite(decoded))
                                {
                                    result.Website = EnsureScheme(decoded);
                                }
                            }
                        }

                        // 6. Parse any outbound redir/redirect?url= matching an external company domain
                        if (string.IsNullOrWhiteSpace(result.Website))
                        {
                            var redirMatches = Regex.Matches(html, @"redir\/redirect\?url=([^&""'>\s]+)", RegexOptions.IgnoreCase);
                            foreach (Match rm in redirMatches)
                            {
                                string decoded = WebUtility.UrlDecode(rm.Groups[1].Value);
                                if (IsValidCompanyWebsite(decoded))
                                {
                                    result.Website = EnsureScheme(decoded);
                                    break;
                                }
                            }
                        }

                        // 7. Parse anchor text in data-tracking-control-name="about_website"
                        if (string.IsNullOrWhiteSpace(result.Website))
                        {
                            var aboutTagMatch = Regex.Match(html, @"data-tracking-control-name=[""']about_website[""'][^>]*>\s*([^\s<]+)", RegexOptions.IgnoreCase);
                            if (aboutTagMatch.Success)
                            {
                                string tagText = WebUtility.HtmlDecode(aboutTagMatch.Groups[1].Value.Trim());
                                if (IsValidCompanyWebsite(tagText))
                                {
                                    result.Website = EnsureScheme(tagText);
                                }
                            }
                        }

                        // 8. Parse Definition list for Website
                        if (string.IsNullOrWhiteSpace(result.Website))
                        {
                            var dtDdMatch = Regex.Match(html, @"Website\s*<\/(?:dt|div|p)>\s*<(?:dd|div|p)[^>]*>\s*<a[^>]+href=[""']([^""']+)[""'][^>]*>\s*([^<]+)<\/a>", RegexOptions.IgnoreCase);
                            if (dtDdMatch.Success)
                            {
                                string linkUrl = dtDdMatch.Groups[1].Value;
                                string linkText = dtDdMatch.Groups[2].Value.Trim();
                                if (linkUrl.Contains("redir/redirect?url="))
                                {
                                    var m = Regex.Match(linkUrl, @"url=([^&""'>\s]+)");
                                    if (m.Success) linkUrl = WebUtility.UrlDecode(m.Groups[1].Value);
                                }
                                if (IsValidCompanyWebsite(linkUrl))
                                {
                                    result.Website = EnsureScheme(linkUrl);
                                }
                                else if (IsValidCompanyWebsite(linkText))
                                {
                                    result.Website = EnsureScheme(linkText);
                                }
                            }
                        }

                        // 9. Parse description text matching ("Visit our website at ...")
                        if (string.IsNullOrWhiteSpace(result.Website))
                        {
                            var descWebsiteMatch = Regex.Match(html, @"(?:Visit our website at|Website:?)\s*(https?:\/\/[^\s<""'\]\)]+)", RegexOptions.IgnoreCase);
                            if (descWebsiteMatch.Success)
                            {
                                string raw = descWebsiteMatch.Groups[1].Value.Trim();
                                if (IsValidCompanyWebsite(raw))
                                {
                                    result.Website = EnsureScheme(raw);
                                }
                            }
                        }
                    }
                }

                // If Company Name still empty or contains login wall text, use humanized slug
                if (string.IsNullOrWhiteSpace(result.CompanyName) || result.CompanyName.Contains("LinkedIn Login", StringComparison.OrdinalIgnoreCase))
                {
                    result.CompanyName = fallbackName;
                }

                // Finalize website & domain
                if (!string.IsNullOrWhiteSpace(result.Website))
                {
                    result.Website = EnsureScheme(result.Website);
                    result.DomainName = ExtractDomain(result.Website);
                }
                else
                {
                    // Do not invent fake websites from slugs when none was found
                    result.Website = string.Empty;
                    result.DomainName = string.Empty;
                }

                // Upload/Save logo to Azure Blob Storage
                string azureLogoFile = await SaveLogoToAzureBlobAsync(result.OriginalLogoUrl, result.DomainName, result.CompanyName);
                result.AzureLogoFileName = azureLogoFile;

                result.Status = "Completed";
                result.StatusMessage = "Successfully extracted details.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping LinkedIn company URL: {Url}", rawUrl);
                result.Status = "Failed";
                result.StatusMessage = ex.Message;
            }

            return result;
        }

        public async Task<List<LinkedInCompanyScrapedDto>> ScrapeAndSaveCompaniesAsync(List<string> urls, bool autoSaveToDb, long? adminUserId)
        {
            var results = new List<LinkedInCompanyScrapedDto>();
            if (urls == null || !urls.Any())
            {
                return results;
            }

            // Filter out empty or duplicate entries in the request batch
            var distinctUrls = urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var url in distinctUrls)
            {
                var dto = await ScrapeCompanyAsync(url);

                if (autoSaveToDb && dto.Status == "Completed" && !string.IsNullOrWhiteSpace(dto.CompanyName))
                {
                    try
                    {
                        var consultancy = await SaveOrUpdateConsultancyAsync(dto, adminUserId);
                        dto.ConsultancyId = consultancy.Id;
                        dto.IsNewRecord = (dto.StatusMessage == "Created new company");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save scraped company to DB: {CompanyName}", dto.CompanyName);
                        dto.Status = "Failed";
                        dto.StatusMessage = $"DB Save error: {ex.Message}";
                    }
                }

                results.Add(dto);
            }

            return results;
        }

        public async Task<LinkedInBatchUpdateResultDto> ReviewAndUpdateLinkedInWebsitesAsync(
            int? limit = null,
            int? offset = null,
            int concurrency = 3,
            long? specificConsultancyId = null)
        {
            var result = new LinkedInBatchUpdateResultDto();

            IQueryable<Consultancy> query = _context.Consultancies
                .Where(c => c.Linkedin != null && c.Linkedin.Trim() != "");

            if (specificConsultancyId.HasValue && specificConsultancyId.Value > 0)
            {
                query = query.Where(c => c.Id == specificConsultancyId.Value);
            }
            else
            {
                query = query.OrderBy(c => c.Id);
                if (offset.HasValue && offset.Value > 0)
                {
                    query = query.Skip(offset.Value);
                }
                if (limit.HasValue && limit.Value > 0)
                {
                    query = query.Take(limit.Value);
                }
            }

            var consultancies = await query
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Website,
                    c.Website2,
                    c.Domainname,
                    c.Linkedin,
                    c.Logo
                })
                .ToListAsync();

            result.TotalScanned = consultancies.Count;
            if (result.TotalScanned == 0) return result;

            _logger.LogInformation("Starting LinkedIn review for {Count} consultancies (concurrency: {Concurrency})", consultancies.Count, concurrency);

            int workerCount = Math.Max(1, Math.Min(concurrency, 5));
            var semaphore = new SemaphoreSlim(workerCount);
            var updatesToApply = new System.Collections.Concurrent.ConcurrentBag<(long Id, LinkedInCompanyScrapedDto Scraped, string OldWeb, string OldDomain, string OldLogo, string OldName, bool Changed)>();

            var tasks = consultancies.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var scraped = await ScrapeCompanyAsync(item.Linkedin!);

                    bool isWebsiteValid = !string.IsNullOrWhiteSpace(scraped.Website) && IsValidCompanyWebsite(scraped.Website);
                    bool isWebsiteChanged = isWebsiteValid && !string.Equals(
                        NormalizeWebCompare(item.Website),
                        NormalizeWebCompare(scraped.Website),
                        StringComparison.OrdinalIgnoreCase);

                    bool changed = isWebsiteChanged || (string.IsNullOrWhiteSpace(item.Website) && isWebsiteValid);

                    updatesToApply.Add((item.Id, scraped, item.Website ?? string.Empty, item.Domainname ?? string.Empty, item.Logo ?? string.Empty, item.Name ?? string.Empty, changed));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reviewing consultancy {Id} ({Linkedin})", item.Id, item.Linkedin);
                }
                finally
                {
                    await Task.Delay(300);
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            var now = DateTime.UtcNow;
            foreach (var update in updatesToApply)
            {
                var consultancy = await _context.Consultancies.FindAsync(update.Id);
                if (consultancy == null) continue;

                if (update.Changed)
                {
                    if (!string.IsNullOrWhiteSpace(consultancy.Website) && string.IsNullOrWhiteSpace(consultancy.Website2) &&
                        !string.Equals(consultancy.Website, update.Scraped.Website, StringComparison.OrdinalIgnoreCase))
                    {
                        consultancy.Website2 = consultancy.Website;
                    }

                    consultancy.Website = update.Scraped.Website;

                    if (!string.IsNullOrWhiteSpace(update.Scraped.DomainName))
                    {
                        consultancy.Domainname = update.Scraped.DomainName;
                    }

                    if (!string.IsNullOrWhiteSpace(update.Scraped.AzureLogoFileName))
                    {
                        consultancy.Logo = update.Scraped.AzureLogoFileName;
                    }

                    if (!string.IsNullOrWhiteSpace(update.Scraped.NormalizedLinkedinUrl) &&
                        !string.Equals(consultancy.Linkedin, update.Scraped.NormalizedLinkedinUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        consultancy.Linkedin = update.Scraped.NormalizedLinkedinUrl;
                    }

                    if (!string.IsNullOrWhiteSpace(update.Scraped.CompanyName) &&
                        !update.Scraped.CompanyName.Contains("LinkedIn Login", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(consultancy.Name) || consultancy.Name.ToLowerInvariant() == update.OldName.ToLowerInvariant())
                        {
                            consultancy.Name = update.Scraped.CompanyName;
                        }
                    }

                    consultancy.Updated = now;

                    result.UpdatedCount++;
                    result.UpdatedItems.Add(new LinkedInUpdateItemDto
                    {
                        ConsultancyId = update.Id,
                        CompanyName = consultancy.Name ?? string.Empty,
                        LinkedinUrl = consultancy.Linkedin ?? string.Empty,
                        OldWebsite = update.OldWeb,
                        NewWebsite = update.Scraped.Website ?? string.Empty,
                        OldDomain = update.OldDomain,
                        NewDomain = update.Scraped.DomainName ?? string.Empty,
                        Logo = consultancy.Logo ?? string.Empty
                    });
                }
                else if (update.Scraped.Status == "Completed")
                {
                    bool minorUpdate = false;
                    if (string.IsNullOrWhiteSpace(consultancy.Domainname) && !string.IsNullOrWhiteSpace(update.Scraped.DomainName))
                    {
                        consultancy.Domainname = update.Scraped.DomainName;
                        minorUpdate = true;
                    }
                    if (string.IsNullOrWhiteSpace(consultancy.Logo) && !string.IsNullOrWhiteSpace(update.Scraped.AzureLogoFileName))
                    {
                        consultancy.Logo = update.Scraped.AzureLogoFileName;
                        minorUpdate = true;
                    }
                    if (!string.IsNullOrWhiteSpace(update.Scraped.NormalizedLinkedinUrl) &&
                        !string.Equals(consultancy.Linkedin, update.Scraped.NormalizedLinkedinUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        consultancy.Linkedin = update.Scraped.NormalizedLinkedinUrl;
                        minorUpdate = true;
                    }
                    if (minorUpdate)
                    {
                        consultancy.Updated = now;
                    }
                    result.UnchangedCount++;
                }
                else
                {
                    result.FailedCount++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("LinkedIn review complete: Scanned={Total}, Updated={Updated}, Unchanged={Unchanged}, Failed={Failed}",
                result.TotalScanned, result.UpdatedCount, result.UnchangedCount, result.FailedCount);

            return result;
        }

        private static string NormalizeWebCompare(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            return url.Trim()
                .ToLowerInvariant()
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("www.", "")
                .TrimEnd('/');
        }

        private async Task<Consultancy> SaveOrUpdateConsultancyAsync(LinkedInCompanyScrapedDto dto, long? adminUserId)
        {
            var now = DateTime.UtcNow;
            string domain = DataAccessLayer.Repository.ConsultancyRepository.ExtractNormalizedDomain(dto.DomainName ?? dto.Website);
            string linkedin = dto.NormalizedLinkedinUrl?.Trim() ?? string.Empty;
            string name = dto.CompanyName?.Trim() ?? string.Empty;
            string linkedinSlug = DataAccessLayer.Repository.ConsultancyRepository.ExtractLinkedInSlug(dto.CompanySlug ?? linkedin);
            string cleanName = name.ToLowerInvariant();

            // Search for existing record by domain, linkedin or exact name
            Consultancy? existing = null;

            if (!string.IsNullOrEmpty(domain))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    (c.Domainname != null && (c.Domainname.ToLower() == domain || c.Domainname.ToLower().Contains(domain) || domain.Contains(c.Domainname.ToLower()))) || 
                    (c.Website != null && c.Website.ToLower().Contains(domain)));
            }

            if (existing == null && !string.IsNullOrEmpty(linkedinSlug))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    c.Linkedin != null && c.Linkedin.ToLower().Contains(linkedinSlug));
            }

            if (existing == null && !string.IsNullOrEmpty(cleanName))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    c.Name != null && c.Name.ToLower() == cleanName);
            }

            if (existing != null)
            {
                // Update existing record
                if (!string.IsNullOrWhiteSpace(dto.Website))
                {
                    if (!string.Equals(existing.Website, dto.Website, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(existing.Website) && string.IsNullOrWhiteSpace(existing.Website2))
                        {
                            existing.Website2 = existing.Website;
                        }
                        existing.Website = dto.Website;
                    }
                }
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    existing.Domainname = domain;
                }
                if (!string.IsNullOrWhiteSpace(dto.NormalizedLinkedinUrl))
                {
                    existing.Linkedin = dto.NormalizedLinkedinUrl;
                }
                if (!string.IsNullOrWhiteSpace(dto.AzureLogoFileName))
                {
                    if (string.IsNullOrWhiteSpace(existing.Logo) || !string.IsNullOrWhiteSpace(dto.OriginalLogoUrl))
                    {
                        existing.Logo = dto.AzureLogoFileName;
                    }
                }
                string fallbackName = HumanizeSlug(dto.CompanySlug ?? linkedinSlug);
                if (!string.IsNullOrWhiteSpace(dto.CompanyName) && 
                    (string.IsNullOrWhiteSpace(existing.Name) || existing.Name.Contains("LinkedIn Login", StringComparison.OrdinalIgnoreCase) || existing.Name == fallbackName))
                {
                    existing.Name = dto.CompanyName;
                }

                existing.Active = true;
                existing.Updated = now;
                if (adminUserId.HasValue && adminUserId.Value > 0)
                {
                    existing.UpdatedBy = adminUserId.Value;
                }

                await _context.SaveChangesAsync();
                dto.StatusMessage = "Updated existing company";
                return existing;
            }
            else
            {
                // Insert new record
                var newConsultancy = new Consultancy
                {
                    Name = dto.CompanyName,
                    Website = dto.Website,
                    Domainname = domain,
                    Linkedin = dto.NormalizedLinkedinUrl,
                    Logo = dto.AzureLogoFileName,
                    Active = true,
                    StatusId = 1,
                    IsDirectCompany = true,
                    Updated = now,
                    UpdatedBy = adminUserId.HasValue && adminUserId.Value > 0 ? adminUserId.Value : 1
                };

                _context.Consultancies.Add(newConsultancy);
                await _context.SaveChangesAsync();
                dto.StatusMessage = "Created new company";
                return newConsultancy;
            }
        }

        private async Task<string> SaveLogoToAzureBlobAsync(string? originalLogoUrl, string? domain, string? companyName)
        {
            string fileName = CleanFileName(domain, companyName) + ".png";

            byte[]? imageBytes = null;

            // 1. Try downloading original logo
            if (!string.IsNullOrWhiteSpace(originalLogoUrl) && originalLogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, originalLogoUrl);
                    req.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
                    req.Headers.Add("Accept", "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
                    using var resp = await _httpClient.SendAsync(req);
                    if (resp.IsSuccessStatusCode)
                    {
                        imageBytes = await resp.Content.ReadAsByteArrayAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed downloading logo from {Url}", originalLogoUrl);
                }
            }

            // 2. Fallback to Clearbit / Google Favicon if original logo failed
            if (imageBytes == null || imageBytes.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    string clearbitUrl = $"https://logo.clearbit.com/{domain}";
                    try
                    {
                        using var req = new HttpRequestMessage(HttpMethod.Get, clearbitUrl);
                        req.Headers.Add("User-Agent", "Mozilla/5.0");
                        using var resp = await _httpClient.SendAsync(req);
                        if (resp.IsSuccessStatusCode)
                        {
                            imageBytes = await resp.Content.ReadAsByteArrayAsync();
                        }
                    }
                    catch { }
                }
            }

            if (imageBytes == null || imageBytes.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    string googleFaviconUrl = $"https://www.google.com/s2/favicons?domain={domain}&sz=128";
                    try
                    {
                        using var req = new HttpRequestMessage(HttpMethod.Get, googleFaviconUrl);
                        req.Headers.Add("User-Agent", "Mozilla/5.0");
                        using var resp = await _httpClient.SendAsync(req);
                        if (resp.IsSuccessStatusCode)
                        {
                            imageBytes = await resp.Content.ReadAsByteArrayAsync();
                        }
                    }
                    catch { }
                }
            }

            // 3. Upload to Azure Blob Storage container "profilepic"
            if (imageBytes != null && imageBytes.Length > 0)
            {
                try
                {
                    var blobServiceClient = new BlobServiceClient(_azureConnectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient("profilepic");
                    await containerClient.CreateIfNotExistsAsync();

                    var blobClient = containerClient.GetBlobClient(fileName);
                    using var stream = new MemoryStream(imageBytes);
                    var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
                    {
                        HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                        {
                            ContentType = "image/png",
                            CacheControl = "no-cache, no-store, must-revalidate"
                        }
                    };
                    await blobClient.UploadAsync(stream, options);

                    return fileName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed uploading logo to Azure Blob Storage: {FileName}", fileName);
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private string ExtractDomain(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }
                var uri = new Uri(url);
                string host = uri.Host.ToLowerInvariant();
                if (host.StartsWith("www.")) host = host.Substring(4);
                return host;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string HumanizeSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return "Company";
            slug = slug.Replace("-", " ").Replace("_", " ");
            var words = slug.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
                }
            }
            return string.Join(" ", words);
        }

        private string CleanCompanyName(string rawTitle)
        {
            if (string.IsNullOrWhiteSpace(rawTitle)) return string.Empty;

            // Remove common LinkedIn titles suffixes
            string cleaned = Regex.Replace(rawTitle, @"\s*\|\s*LinkedIn.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @":\s*Overview.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @":\s*About.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\s*-\s*LinkedIn.*$", "", RegexOptions.IgnoreCase);

            return cleaned.Trim();
        }

        private string CleanFileName(string? domain, string? companyName)
        {
            string target = !string.IsNullOrWhiteSpace(domain) ? domain : companyName ?? "company_logo";
            target = target.ToLowerInvariant().Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim('/', ' ', '\\');
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                target = target.Replace(c, '_');
            }
            target = target.Replace(".", "_").Replace(" ", "_");
            return target;
        }

        private static bool IsValidCompanyWebsite(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

            string host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("www.")) host = host.Substring(4);

            string[] ignoredDomains = new[]
            {
                "linkedin.com", "licdn.com", "lnkd.in",
                "facebook.com", "fb.com",
                "twitter.com", "x.com", "t.co",
                "instagram.com",
                "youtube.com", "youtu.be",
                "google.com", "goo.gl",
                "apple.com", "microsoft.com", "play.google.com",
                "pinterest.com", "tiktok.com"
            };

            foreach (var d in ignoredDomains)
            {
                if (host == d || host.EndsWith("." + d))
                {
                    return false;
                }
            }

            return host.Contains('.');
        }

        private static string EnsureScheme(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            url = url.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }
            return url;
        }

        private async Task<string> FetchLinkedInHtmlAsync(string canonicalUrl)
        {
            string html = string.Empty;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, canonicalUrl);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

                using var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    html = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed direct HTTP fetch for LinkedIn URL: {Url}. Trying curl fallback.", canonicalUrl);
            }

            // Fallback: If HttpClient was blocked (e.g. HTTP 999 or empty or login wall), attempt using system curl
            if (string.IsNullOrWhiteSpace(html) || html.Contains("<title>LinkedIn Login", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "curl.exe",
                        Arguments = $"-s -L -H \"User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36\" -H \"Accept-Language: en-US,en;q=0.9\" \"{canonicalUrl}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process != null)
                    {
                        string curlOutput = await process.StandardOutput.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        if (!string.IsNullOrWhiteSpace(curlOutput) && curlOutput.Length > 500)
                        {
                            html = curlOutput;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Curl fallback failed for LinkedIn URL: {Url}", canonicalUrl);
                }
            }

            return html;
        }
    }
}
