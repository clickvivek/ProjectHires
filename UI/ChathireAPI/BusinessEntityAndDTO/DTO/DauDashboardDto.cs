using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class DauSummaryDto
    {
        public int DauCount { get; set; }
        public int WauCount { get; set; }
        public int MauCount { get; set; }
        public int ActiveNowCount { get; set; }
        public int TotalLoginsInPeriod { get; set; }
        public int TotalRegisteredUsers { get; set; }
    }

    public class DauTrendPointDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public string PeriodKey { get; set; } = string.Empty;
        public int UniqueUsers { get; set; }
        public int TotalLogins { get; set; }
    }

    public class UserActivityDrilldownDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime? LoginTime { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
    }

    public class DauDashboardDto
    {
        public DauSummaryDto Summary { get; set; } = new DauSummaryDto();
        public List<DauTrendPointDto> Trends { get; set; } = new List<DauTrendPointDto>();
        public List<UserActivityDrilldownDto> UserActivities { get; set; } = new List<UserActivityDrilldownDto>();
    }
}
