using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ProjectStartInWeek
{
    public short Id { get; set; }

    public string Description { get; set; } = null!;

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<JobOpening> JobOpenings { get; } = new List<JobOpening>();
}
