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

                result.NormalizedLinkedinUrl = url;

                // Extract company slug from URL
                // Formats: linkedin.com/company/google, linkedin.com/company/google/about, linkedin.com/school/stanford-university
                var slugMatch = Regex.Match(url, @"linkedin\.com\/(?:company|school)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
                string slug = slugMatch.Success ? slugMatch.Groups[1].Value.Trim() : string.Empty;

                if (string.IsNullOrEmpty(slug))
                {
                    // If no company slug matched, attempt to take the last segment
                    var uri = new Uri(url);
                    slug = uri.Segments.LastOrDefault()?.Trim('/') ?? string.Empty;
                }

                result.CompanySlug = slug;

                // Prepare fallback default name from slug
                string fallbackName = HumanizeSlug(slug);
                result.CompanyName = fallbackName;

                // Fetch HTML from LinkedIn public URL
                string html = string.Empty;
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
                    request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                    request.Headers.Add("Sec-Fetch-Dest", "document");
                    request.Headers.Add("Sec-Fetch-Mode", "navigate");
                    request.Headers.Add("Sec-Fetch-Site", "none");
                    request.Headers.Add("Sec-Fetch-User", "?1");
                    request.Headers.Add("Upgrade-Insecure-Requests", "1");

                    using var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        html = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed direct HTTP fetch for LinkedIn URL: {Url}. Proceeding with meta fallbacks.", url);
                }

                if (!string.IsNullOrEmpty(html))
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
                        result.CompanyName = CleanCompanyName(rawTitle);
                    }

                    // 2. Parse og:image (Logo)
                    var ogImageMatch = Regex.Match(html, @"<meta\s+property=[""']og:image[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    if (!ogImageMatch.Success)
                    {
                        ogImageMatch = Regex.Match(html, @"<meta\s+content=[""']([^""']+)[""']\s+property=[""']og:image[""']", RegexOptions.IgnoreCase);
                    }
                    if (ogImageMatch.Success)
                    {
                        result.OriginalLogoUrl = WebUtility.HtmlDecode(ogImageMatch.Groups[1].Value);
                    }

                    // 3. Parse og:description
                    var ogDescMatch = Regex.Match(html, @"<meta\s+property=[""']og:description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    if (!ogDescMatch.Success)
                    {
                        ogDescMatch = Regex.Match(html, @"<meta\s+name=[""']description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    }
                    if (ogDescMatch.Success)
                    {
                        result.Description = WebUtility.HtmlDecode(ogDescMatch.Groups[1].Value);
                    }

                    // 4. Parse JSON-LD Schema
                    var jsonLdMatches = Regex.Matches(html, @"<script\s+type=[""']application\/ld\+json[""'][^>]*>(.*?)<\/script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    foreach (Match m in jsonLdMatches)
                    {
                        try
                        {
                            string jsonContent = m.Groups[1].Value.Trim();
                            if (!string.IsNullOrEmpty(jsonContent) && jsonContent.StartsWith("{"))
                            {
                                var jObj = JObject.Parse(jsonContent);
                                if (jObj["name"] != null && string.IsNullOrWhiteSpace(result.CompanyName))
                                {
                                    result.CompanyName = jObj["name"]?.ToString();
                                }
                                if (jObj["sameAs"] != null && string.IsNullOrWhiteSpace(result.Website))
                                {
                                    result.Website = jObj["sameAs"]?.ToString();
                                }
                                if (jObj["logo"] != null && string.IsNullOrWhiteSpace(result.OriginalLogoUrl))
                                {
                                    var logoToken = jObj["logo"];
                                    result.OriginalLogoUrl = logoToken is JObject lo ? lo["url"]?.ToString() : logoToken?.ToString();
                                }
                            }
                        }
                        catch { }
                    }
                }

                // If Company Name still empty, use humanized slug
                if (string.IsNullOrWhiteSpace(result.CompanyName))
                {
                    result.CompanyName = fallbackName;
                }

                // Derive website & domain if empty
                if (string.IsNullOrWhiteSpace(result.Website))
                {
                    string cleanSlug = slug.ToLowerInvariant().Replace("-", "").Replace("_", "");
                    result.Website = $"https://www.{cleanSlug}.com";
                }

                result.DomainName = ExtractDomain(result.Website);

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

        private async Task<Consultancy> SaveOrUpdateConsultancyAsync(LinkedInCompanyScrapedDto dto, long? adminUserId)
        {
            var now = DateTime.UtcNow;
            string domain = dto.DomainName?.Trim().ToLowerInvariant() ?? string.Empty;
            string linkedin = dto.NormalizedLinkedinUrl?.Trim() ?? string.Empty;
            string name = dto.CompanyName?.Trim() ?? string.Empty;

            // Search for existing record by domain, linkedin or exact name
            Consultancy? existing = null;

            if (!string.IsNullOrEmpty(domain))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => c.Domainname == domain || (c.Website != null && c.Website.Contains(domain)));
            }

            if (existing == null && !string.IsNullOrEmpty(linkedin))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => c.Linkedin != null && c.Linkedin.Contains(dto.CompanySlug ?? linkedin));
            }

            if (existing == null && !string.IsNullOrEmpty(name))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => c.Name != null && c.Name.ToLower() == name.ToLower());
            }

            if (existing != null)
            {
                // Update existing record
                if (!string.IsNullOrWhiteSpace(dto.Website) && string.IsNullOrWhiteSpace(existing.Website))
                {
                    existing.Website = dto.Website;
                }
                if (!string.IsNullOrWhiteSpace(dto.DomainName) && string.IsNullOrWhiteSpace(existing.Domainname))
                {
                    existing.Domainname = dto.DomainName;
                }
                if (!string.IsNullOrWhiteSpace(dto.NormalizedLinkedinUrl))
                {
                    existing.Linkedin = dto.NormalizedLinkedinUrl;
                }
                if (!string.IsNullOrWhiteSpace(dto.AzureLogoFileName))
                {
                    existing.Logo = dto.AzureLogoFileName;
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
                    Domainname = dto.DomainName,
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
                    req.Headers.Add("User-Agent", "Mozilla/5.0");
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
                        using var resp = await _httpClient.GetAsync(clearbitUrl);
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
                        using var resp = await _httpClient.GetAsync(googleFaviconUrl);
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
                    await blobClient.UploadAsync(stream, overwrite: true);

                    return fileName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed uploading logo to Azure Blob Storage: {FileName}", fileName);
                }
            }

            return fileName;
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
    }
}
