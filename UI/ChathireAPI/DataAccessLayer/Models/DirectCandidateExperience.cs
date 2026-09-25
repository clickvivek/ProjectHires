using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class DirectCandidateExperience
{
    public long Id { get; set; }

    public long CandidateUserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int? CityId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User CandidateUser { get; set; } = null!;

    public virtual City? City { get; set; }
}
