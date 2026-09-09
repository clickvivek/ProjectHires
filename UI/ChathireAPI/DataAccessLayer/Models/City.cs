using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class City
{
    public int Id { get; set; }

    public int IdState { get; set; }

    public string City1 { get; set; } = null!;

    public string? Zip { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public bool? IsState { get; set; }

    public virtual ICollection<CandidatePrefLocation> CandidatePrefLocations { get; } = new List<CandidatePrefLocation>();

    public virtual ICollection<CandidateProfile> CandidateProfiles { get; } = new List<CandidateProfile>();

    public virtual ICollection<Consultancy> Consultancies { get; } = new List<Consultancy>();

    public virtual State IdStateNavigation { get; set; } = null!;

    public virtual ICollection<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMap>();

    public virtual ICollection<JobOpeningLocation> JobOpeningLocations { get; } = new List<JobOpeningLocation>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
