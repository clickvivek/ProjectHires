using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class SearchedDto
    {
        public long Id { get; set; }
        public string SearchType { get; set; } = string.Empty; // "JobSearch" or "HotlistSearch"
        public string? Keywords { get; set; }
        public string? Location { get; set; }
        public string? Filters { get; set; }
        public int TotalResults { get; set; }
        public long? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
    }

    public class SearchedAnalyticsSummaryDto
    {
        public int TotalSearches { get; set; }
        public int TotalJobSearches { get; set; }
        public int TotalHotlistSearches { get; set; }
        public int SearchesToday { get; set; }
        public int ZeroResultSearches { get; set; }
        public List<TopSearchKeywordDto> TopJobKeywords { get; set; } = new List<TopSearchKeywordDto>();
        public List<TopSearchKeywordDto> TopHotlistKeywords { get; set; } = new List<TopSearchKeywordDto>();
    }

    public class TopSearchKeywordDto
    {
        public string Keyword { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AvgResults { get; set; }
    }
}
