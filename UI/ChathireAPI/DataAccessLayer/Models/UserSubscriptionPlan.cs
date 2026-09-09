using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserSubscriptionPlan
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

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

    public virtual User? User { get; set; }
}
