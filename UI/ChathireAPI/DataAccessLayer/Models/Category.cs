using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<JobOpening> JobOpenings { get; } = new List<JobOpening>();
}
