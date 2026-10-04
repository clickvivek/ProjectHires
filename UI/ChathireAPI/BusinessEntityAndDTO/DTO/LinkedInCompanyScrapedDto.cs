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

    public class LinkedInBatchUpdateRequestDto
    {
        public int? Limit { get; set; } = 50;
        public int? Offset { get; set; } = 0;
        public int Concurrency { get; set; } = 3;
        public long? SpecificConsultancyId { get; set; }
    }

    public class LinkedInBatchUpdateResultDto
    {
        public int TotalScanned { get; set; }
        public int UpdatedCount { get; set; }
        public int UnchangedCount { get; set; }
        public int FailedCount { get; set; }
        public List<LinkedInUpdateItemDto> UpdatedItems { get; set; } = new List<LinkedInUpdateItemDto>();
    }

    public class LinkedInUpdateItemDto
    {
        public long ConsultancyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string LinkedinUrl { get; set; } = string.Empty;
        public string OldWebsite { get; set; } = string.Empty;
        public string NewWebsite { get; set; } = string.Empty;
        public string OldDomain { get; set; } = string.Empty;
        public string NewDomain { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
    }
}
