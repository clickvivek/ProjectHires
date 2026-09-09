using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningEmploymentType
{
    public long Id { get; set; }

    public long? JobOpeningId { get; set; }

    public short? EmploymentTypeId { get; set; }

    public bool? Active { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual EmploymentType? EmploymentType { get; set; }

    public virtual JobOpening? JobOpening { get; set; }
}
