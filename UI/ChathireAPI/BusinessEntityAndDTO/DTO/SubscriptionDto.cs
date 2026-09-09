using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class SubscriptionPlanDto
    {
        public long Id { get; set; }

        public string? Description { get; set; }

        public int? NoOfDownloads { get; set; }

        public int? NoOfJobPosting { get; set; }

        public int? ValidityInDays { get; set; }

        public decimal? Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int? NoOfUsers { get; set; }

        public bool? IsFree { get; set; }

        public bool? Active { get; set; }
        public List<SubscriptionPlanFeatureDto>? SubscriptionPlanFeatures { get; set; } = new List<SubscriptionPlanFeatureDto>();
    }

    public partial class SubscriptionPlanFeatureDto
    {
        public long Id { get; set; }

        public string? Description { get; set; }

        public long? SubscriptionPlanId { get; set; }

        public bool? Active { get; set; }
    }
}
