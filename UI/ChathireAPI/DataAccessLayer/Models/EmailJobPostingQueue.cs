using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class EmailJobPostingQueue : BaseModel<long>
{
    public long Id { get; set; }

    public string SenderEmail { get; set; } = null!;

    public string? SenderName { get; set; }

    public string? EmailSubject { get; set; }

    public string? RawEmailBodyText { get; set; }

    public string? RawEmailBodyHtml { get; set; }

    public long? UserId { get; set; }

    public long? ConsultancyId { get; set; }

    public string? MatchedEmailType { get; set; }

    public string Status { get; set; } = "Received";

    public string? ParsedJobJson { get; set; }

    public long? CreatedJobOpeningId { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public DateTime ReceivedDate { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }
}
