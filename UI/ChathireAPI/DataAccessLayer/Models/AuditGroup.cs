using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class AuditGroup
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<Audit> Audits { get; } = new List<Audit>();
}
