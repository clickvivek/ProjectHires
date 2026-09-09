using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class UserAccess
{
    public long Id { get; set; }

    public long UserTypeId { get; set; }

    public long BofunctionId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Bofunction Bofunction { get; set; } = null!;

    public virtual UserType UserType { get; set; } = null!;
}
