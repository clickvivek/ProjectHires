using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateProfile
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? ConsultancyUserId { get; set; }

    public string? CandidateName { get; set; }

    public string? Title { get; set; }

    public bool? Active { get; set; }

    public DateTime? PostedDate { get; set; }

    public bool? CanRelocate { get; set; }

    public int? TotalExp { get; set; }

    public DateTime? Updated { get; set; }

    public short StatusId { get; set; }

    public string? LinkedIn { get; set; }

    public string? Resume { get; set; }

    public short? Availability { get; set; }

    public short? VisaId { get; set; }

    public DateTime? VisaExpiryDate { get; set; }

    public string? Passportno { get; set; }

    public DateTime? FirstArrivalDate { get; set; }

    public bool? Backgroundchecked { get; set; }

    public int? CityId { get; set; }

    public bool? RemoteOnly { get; set; }

    public bool? AnyLocation { get; set; }

    public string? Comment { get; set; }

    public string? EmployerInfo { get; set; }

    public short? FromAmt { get; set; }

    public short? ToAmt { get; set; }

    public long? UpdatedBy { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public short? ConsultingRoleId { get; set; }

    public virtual CandidateAvailability? AvailabilityNavigation { get; set; }

    public virtual ICollection<CandidateDocument> CandidateDocuments { get; } = new List<CandidateDocument>();

    public virtual ICollection<CandidatePrefJobType> CandidatePrefJobTypes { get; } = new List<CandidatePrefJobType>();

    public virtual ICollection<CandidatePrefLocation> CandidatePrefLocations { get; } = new List<CandidatePrefLocation>();

    public virtual ICollection<CandidateProfileDomain> CandidateProfileDomains { get; } = new List<CandidateProfileDomain>();

    public virtual ICollection<CandidateProfileEmploymentType> CandidateProfileEmploymentTypes { get; } = new List<CandidateProfileEmploymentType>();

    public virtual ICollection<CandidateProfileSkill> CandidateProfileSkills { get; } = new List<CandidateProfileSkill>();

    public virtual ICollection<CandidateProfileViewHistory> CandidateProfileViewHistories { get; } = new List<CandidateProfileViewHistory>();

    public virtual City? City { get; set; }

    public virtual ConsultancyUser? ConsultancyUser { get; set; }

    public virtual CandidateConsultingRole? ConsultingRole { get; set; }

    public virtual ICollection<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMap>();

    public virtual ProfileStatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Visa? Visa { get; set; }
}
