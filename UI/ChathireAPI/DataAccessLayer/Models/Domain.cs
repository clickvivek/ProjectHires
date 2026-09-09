using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Domain
{
    public short Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<CandidateProfileDomain> CandidateProfileDomains { get; } = new List<CandidateProfileDomain>();
}
