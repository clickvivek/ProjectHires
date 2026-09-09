using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Bofunction
{
    public long Id { get; set; }

    public string? Description { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public string? Name { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<UserAccess> UserAccesses { get; } = new List<UserAccess>();
}
