using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class UserSubscriptionPlanDto
    {
        public long Id { get; set; }

        public long? UserId { get; set; }

        public long? SubscriptionPlanId { get; set; }

        public long? PromocodeId { get; set; }

        public int? NoOfUsedDownloads { get; set; }

        public int? NoOfUsedJobPosting { get; set; }

        public int? ActualDownloads { get; set; }

        public int? ActualJobPosting { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal? Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int? NoOfUsers { get; set; }

        public bool? IsFree { get; set; }

        public bool? Active { get; set; }

        public SubscriptionPlanDto? SubscriptionPlan { get; set; } = new SubscriptionPlanDto();
        
        public UserDto? User { get; set; } = new UserDto();
    }

    public partial class AssignSubscriptionDto
    {
        public long UserId { get; set; }

        public long SubscriptionPlanId { get; set; }
    }

}
