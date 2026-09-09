using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class SubscriptionPlanFeature
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public long? SubscriptionPlanId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
}
