using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class EmailJobPostingQueueDto
    {
        public long Id { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string? EmailSubject { get; set; }
        public string? RawEmailBodyText { get; set; }
        public string? RawEmailBodyHtml { get; set; }
        public long? UserId { get; set; }
        public string? RecruiterName { get; set; }
        public string? CompanyName { get; set; }
        public long? ConsultancyId { get; set; }
        public string? MatchedEmailType { get; set; } // "Primary" or "Alternate"
        public string Status { get; set; } = "Received"; // Received, Parsed, Published, Failed, QuotaExceeded, Rejected
        public string? ParsedJobJson { get; set; }
        public ParsedJobDataDto? ParsedJob { get; set; }
        public ParsedHotlistDataDto? ParsedHotlist { get; set; }
        public long? CreatedJobOpeningId { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public long? UpdatedBy { get; set; }
    }

    public class ParsedJobDataDto
    {
        public string Name { get; set; } = string.Empty; // Job Title
        public string? Description { get; set; } // Clean HTML formatted description
        public string? JobLocation { get; set; }
        public bool? IsRemote { get; set; }
        public string? Country { get; set; } = "US";
        public string? Postalcode { get; set; }
        public int? TotalExp { get; set; }
        public short? FromAmt { get; set; }
        public short? ToAmt { get; set; }
        public short? NumberOfOpening { get; set; } = 1;
        public int? CategoryId { get; set; }
        public short? PriorityId { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> EmploymentTypes { get; set; } = new(); // e.g. Remote, Onsite, Hybrid
        public List<string> JobTypes { get; set; } = new(); // e.g. C2C, W2-Contract, Full-time
        public List<string> Visas { get; set; } = new(); // e.g. US Citizen, Green Card, H1B, Any
        public List<string> Locations { get; set; } = new();
    }

    public class InboundEmailWebhookDto
    {
        public string? Type { get; set; } // e.g. "email.received"
        public string? Created_At { get; set; }
        public ResendWebhookDataDto? Data { get; set; }

        public string? From { get; set; }
        public string? To { get; set; }
        public string? RecipientEmail { get; set; }
        public string? Subject { get; set; }
        public string? Text { get; set; }
        public string? Html { get; set; }
        public string? SenderName { get; set; }
        public string? Email_Id { get; set; }
    }

    public class ResendWebhookDataDto
    {
        public string? Email_Id { get; set; }
        public string? Id { get; set; }
        public string? From { get; set; }
        public object? To { get; set; }
        public List<string>? Received_For { get; set; }
        public string? Subject { get; set; }
        public string? Text { get; set; }
        public string? Html { get; set; }
    }

    public class SimulateInboundEmailDto
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string? RecipientEmail { get; set; } = "jobpostings@chathire.com";
        public bool AutoPublish { get; set; } = true;
    }

    public class ParsedHotlistCandidateDto
    {
        public string? CandidateName { get; set; }
        public string? Name { get => CandidateName; set => CandidateName = value; }
        public string Title { get; set; } = string.Empty;
        public int? TotalExp { get; set; }
        public string? Visa { get; set; }
        public short? VisaId { get; set; }
        public string? Location { get; set; }
        public int? CityId { get; set; }
        public bool? RemoteOnly { get; set; }
        public bool? CanRelocate { get; set; }
        public bool? AnyLocation { get; set; }
        public short? FromAmt { get; set; }
        public short? ToAmt { get; set; }
        public string? Comment { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> EmploymentTypes { get; set; } = new();
        public List<string> JobTypes { get; set; } = new();
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public long? CreatedCandidateProfileId { get; set; }
    }

    public class ParsedHotlistDataDto
    {
        public string? EmailSubject { get; set; }
        public string? RecruiterNotes { get; set; }
        public List<ParsedHotlistCandidateDto> Candidates { get; set; } = new();
    }

    public class SimulateInboundHotlistEmailDto
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string EmailSubject { get; set; } = "Hotlist of Available Consultants";
        public string EmailBody { get; set; } = string.Empty;
        public string? RecipientEmail { get; set; } = "posthotlist@chathire.com";
        public bool AutoPublish { get; set; } = true;
    }

    public class ApproveEmailJobPostingDto
    {
        public long QueueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? JobLocation { get; set; }
        public int? TotalExp { get; set; }
        public short? FromAmt { get; set; }
        public short? ToAmt { get; set; }
        public short? NumberOfOpening { get; set; } = 1;
        public List<string>? Skills { get; set; }
        public List<string>? EmploymentTypes { get; set; }
        public List<string>? JobTypes { get; set; }
        public List<string>? Visas { get; set; }
        public List<string>? Locations { get; set; }
    }

    public class RejectEmailJobPostingDto
    {
        public long QueueId { get; set; }
        public string? Reason { get; set; }
    }

    public class EmailJobPostingFilterDto
    {
        public string? Status { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class EmailJobPostingStatsDto
    {
        public int TotalReceived { get; set; }
        public int PublishedCount { get; set; }
        public int ParsedCount { get; set; }
        public int FailedCount { get; set; }
        public int QuotaExceededCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
