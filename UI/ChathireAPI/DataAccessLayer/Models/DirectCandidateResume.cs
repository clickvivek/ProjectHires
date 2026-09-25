using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class DirectCandidateResume
{
    public long Id { get; set; }

    public long CandidateUserId { get; set; }

    public string FileName { get; set; } = null!;

    public string BlobUrl { get; set; } = null!;

    public int? FileSizeInKb { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime UploadedDate { get; set; }

    public virtual User CandidateUser { get; set; } = null!;
}
