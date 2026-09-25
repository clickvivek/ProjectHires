using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserReferral
{
    public long Id { get; set; }

    public long ReferrerUserId { get; set; }

    public string ReferredEmail { get; set; } = null!;

    public string ReferralCode { get; set; } = null!;

    public string Status { get; set; } = "Invited";

    public long? ReferredUserId { get; set; }

    public bool RewardClaimed { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? RegisteredDate { get; set; }

    public DateTime? RewardGrantedDate { get; set; }

    public DateTime Updated { get; set; }
}
