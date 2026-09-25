using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessEntityAndDTO.DTO
{
    public class SubmitReferralRequestDto
    {
        [Required(ErrorMessage = "At least 10 email addresses are required.")]
        public List<string> Emails { get; set; } = new List<string>();

        public string? CustomMessage { get; set; }
    }

    public class ReferralSkipDetailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class SubmitReferralResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalSubmitted { get; set; }
        public int SuccessfullyInvited { get; set; }
        public int AlreadyRegistered { get; set; }
        public int AlreadyInvited { get; set; }
        public int InvalidEmails { get; set; }
        public List<string> InvitedEmails { get; set; } = new List<string>();
        public List<ReferralSkipDetailDto> SkippedDetails { get; set; } = new List<ReferralSkipDetailDto>();
    }

    public class UserReferralDto
    {
        public long Id { get; set; }
        public string ReferredEmail { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Invited";
        public long? ReferredUserId { get; set; }
        public bool RewardClaimed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public DateTime? RewardGrantedDate { get; set; }
    }

    public class ReferralStatsDto
    {
        public int TotalInvited { get; set; }
        public int TotalRegistered { get; set; }
        public int FreePostingsEarned { get; set; }
        public int FreeMonthsEarned { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
        public string ReferralLink { get; set; } = string.Empty;
        public List<UserReferralDto> Referrals { get; set; } = new List<UserReferralDto>();
    }

    public class ProcessSignupReferralDto
    {
        public string Email { get; set; } = string.Empty;
        public string? ReferralCode { get; set; }
        public long NewUserId { get; set; }
    }
}
