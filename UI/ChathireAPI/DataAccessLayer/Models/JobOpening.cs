using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpening
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Country { get; set; }

    public int? Joiningdays { get; set; }

    public string? Description { get; set; }

    public int? TotalExp { get; set; }

    public DateTime? PostedDate { get; set; }

    public DateTime? LastDate { get; set; }

    public short? NumberOfOpening { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? ConsultancyUserId { get; set; }

    public short? PriorityId { get; set; }

    public int? CategoryId { get; set; }

    public string? JobLocation { get; set; }

    public string? Postalcode { get; set; }

    public short? ProjectStartId { get; set; }

    public bool? DirectClient { get; set; }

    public bool? IsReviewed { get; set; }

    public short? FromAmt { get; set; }

    public short? ToAmt { get; set; }

    public bool? NotifyOnCandidateProfileMap { get; set; }

    public bool? NotifyWithResume { get; set; }

    public long? UpdatedBy { get; set; }

    public bool? LocalCandidatePref { get; set; }

    public bool? LocalCandidateOnly { get; set; }

    public long? ReviewedBy { get; set; }

    public bool? IsExpired { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ConsultancyUser? ConsultancyUser { get; set; }

    public virtual ICollection<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMap>();

    public virtual ICollection<JobOpeningEmploymentType> JobOpeningEmploymentTypes { get; } = new List<JobOpeningEmploymentType>();

    public virtual ICollection<JobOpeningJobType> JobOpeningJobTypes { get; } = new List<JobOpeningJobType>();

    public virtual ICollection<JobOpeningLocation> JobOpeningLocations { get; } = new List<JobOpeningLocation>();

    public virtual ICollection<JobOpeningSkill> JobOpeningSkills { get; } = new List<JobOpeningSkill>();

    public virtual ICollection<JobOpeningVisaMap> JobOpeningVisaMaps { get; } = new List<JobOpeningVisaMap>();

    public virtual ProjectStartInWeek? ProjectStart { get; set; }
}
