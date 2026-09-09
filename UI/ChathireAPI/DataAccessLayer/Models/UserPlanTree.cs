using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserPlanTree
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public long? UserId { get; set; }

    public long? AssignedUserId { get; set; }

    public int? AssignedDownloads { get; set; }

    public int? AssignedPosting { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual User? AssignedUser { get; set; }

    public virtual User? User { get; set; }
}
