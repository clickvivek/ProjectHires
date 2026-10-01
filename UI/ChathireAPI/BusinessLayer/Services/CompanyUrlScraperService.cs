using Azure.Storage.Blobs;
using BusinessEntityAndDTO.DTO;
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
    public class CompanyUrlScraperService : ICompanyUrlScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly EFContexts _context;
        private readonly ILogger<CompanyUrlScraperService> _logger;
        private readonly string _azureConnectionString;

        public CompanyUrlScraperService(
            HttpClient httpClient,
            EFContexts context,
            IConfiguration configuration,
            ILogger<CompanyUrlScraperService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _azureConnectionString = configuration.GetConnectionString("AzureStorage")
                ?? "DefaultEndpointsProtocol=https;AccountName=hiresblob;AccountKey=XPt9HT6xcg2SEjcClC1aYbRoDR/PO/Lv8f0IB9TgjTbXEAO286ChXwGFNMlYMNvoE3TCcNaXYIHP+AStJ0IZ5A==;EndpointSuffix=core.windows.net";
        }

        public async Task<CompanyUrlScrapedDto> ScrapeCompanyUrlAsync(string rawUrl)
        {
            var result = new CompanyUrlScrapedDto
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
                // 1. Normalize URL
                string normalizedUrl = NormalizeUrl(result.InputUrl);
                result.NormalizedWebsiteUrl = normalizedUrl;
                string domain = ExtractDomain(normalizedUrl);
                result.DomainName = domain;

                if (string.IsNullOrEmpty(domain))
                {
                    result.Status = "Failed";
                    result.StatusMessage = "Invalid website domain or URL format.";
                    return result;
                }

                // Default fallback name from domain
                string fallbackName = HumanizeDomain(domain);
                result.CompanyName = fallbackName;

                // 2. Fetch HTML content from the company website
                string html = string.Empty;
                Uri baseUri = new Uri(normalizedUrl);

                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15));
                    using var request = new HttpRequestMessage(HttpMethod.Get, normalizedUrl);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
                    request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                    request.Headers.Add("Upgrade-Insecure-Requests", "1");

                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        html = await response.Content.ReadAsStringAsync();
                        // Update base URI in case of redirects
                        if (response.RequestMessage?.RequestUri != null)
                        {
                            baseUri = response.RequestMessage.RequestUri;
                            result.NormalizedWebsiteUrl = baseUri.GetLeftPart(UriPartial.Authority);
                            result.DomainName = ExtractDomain(result.NormalizedWebsiteUrl);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("HTTP {StatusCode} when fetching company URL: {Url}", response.StatusCode, normalizedUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Direct fetch failed for company URL: {Url}. Using fallback extraction.", normalizedUrl);
                }

                // 3. Extract Company Name, LinkedIn URL, and Logo from HTML
                if (!string.IsNullOrEmpty(html))
                {
                    // A. Parse JSON-LD schema
                    ParseJsonLd(html, result, baseUri);

                    // B. Parse OpenGraph and Meta tags
                    ParseMetaTags(html, result);

                    // C. Parse HTML <title> tag if Company Name is still domain fallback or empty
                    if (string.IsNullOrWhiteSpace(result.CompanyName) || result.CompanyName.Equals(fallbackName, StringComparison.OrdinalIgnoreCase))
                    {
                        string parsedTitleName = ExtractNameFromTitle(html, domain);
                        if (!string.IsNullOrWhiteSpace(parsedTitleName))
                        {
                            result.CompanyName = parsedTitleName;
                        }
                    }

                    // D. Parse LinkedIn URL from HTML anchors / social links
                    if (string.IsNullOrWhiteSpace(result.LinkedinUrl))
                    {
                        result.LinkedinUrl = ExtractLinkedInUrl(html);
                    }

                    // E. Parse Favicon / Icon from HTML if no logo yet
                    if (string.IsNullOrWhiteSpace(result.OriginalLogoUrl))
                    {
                        result.OriginalLogoUrl = ExtractFaviconOrLogo(html, baseUri);
                    }
                }

                // Final safety for company name
                if (string.IsNullOrWhiteSpace(result.CompanyName))
                {
                    result.CompanyName = fallbackName;
                }

                // 4. Save/Upload Logo to Azure Blob Storage
                string azureLogoFile = await SaveLogoToAzureBlobAsync(result.OriginalLogoUrl, result.DomainName, result.CompanyName);
                result.AzureLogoFileName = azureLogoFile;

                result.Status = "Completed";
                result.StatusMessage = "Successfully extracted company details.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping company URL: {Url}", rawUrl);
                result.Status = "Failed";
                result.StatusMessage = ex.Message;
            }

            return result;
        }

        public async Task<List<CompanyUrlScrapedDto>> ScrapeAndSaveCompanyUrlsAsync(List<string> urls, bool autoSaveToDb, long? adminUserId)
        {
            var results = new List<CompanyUrlScrapedDto>();
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
                var dto = await ScrapeCompanyUrlAsync(url);

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
                        _logger.LogError(ex, "Failed to save company URL to DB: {CompanyName} ({Url})", dto.CompanyName, dto.InputUrl);
                        dto.Status = "Failed";
                        dto.StatusMessage = $"DB Save error: {ex.Message}";
                    }
                }

                results.Add(dto);
            }

            return results;
        }

        private async Task<Consultancy> SaveOrUpdateConsultancyAsync(CompanyUrlScrapedDto dto, long? adminUserId)
        {
            var now = DateTime.UtcNow;
            string domain = Truncate(dto.DomainName?.Trim().ToLowerInvariant(), 255);
            string linkedin = Truncate(dto.LinkedinUrl?.Trim(), 255);
            string name = Truncate(dto.CompanyName?.Trim(), 50);
            string website = Truncate(dto.NormalizedWebsiteUrl?.Trim(), 255);
            string logo = Truncate(dto.AzureLogoFileName?.Trim(), 255);

            // Update DTO with clean truncated values
            dto.CompanyName = name;
            dto.DomainName = domain;
            dto.NormalizedWebsiteUrl = website;
            dto.LinkedinUrl = string.IsNullOrEmpty(linkedin) ? null : linkedin;

            // Search for existing record by domain, linkedin, or exact name
            Consultancy? existing = null;

            if (!string.IsNullOrEmpty(domain))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    c.Domainname == domain || 
                    (c.Website != null && c.Website.ToLower().Contains(domain)));
            }

            if (existing == null && !string.IsNullOrEmpty(linkedin))
            {
                var slugMatch = Regex.Match(linkedin, @"linkedin\.com\/(?:company|school)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
                string slug = slugMatch.Success ? slugMatch.Groups[1].Value : linkedin;
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    c.Linkedin != null && c.Linkedin.ToLower().Contains(slug.ToLower()));
            }

            if (existing == null && !string.IsNullOrEmpty(name))
            {
                existing = await _context.Consultancies.FirstOrDefaultAsync(c => 
                    c.Name != null && c.Name.ToLower() == name.ToLower());
            }

            if (existing != null)
            {
                // Update existing record with newly discovered information if missing
                if (!string.IsNullOrWhiteSpace(website) && string.IsNullOrWhiteSpace(existing.Website))
                {
                    existing.Website = website;
                }
                if (!string.IsNullOrWhiteSpace(domain) && string.IsNullOrWhiteSpace(existing.Domainname))
                {
                    existing.Domainname = domain;
                }
                if (!string.IsNullOrWhiteSpace(linkedin) && (string.IsNullOrWhiteSpace(existing.Linkedin) || existing.Linkedin.Length < 10))
                {
                    existing.Linkedin = linkedin;
                }
                if (!string.IsNullOrWhiteSpace(logo) && string.IsNullOrWhiteSpace(existing.Logo))
                {
                    existing.Logo = logo;
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
                    Name = name,
                    Website = website,
                    Domainname = domain,
                    Linkedin = string.IsNullOrEmpty(linkedin) ? null : linkedin,
                    Logo = string.IsNullOrEmpty(logo) ? null : logo,
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

        private static string Truncate(string? val, int maxLen)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            return val.Length <= maxLen ? val : val.Substring(0, maxLen);
        }

        private void ParseJsonLd(string html, CompanyUrlScrapedDto result, Uri baseUri)
        {
            var jsonLdMatches = Regex.Matches(html, @"<script\s+type=[""']application\/ld\+json[""'][^>]*>(.*?)<\/script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match m in jsonLdMatches)
            {
                try
                {
                    string jsonContent = m.Groups[1].Value.Trim();
                    if (string.IsNullOrEmpty(jsonContent)) continue;

                    JToken token;
                    if (jsonContent.StartsWith("["))
                    {
                        token = JArray.Parse(jsonContent);
                    }
                    else if (jsonContent.StartsWith("{"))
                    {
                        token = JObject.Parse(jsonContent);
                    }
                    else
                    {
                        continue;
                    }

                    ExtractFromJsonLdToken(token, result, baseUri);
                }
                catch { }
            }
        }

        private void ExtractFromJsonLdToken(JToken token, CompanyUrlScrapedDto result, Uri baseUri)
        {
            if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    ExtractFromJsonLdToken(item, result, baseUri);
                }
                return;
            }

            if (token is JObject obj)
            {
                string? type = obj["@type"]?.ToString();
                
                // If it contains @graph
                if (obj["@graph"] is JArray graph)
                {
                    foreach (var gItem in graph)
                    {
                        ExtractFromJsonLdToken(gItem, result, baseUri);
                    }
                }

                // Check Organization, Corporation, WebSite, LocalBusiness
                bool isOrg = type != null && (
                    type.Equals("Organization", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("Corporation", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("Company", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("LocalBusiness", StringComparison.OrdinalIgnoreCase) ||
                    type.Equals("WebSite", StringComparison.OrdinalIgnoreCase));

                if (isOrg || string.IsNullOrWhiteSpace(result.CompanyName))
                {
                    string? name = obj["name"]?.ToString() ?? obj["legalName"]?.ToString() ?? obj["alternateName"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && !name.Equals("Home", StringComparison.OrdinalIgnoreCase))
                    {
                        result.CompanyName = CleanCompanyName(name);
                    }
                }

                // Extract LinkedIn from sameAs
                if (string.IsNullOrWhiteSpace(result.LinkedinUrl) && obj["sameAs"] != null)
                {
                    var sameAsToken = obj["sameAs"];
                    if (sameAsToken is JArray sameAsArr)
                    {
                        foreach (var s in sameAsArr)
                        {
                            string sUrl = s.ToString();
                            if (sUrl.Contains("linkedin.com/company", StringComparison.OrdinalIgnoreCase) || sUrl.Contains("linkedin.com/school", StringComparison.OrdinalIgnoreCase))
                            {
                                result.LinkedinUrl = NormalizeLinkedInUrl(sUrl);
                                break;
                            }
                        }
                    }
                    else
                    {
                        string sUrl = sameAsToken.ToString();
                        if (sUrl.Contains("linkedin.com/company", StringComparison.OrdinalIgnoreCase) || sUrl.Contains("linkedin.com/school", StringComparison.OrdinalIgnoreCase))
                        {
                            result.LinkedinUrl = NormalizeLinkedInUrl(sUrl);
                        }
                    }
                }

                // Extract Logo
                if (string.IsNullOrWhiteSpace(result.OriginalLogoUrl) && obj["logo"] != null)
                {
                    var logoToken = obj["logo"];
                    string? logoUrl = logoToken is JObject lo ? lo["url"]?.ToString() : logoToken?.ToString();
                    if (!string.IsNullOrWhiteSpace(logoUrl))
                    {
                        result.OriginalLogoUrl = ResolveAbsoluteUrl(logoUrl, baseUri);
                    }
                }

                // Description
                if (string.IsNullOrWhiteSpace(result.Description) && obj["description"] != null)
                {
                    result.Description = obj["description"]?.ToString();
                }
            }
        }

        private void ParseMetaTags(string html, CompanyUrlScrapedDto result)
        {
            // og:site_name
            var siteNameMatch = Regex.Match(html, @"<meta\s+property=[""']og:site_name[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            if (!siteNameMatch.Success)
            {
                siteNameMatch = Regex.Match(html, @"<meta\s+content=[""']([^""']+)[""']\s+property=[""']og:site_name[""']", RegexOptions.IgnoreCase);
            }
            if (siteNameMatch.Success)
            {
                string siteName = WebUtility.HtmlDecode(siteNameMatch.Groups[1].Value).Trim();
                if (!string.IsNullOrWhiteSpace(siteName))
                {
                    result.CompanyName = CleanCompanyName(siteName);
                }
            }

            // og:title
            if (string.IsNullOrWhiteSpace(result.CompanyName))
            {
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
            }

            // og:image
            if (string.IsNullOrWhiteSpace(result.OriginalLogoUrl))
            {
                var ogImageMatch = Regex.Match(html, @"<meta\s+property=[""']og:image[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (!ogImageMatch.Success)
                {
                    ogImageMatch = Regex.Match(html, @"<meta\s+content=[""']([^""']+)[""']\s+property=[""']og:image[""']", RegexOptions.IgnoreCase);
                }
                if (ogImageMatch.Success)
                {
                    result.OriginalLogoUrl = WebUtility.HtmlDecode(ogImageMatch.Groups[1].Value);
                }
            }

            // Description
            if (string.IsNullOrWhiteSpace(result.Description))
            {
                var ogDescMatch = Regex.Match(html, @"<meta\s+(?:property=[""']og:description[""']|name=[""']description[""'])\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (ogDescMatch.Success)
                {
                    result.Description = WebUtility.HtmlDecode(ogDescMatch.Groups[1].Value);
                }
            }
        }

        private string ExtractNameFromTitle(string html, string domain)
        {
            var titleMatch = Regex.Match(html, @"<title[^>]*>(.*?)<\/title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!titleMatch.Success) return string.Empty;

            string title = WebUtility.HtmlDecode(titleMatch.Groups[1].Value).Trim();
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            // Split by standard separators: |, -, —, –, •, :, », : 
            var parts = Regex.Split(title, @"\s+[\|\-—–•:»]\s+|\s+[\|\-—–•:»]$|^[\|\-—–•:»]\s+|:\s+");
            var cleanParts = parts.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            if (!cleanParts.Any()) return CleanCompanyName(title);

            // Look for a part that closely matches or mentions the domain or is the shortest brand token
            string cleanDomainWord = domain.Split('.').FirstOrDefault() ?? string.Empty;

            foreach (var part in cleanParts)
            {
                if (part.IndexOf(cleanDomainWord, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return CleanCompanyName(part);
                }
            }

            // Otherwise take the first or shortest token
            string bestPart = cleanParts.OrderBy(p => p.Length).First();
            return CleanCompanyName(bestPart);
        }

        private string? ExtractLinkedInUrl(string html)
        {
            // Search for LinkedIn company or school links in anchor tags or raw text
            var matches = Regex.Matches(html, @"https?:\/\/(?:[a-zA-Z0-9-]+\.)?linkedin\.com\/(?:company|school)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                if (m.Success)
                {
                    string slug = m.Groups[1].Value.Trim().TrimEnd('/', '?', '#');
                    if (!string.IsNullOrWhiteSpace(slug) && !slug.Equals("share", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"https://www.linkedin.com/company/{slug}";
                    }
                }
            }

            return null;
        }

        private string? ExtractFaviconOrLogo(string html, Uri baseUri)
        {
            // Check apple-touch-icon or icon
            var iconMatch = Regex.Match(html, @"<link\s+[^>]*rel=[""'](?:shortcut\s+icon|icon|apple-touch-icon)[""'][^>]*href=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            if (!iconMatch.Success)
            {
                iconMatch = Regex.Match(html, @"<link\s+[^>]*href=[""']([^""']+)[""'][^>]*rel=[""'](?:shortcut\s+icon|icon|apple-touch-icon)[""']", RegexOptions.IgnoreCase);
            }

            if (iconMatch.Success)
            {
                string href = WebUtility.HtmlDecode(iconMatch.Groups[1].Value);
                return ResolveAbsoluteUrl(href, baseUri);
            }

            return null;
        }

        private string ResolveAbsoluteUrl(string relativeOrAbsolute, Uri baseUri)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolute)) return string.Empty;
            if (relativeOrAbsolute.StartsWith("//"))
            {
                return "https:" + relativeOrAbsolute;
            }
            if (Uri.TryCreate(baseUri, relativeOrAbsolute, out Uri? absoluteUri))
            {
                return absoluteUri.ToString();
            }
            return relativeOrAbsolute;
        }

        private async Task<string> SaveLogoToAzureBlobAsync(string? originalLogoUrl, string? domain, string? companyName)
        {
            string fileName = CleanFileName(domain, companyName) + ".png";
            byte[]? imageBytes = null;

            // 1. Try downloading original logo URL if present
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

            // 2. Fallback to Clearbit logo
            if ((imageBytes == null || imageBytes.Length == 0) && !string.IsNullOrWhiteSpace(domain))
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

            // 3. Fallback to Google Favicon service
            if ((imageBytes == null || imageBytes.Length == 0) && !string.IsNullOrWhiteSpace(domain))
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

            // 4. Upload to Azure Blob Storage container "profilepic"
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

        private string NormalizeUrl(string rawUrl)
        {
            string url = rawUrl.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }
            return url;
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

        private string HumanizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return "Company";
            string namePart = domain.Split('.').FirstOrDefault() ?? domain;
            namePart = namePart.Replace("-", " ").Replace("_", " ");
            var words = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
                }
            }
            return string.Join(" ", words);
        }

        private string NormalizeLinkedInUrl(string raw)
        {
            var match = Regex.Match(raw, @"linkedin\.com\/(?:company|school)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"https://www.linkedin.com/company/{match.Groups[1].Value.TrimEnd('/', '?', '#')}";
            }
            return raw;
        }

        private string CleanCompanyName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;
            string cleaned = rawName.Trim();
            // Remove common SEO suffixes
            cleaned = Regex.Replace(cleaned, @"\s*\|\s*Home.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\s*\|\s*Official Site.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\s*-\s*Official Website.*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\s*-\s*Home.*$", "", RegexOptions.IgnoreCase);
            if (cleaned.Contains(":") && cleaned.Length > 25)
            {
                var beforeColon = cleaned.Split(':')[0].Trim();
                if (beforeColon.Length >= 2) cleaned = beforeColon;
            }
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
