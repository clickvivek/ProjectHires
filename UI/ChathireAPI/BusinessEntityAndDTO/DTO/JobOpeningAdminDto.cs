using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class JobOpeningAdminItemDto
    {
        public long Id { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public long? CompanyId { get; set; }
        public string? CompanyLogo { get; set; }
        public string? PostedByUserName { get; set; }
        public string? PostedByFullName { get; set; }
        public string? PostedByEmail { get; set; }
        public long? PostedByUserId { get; set; }
        public long? ConsultancyUserId { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? LastDate { get; set; }
        public DateTime? Updated { get; set; }
        public bool? Active { get; set; }
        public bool? IsExpired { get; set; }
        public string? JobLocation { get; set; }
        public string? Country { get; set; }
        public int? TotalExp { get; set; }
        public short? NumberOfOpening { get; set; }
        public string? SalaryDisplay { get; set; }
        public int ApplicationCount { get; set; }
        public string? Source { get; set; } = "PORTAL";
        public List<string> Skills { get; set; } = new List<string>();
    }

    public class JobOpeningAdminResponseDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<JobOpeningAdminItemDto> Items { get; set; } = new List<JobOpeningAdminItemDto>();
    }
}
