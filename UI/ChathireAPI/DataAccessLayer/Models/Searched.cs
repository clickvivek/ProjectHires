using System;

namespace DataAccessLayer.Models
{
    public partial class Searched
    {
        public long Id { get; set; }
        public string SearchType { get; set; } = null!; // "JobSearch" or "HotlistSearch"
        public string? Keywords { get; set; }
        public string? Location { get; set; }
        public string? Filters { get; set; }
        public int TotalResults { get; set; }
        public long? UserId { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; } = true;
    }
}
