using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningLocation
{
    public long Id { get; set; }

    public long JobOpeningId { get; set; }

    public int? NumberOfOpenings { get; set; }

    public DateTime? Updated { get; set; }

    public int? CityId { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual City? City { get; set; }

    public virtual JobOpening JobOpening { get; set; } = null!;
}
