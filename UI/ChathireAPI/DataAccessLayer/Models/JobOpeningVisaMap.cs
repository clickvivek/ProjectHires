using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningVisaMap
{
    public long Id { get; set; }

    public long JobOpeningId { get; set; }

    public short VisaId { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual JobOpening JobOpening { get; set; } = null!;

    public virtual Visa Visa { get; set; } = null!;
}
