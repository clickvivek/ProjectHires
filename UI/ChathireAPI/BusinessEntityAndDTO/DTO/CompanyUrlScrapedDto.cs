using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class CompanyUrlScrapeRequestDto
    {
        public List<string> Urls { get; set; } = new List<string>();
        public bool AutoSaveToDb { get; set; } = true;
    }

    public class CompanyUrlScrapedDto
    {
        public string InputUrl { get; set; } = string.Empty;
        public string? NormalizedWebsiteUrl { get; set; }
        public string? DomainName { get; set; }
        public string? CompanyName { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? OriginalLogoUrl { get; set; }
        public string? AzureLogoFileName { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending"; // "Completed", "Failed"
        public string? StatusMessage { get; set; } // "Created new company", "Updated existing company", or error details
        public long? ConsultancyId { get; set; }
        public bool IsNewRecord { get; set; }
    }
}
