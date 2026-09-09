using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ConsultancyUser
{
    public long Id { get; set; }

    public long? ConsultancyId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public string? PublicProfileUserName { get; set; }

    public long UserId { get; set; }

    public int? NoOfViews { get; set; }

    public long? ParentId { get; set; }

    public int? NoOfPosting { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<CandidateProfileViewHistory> CandidateProfileViewHistories { get; } = new List<CandidateProfileViewHistory>();

    public virtual ICollection<CandidateProfile> CandidateProfiles { get; } = new List<CandidateProfile>();

    public virtual Consultancy? Consultancy { get; set; }

    public virtual ICollection<ConsultancyUser> InverseParent { get; } = new List<ConsultancyUser>();

    public virtual ICollection<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMap>();

    public virtual ICollection<JobOpeningProfileConsultancyComment> JobOpeningProfileConsultancyComments { get; } = new List<JobOpeningProfileConsultancyComment>();

    public virtual ICollection<JobOpening> JobOpenings { get; } = new List<JobOpening>();

    public virtual ConsultancyUser? Parent { get; set; }

    public virtual User User { get; set; } = null!;
}
