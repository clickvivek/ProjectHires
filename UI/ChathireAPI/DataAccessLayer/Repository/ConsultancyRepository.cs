using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IConsultancyRepository : IRepository<Consultancy, long>
    {
        Task<List<Consultancy>> SearchConsultancies(string conName);
        Task<Consultancy?> FindExistingDuplicateAsync(string? domain, string? website, string? linkedin, string? name);
    }
    public class ConsultancyRepository : BaseRepository<Consultancy, long>, IConsultancyRepository
    {
        public ConsultancyRepository(EFContexts context) : base(context) { }


        public async Task<List<Consultancy>> SearchConsultancies(string conName)
        {
            if (string.IsNullOrWhiteSpace(conName))
            {
                return new List<Consultancy>();
            }

            var clean = conName.Trim();
            var cleanNoProtocol = clean
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("www.", "", StringComparison.OrdinalIgnoreCase)
                .Trim('/', ' ', '\\');

            return await _context.Consultancies
                .Where(s => (s.Active == null || s.Active == true) &&
                            ((s.Name != null && (s.Name.Contains(clean) || s.Name.Contains(cleanNoProtocol))) ||
                             (s.Website != null && (s.Website.Contains(clean) || s.Website.Contains(cleanNoProtocol))) ||
                             (s.Website2 != null && (s.Website2.Contains(clean) || s.Website2.Contains(cleanNoProtocol))) ||
                             (s.Domainname != null && (s.Domainname.Contains(clean) || s.Domainname.Contains(cleanNoProtocol)))))
                .Take(50)
                .ToListAsync();
        }

        public async Task<Consultancy?> FindExistingDuplicateAsync(string? domain, string? website, string? linkedin, string? name)
        {
            string cleanDomain = ExtractNormalizedDomain(domain);
            if (string.IsNullOrEmpty(cleanDomain) && !string.IsNullOrEmpty(website))
            {
                cleanDomain = ExtractNormalizedDomain(website);
            }

            string cleanSlug = ExtractLinkedInSlug(linkedin);
            string cleanName = name?.Trim().ToLowerInvariant() ?? string.Empty;

            // 1. Check exact or substring domain match
            if (!string.IsNullOrEmpty(cleanDomain))
            {
                var match = await _context.Consultancies.FirstOrDefaultAsync(c =>
                    (c.Domainname != null && (c.Domainname.ToLower() == cleanDomain || c.Domainname.ToLower().Contains(cleanDomain) || cleanDomain.Contains(c.Domainname.ToLower()))) ||
                    (c.Website != null && c.Website.ToLower().Contains(cleanDomain)) ||
                    (c.Website2 != null && c.Website2.ToLower().Contains(cleanDomain)));
                
                if (match != null)
                {
                    return match;
                }
            }

            // 2. Check LinkedIn URL or slug match
            if (!string.IsNullOrEmpty(cleanSlug))
            {
                var match = await _context.Consultancies.FirstOrDefaultAsync(c =>
                    c.Linkedin != null && c.Linkedin.ToLower().Contains(cleanSlug));
                
                if (match != null)
                {
                    return match;
                }
            }

            // 3. Check exact company name match
            if (!string.IsNullOrEmpty(cleanName))
            {
                var match = await _context.Consultancies.FirstOrDefaultAsync(c =>
                    c.Name != null && c.Name.ToLower() == cleanName);
                
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        public static string ExtractNormalizedDomain(string? urlOrDomain)
        {
            if (string.IsNullOrWhiteSpace(urlOrDomain)) return string.Empty;
            try
            {
                string val = urlOrDomain.Trim();
                if (val.Contains('@'))
                {
                    val = val.Split('@')[1];
                }
                if (!val.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !val.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    val = "https://" + val;
                }
                var uri = new Uri(val);
                string host = uri.Host.ToLowerInvariant();
                if (host.StartsWith("www.")) host = host.Substring(4);
                return host.Trim('/', ' ', '\\');
            }
            catch
            {
                string clean = urlOrDomain.ToLowerInvariant()
                    .Replace("https://", "")
                    .Replace("http://", "")
                    .Replace("www.", "")
                    .Trim('/', ' ', '\\');
                if (clean.Contains('/')) clean = clean.Split('/')[0];
                if (clean.Contains(':')) clean = clean.Split(':')[0];
                return clean;
            }
        }

        public static string ExtractLinkedInSlug(string? linkedinUrl)
        {
            if (string.IsNullOrWhiteSpace(linkedinUrl)) return string.Empty;
            var match = Regex.Match(linkedinUrl, @"linkedin\.com\/(?:company|school|in)\/([a-zA-Z0-9\-_%]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim().TrimEnd('/').ToLowerInvariant();
            }
            string clean = linkedinUrl.ToLowerInvariant()
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("www.", "")
                .Replace("linkedin.com/company/", "")
                .Replace("linkedin.com/school/", "")
                .Replace("linkedin.com/in/", "")
                .Trim('/', ' ', '\\');
            return clean;
        }

    }
}
