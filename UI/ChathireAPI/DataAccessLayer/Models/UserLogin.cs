using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserLogin
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public DateTime? LoginTime { get; set; }

    public string? IpAddress { get; set; }

    public string? Location { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual User? User { get; set; }
}
