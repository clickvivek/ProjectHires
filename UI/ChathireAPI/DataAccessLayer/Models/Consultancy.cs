using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Consultancy
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public string? Website { get; set; }

    public string? Linkedin { get; set; }

    public string? Logo { get; set; }

    public int? CityId { get; set; }

    public string? Domainname { get; set; }

    public long? UpdatedBy { get; set; }

    public bool? IsDirectCompany { get; set; }

    public short? StatusId { get; set; }

    public virtual City? City { get; set; }

    public virtual ICollection<ConsultancyUser> ConsultancyUsers { get; } = new List<ConsultancyUser>();

    public virtual ConsultancyStatus? Status { get; set; }
}
