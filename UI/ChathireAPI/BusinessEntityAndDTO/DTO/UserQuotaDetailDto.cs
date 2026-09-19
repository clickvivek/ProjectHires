using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class UserQuotaDetailDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public long? UserTypeId { get; set; }
        public string RoleName { get; set; } = "";
        public bool RoleRecruiter { get; set; }
        public bool RoleBenchSales { get; set; }
        public DateTime? SignupDate { get; set; }

        public long? UserSubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = "Free Plan";
        public bool IsFree { get; set; } = true;
        public DateTime CycleStartDate { get; set; }
        public DateTime CycleEndDate { get; set; }
        public int DaysRemainingInCycle { get; set; }

        public int ActualJobPosting { get; set; }
        public int UsedJobPostings { get; set; }
        public int RemainingJobPostings { get; set; }

        public int ActualDownloads { get; set; }
        public int UsedDownloads { get; set; }
        public int RemainingDownloads { get; set; }

        public int DailyChatLimit { get; set; }
        public bool IsLimitReached { get; set; }
        public bool Active { get; set; } = true;
    }

    public class UserQuotaListResponseDto
    {
        public List<UserQuotaDetailDto> Items { get; set; } = new List<UserQuotaDetailDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateUserQuotaDto
    {
        public long UserId { get; set; }
        public long? UserSubscriptionPlanId { get; set; }
        public int ActualJobPosting { get; set; }
        public int ActualDownloads { get; set; }
        public int DailyChatLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFree { get; set; } = true;
        public string? PlanName { get; set; }
    }
}
