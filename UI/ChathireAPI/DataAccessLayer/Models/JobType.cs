using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobType
{
    public short Id { get; set; }

    public string Description { get; set; } = null!;

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<CandidatePrefJobType> CandidatePrefJobTypes { get; } = new List<CandidatePrefJobType>();

    public virtual ICollection<JobOpeningJobType> JobOpeningJobTypes { get; } = new List<JobOpeningJobType>();
}
