using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningProfileConsultancyComment
{
    public long Id { get; set; }

    public long JobopeningCandidateProfileMapId { get; set; }

    public long ConsultancyUserId { get; set; }

    public string? Comment { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ConsultancyUser ConsultancyUser { get; set; } = null!;

    public virtual JobOpeningCandidateProfileMap JobopeningCandidateProfileMap { get; set; } = null!;
}
