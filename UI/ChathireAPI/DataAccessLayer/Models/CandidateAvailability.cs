using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateAvailability
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? Updated { get; set; }

    public long UpdatedBy { get; set; }

    public virtual ICollection<CandidateProfile> CandidateProfiles { get; } = new List<CandidateProfile>();
}
