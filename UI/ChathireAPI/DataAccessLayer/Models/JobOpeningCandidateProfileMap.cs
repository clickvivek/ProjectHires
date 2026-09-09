using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningCandidateProfileMap
{
    public long Id { get; set; }

    public long JobOpeningId { get; set; }

    public long? CandidateProfileId { get; set; }

    public DateTime? AppliedDate { get; set; }

    public bool? Active { get; set; }

    public short CandidateProfileMappingStatusId { get; set; }

    public string? Comment { get; set; }

    public long? UpdatedBy { get; set; }

    public long? ConsultancyUserId { get; set; }

    public long? CandidateUserId { get; set; }

    public DateTime? Updated { get; set; }

    public string? Doc { get; set; }

    public string? CandidateName { get; set; }

    public int? TotalYearsOfExp { get; set; }

    public string? LinkedIn { get; set; }

    public int? CurrentLocationCityid { get; set; }

    public bool IsRead { get; set; }

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual CandidateProfileMappingStatus CandidateProfileMappingStatus { get; set; } = null!;

    public virtual User? CandidateUser { get; set; }

    public virtual ConsultancyUser? ConsultancyUser { get; set; }

    public virtual City? CurrentLocationCity { get; set; }

    public virtual JobOpening JobOpening { get; set; } = null!;

    public virtual ICollection<JobOpeningProfileConsultancyComment> JobOpeningProfileConsultancyComments { get; } = new List<JobOpeningProfileConsultancyComment>();
}
