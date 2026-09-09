using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class State
{
    public int Id { get; set; }

    public string StateCode { get; set; } = null!;

    public string StateName { get; set; } = null!;

    public string? CountryCode { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual ICollection<City> Cities { get; } = new List<City>();

    public virtual Country? CountryCodeNavigation { get; set; }
}
