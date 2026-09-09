using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateProfileViewHistory
{
    public long Id { get; set; }

    public long? CandidateProfileid { get; set; }

    public long? ConsultancyUserId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual ConsultancyUser? ConsultancyUser { get; set; }
}
