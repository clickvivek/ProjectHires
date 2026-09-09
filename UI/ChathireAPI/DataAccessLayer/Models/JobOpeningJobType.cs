using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningJobType
{
    public long Id { get; set; }

    public long? JobOpeningId { get; set; }

    public short? JobTypeId { get; set; }

    public bool? Active { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual JobOpening? JobOpening { get; set; }

    public virtual JobType? JobType { get; set; }
}
