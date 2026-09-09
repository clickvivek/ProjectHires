using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidatePrefLocation
{
    public long Id { get; set; }

    public long CandidateId { get; set; }

    public int CityId { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual CandidateProfile Candidate { get; set; } = null!;

    public virtual City City { get; set; } = null!;
}
