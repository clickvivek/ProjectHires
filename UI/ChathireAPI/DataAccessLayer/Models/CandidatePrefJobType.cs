using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidatePrefJobType
{
    public long Id { get; set; }

    public long CandidateProfileId { get; set; }

    public short JobTypeId { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual CandidateProfile CandidateProfile { get; set; } = null!;

    public virtual JobType JobType { get; set; } = null!;
}
