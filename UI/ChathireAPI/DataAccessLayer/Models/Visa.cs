using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Visa
{
    public short Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<CandidateProfile> CandidateProfiles { get; } = new List<CandidateProfile>();

    public virtual ICollection<JobOpeningVisaMap> JobOpeningVisaMaps { get; } = new List<JobOpeningVisaMap>();
}
