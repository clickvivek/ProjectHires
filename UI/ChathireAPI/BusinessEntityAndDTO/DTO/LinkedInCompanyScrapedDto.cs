using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class LinkedInScrapeRequestDto
    {
        public List<string> Urls { get; set; } = new List<string>();
        public bool AutoSaveToDb { get; set; } = true;
    }

    public class LinkedInCompanyScrapedDto
    {
        public string InputUrl { get; set; } = string.Empty;
        public string? NormalizedLinkedinUrl { get; set; }
        public string? CompanySlug { get; set; }
        public string? CompanyName { get; set; }
        public string? Website { get; set; }
        public string? DomainName { get; set; }
        public string? OriginalLogoUrl { get; set; }
        public string? AzureLogoFileName { get; set; }
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = "Pending"; // "Completed", "Updated", "Failed"
        public string? StatusMessage { get; set; }
        public long? ConsultancyId { get; set; }
        public bool IsNewRecord { get; set; }
    }
}
