using System;

namespace BusinessEntityAndDTO.DTO
{
    public class UserQuotaStatusDto
    {
        public DateTime CycleStartDate { get; set; }
        public DateTime CycleEndDate { get; set; }
        public int DaysRemainingInCycle { get; set; }

        public int MaxJobPostings { get; set; }
        public int UsedJobPostings { get; set; }
        public int RemainingJobPostings { get; set; }

        public int MaxDownloads { get; set; }
        public int UsedDownloads { get; set; }
        public int RemainingDownloads { get; set; }

        public int DailyChatLimit { get; set; }
        public int UsedChatsToday { get; set; }
        public int RemainingChatsToday { get; set; }

        public bool IsFreeTier { get; set; }
        public string PlanName { get; set; } = "Free Plan";
        public bool IsLimitReached { get; set; }
        public double PostingsPercentage { get; set; }
        public double DownloadsPercentage { get; set; }
    }
}
