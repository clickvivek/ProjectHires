using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class SubscriptionPlan
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

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; } = new List<SubscriptionPlanFeature>();

    public virtual ICollection<UserSubscriptionPlan> UserSubscriptionPlans { get; } = new List<UserSubscriptionPlan>();
}
