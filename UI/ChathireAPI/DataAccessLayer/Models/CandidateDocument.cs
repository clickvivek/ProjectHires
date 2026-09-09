using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateDocument
{
    public long Id { get; set; }

    public short DocumentId { get; set; }

    public long CandidateProfileId { get; set; }

    public string? Doc { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual CandidateProfile CandidateProfile { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
