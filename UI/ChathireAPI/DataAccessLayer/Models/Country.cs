using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Country
{
    public string CountryCode { get; set; } = null!;

    public string? Name { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<State> States { get; } = new List<State>();
}
