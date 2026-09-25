using System;

namespace BusinessEntityAndDTO.DTO
{
    public class DirectCandidateDetailDto
    {
        public long CandidateUserId { get; set; }
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public short? VisaId { get; set; }
        public string? VisaName { get; set; }
        public DateTime? VisaExpiryDate { get; set; }
        public decimal? TotalYearsOfExp { get; set; }
        public decimal? ExpectedAnnualSalary { get; set; }
        public decimal? ExpectedHourlyRate { get; set; }
        public string? PreferredWorkType { get; set; }
        public int? NoticePeriodDays { get; set; }
        public bool CanRelocate { get; set; }
        public bool RemoteOnly { get; set; }
        public bool IsActivelyLooking { get; set; }
        public string? GitHubUrl { get; set; }
        public bool IsPublicProfileEnabled { get; set; }
        public string? PublicProfileSlug { get; set; }
        public bool IsShowCompensationPublic { get; set; } = true;
        public string? PreferredLocations { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class DirectCandidateDetailDtoForUpdate
    {
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public short? VisaId { get; set; }
        public DateTime? VisaExpiryDate { get; set; }
        public decimal? TotalYearsOfExp { get; set; }
        public decimal? ExpectedAnnualSalary { get; set; }
        public decimal? ExpectedHourlyRate { get; set; }
        public string? PreferredWorkType { get; set; }
        public int? NoticePeriodDays { get; set; }
        public bool CanRelocate { get; set; }
        public bool RemoteOnly { get; set; }
        public bool IsActivelyLooking { get; set; }
        public string? GitHubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public bool IsPublicProfileEnabled { get; set; }
        public string? PublicProfileSlug { get; set; }
        public bool IsShowCompensationPublic { get; set; } = true;
        public string? PreferredLocations { get; set; }
    }
}
