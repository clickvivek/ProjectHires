using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateProfileDomain
{
    public long Id { get; set; }

    public long? CandidateProfileId { get; set; }

    public short? DomainId { get; set; }

    public bool? Active { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual Domain? Domain { get; set; }
}
