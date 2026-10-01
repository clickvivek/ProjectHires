using System;

namespace DataAccessLayer.Models
{
    public partial class Config
    {
        public long Id { get; set; }
        public string ConfigKey { get; set; } = null!;
        public string ConfigValue { get; set; } = null!;
        public string? Description { get; set; }
        public bool? Active { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public DateTime? Updated { get; set; }
        public long? UpdatedBy { get; set; }
    }
}
