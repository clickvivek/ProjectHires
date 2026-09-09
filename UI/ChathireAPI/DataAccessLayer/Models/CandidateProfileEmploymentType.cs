using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateProfileEmploymentType
{
    public long Id { get; set; }

    public long? CandidateProfileId { get; set; }

    public short? EmploymentTypeId { get; set; }

    public bool? Active { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual EmploymentType? EmploymentType { get; set; }
}
