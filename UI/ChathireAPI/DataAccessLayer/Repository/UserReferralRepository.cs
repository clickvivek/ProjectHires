using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IUserReferralRepository : IRepository<UserReferral, long>
    {
        Task<SubmitReferralResponseDto> SubmitReferrals(SubmitReferralRequestDto dto, UserContext userContext);
        Task<ReferralStatsDto> GetReferralStats(UserContext userContext);
        Task<bool> ProcessSignupReferral(string email, string referralCode, long newUserId);
    }

    public class UserReferralRepository : BaseRepository<UserReferral, long>, IUserReferralRepository
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public UserReferralRepository(EFContexts context) : base(context) { }

        public async Task<SubmitReferralResponseDto> SubmitReferrals(SubmitReferralRequestDto dto, UserContext userContext)
        {
            if (userContext == null || userContext.UserId <= 0)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = "User authentication required to submit referrals."
                };
            }

            var referrer = await _context.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId);
            if (referrer == null)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = "Referrer user account not found."
                };
            }

            string referrerName = $"{referrer.Fname} {referrer.Lname}".Trim();
            if (string.IsNullOrWhiteSpace(referrerName))
            {
                referrerName = referrer.UserName ?? "A colleague";
            }

            string referrerEmail = (referrer.Email ?? "").Trim().ToLower();
            string referralCode = $"REF{referrer.Id}";

            if (dto.Emails == null || dto.Emails.Count == 0)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = "Please provide at least 10 email addresses."
                };
            }

            var rawEmails = dto.Emails
                .SelectMany(e => (e ?? "").Split(new[] { ',', ';', '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(e => e.Trim().ToLower())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            if (rawEmails.Count < 10)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = $"Minimum 10 email addresses are required to submit referrals. You provided {rawEmails.Count} valid email(s)."
                };
            }

            var seenInBatch = new HashSet<string>();
            var validEmailsToProcess = new List<string>();
            var skippedDetails = new List<ReferralSkipDetailDto>();

            int invalidCount = 0;
            int alreadyRegisteredCount = 0;
            int alreadyInvitedCount = 0;

            // 1. Client & Intra-batch Validation
            foreach (var email in rawEmails)
            {
                if (!EmailRegex.IsMatch(email))
                {
                    invalidCount++;
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = "Invalid email format"
                    });
                    continue;
                }

                if (email == referrerEmail)
                {
                    invalidCount++;
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = "Cannot refer your own email address"
                    });
                    continue;
                }

                if (!seenInBatch.Add(email))
                {
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = "Duplicate email in submitted list"
                    });
                    continue;
                }

                validEmailsToProcess.Add(email);
            }

            if (validEmailsToProcess.Count == 0)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = "No valid unique email addresses found in the submitted list.",
                    TotalSubmitted = rawEmails.Count,
                    InvalidEmails = invalidCount,
                    SkippedDetails = skippedDetails
                };
            }

            // 2. Query DB for existing users and previous referrals
            var existingUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.Email != null && validEmailsToProcess.Contains(u.Email.ToLower()))
                .Select(u => u.Email!.ToLower())
                .ToListAsync();

            var existingUserSet = new HashSet<string>(existingUsers);

            var previousReferrals = await _context.UserReferrals
                .AsNoTracking()
                .Where(r => r.ReferrerUserId == userContext.UserId && validEmailsToProcess.Contains(r.ReferredEmail.ToLower()))
                .Select(r => r.ReferredEmail.ToLower())
                .ToListAsync();

            var previousReferralSet = new HashSet<string>(previousReferrals);

            var newlyInvitedEmails = new List<string>();
            var now = DateTime.UtcNow;

            foreach (var email in validEmailsToProcess)
            {
                if (existingUserSet.Contains(email))
                {
                    alreadyRegisteredCount++;
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = "Already registered on ChatHire"
                    });
                    continue;
                }

                if (previousReferralSet.Contains(email))
                {
                    alreadyInvitedCount++;
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = "Already invited previously by you"
                    });
                    continue;
                }

                // Valid new referral to invite
                var referral = new UserReferral
                {
                    ReferrerUserId = userContext.UserId,
                    ReferredEmail = email,
                    ReferralCode = referralCode,
                    Status = "Invited",
                    RewardClaimed = false,
                    CreatedDate = now,
                    Updated = now
                };

                _context.UserReferrals.Add(referral);
                newlyInvitedEmails.Add(email);
            }

            if (newlyInvitedEmails.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return new SubmitReferralResponseDto
            {
                Success = true,
                Message = newlyInvitedEmails.Count > 0
                    ? $"Successfully sent {newlyInvitedEmails.Count} referral invitation(s)! You will earn 3 months of free postings when your friends join."
                    : "No new invitations sent. All submitted emails were either already registered or already invited.",
                TotalSubmitted = rawEmails.Count,
                SuccessfullyInvited = newlyInvitedEmails.Count,
                AlreadyRegistered = alreadyRegisteredCount,
                AlreadyInvited = alreadyInvitedCount,
                InvalidEmails = invalidCount,
                InvitedEmails = newlyInvitedEmails,
                SkippedDetails = skippedDetails
            };
        }

        public async Task<ReferralStatsDto> GetReferralStats(UserContext userContext)
        {
            if (userContext == null || userContext.UserId <= 0)
            {
                return new ReferralStatsDto();
            }

            string referralCode = $"REF{userContext.UserId}";
            string referralLink = $"https://chathire.com/#/signup?ref={referralCode}";

            var list = await _context.UserReferrals
                .AsNoTracking()
                .Where(r => r.ReferrerUserId == userContext.UserId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            int totalInvited = list.Count;
            int totalRegistered = list.Count(r => r.Status == "Registered" || r.Status == "Rewarded" || r.ReferredUserId.HasValue);
            int freePostingsEarned = totalRegistered * 25; // 25 free postings per joined colleague
            int freeMonthsEarned = totalRegistered >= 3 ? (totalRegistered / 3) * 3 : (totalRegistered > 0 ? 1 : 0);

            var dtoList = list.Select(r => new UserReferralDto
            {
                Id = r.Id,
                ReferredEmail = r.ReferredEmail,
                ReferralCode = r.ReferralCode,
                Status = r.Status,
                ReferredUserId = r.ReferredUserId,
                RewardClaimed = r.RewardClaimed,
                CreatedDate = r.CreatedDate,
                RegisteredDate = r.RegisteredDate,
                RewardGrantedDate = r.RewardGrantedDate
            }).ToList();

            return new ReferralStatsDto
            {
                TotalInvited = totalInvited,
                TotalRegistered = totalRegistered,
                FreePostingsEarned = freePostingsEarned,
                FreeMonthsEarned = freeMonthsEarned,
                ReferralCode = referralCode,
                ReferralLink = referralLink,
                Referrals = dtoList
            };
        }

        public async Task<bool> ProcessSignupReferral(string email, string referralCode, long newUserId)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            string cleanEmail = email.Trim().ToLower();
            string cleanCode = (referralCode ?? "").Trim().ToUpper();

            // 1. Match by email first
            var referral = await _context.UserReferrals
                .FirstOrDefaultAsync(r => r.ReferredEmail.ToLower() == cleanEmail);

            // 2. If not found by email, but signed up via referral link/code
            if (referral == null && !string.IsNullOrEmpty(cleanCode))
            {
                long referrerUserId = 0;
                if (cleanCode.StartsWith("REF") && long.TryParse(cleanCode.Substring(3), out long parsedId))
                {
                    referrerUserId = parsedId;
                }

                if (referrerUserId > 0 && referrerUserId != newUserId)
                {
                    referral = new UserReferral
                    {
                        ReferrerUserId = referrerUserId,
                        ReferredEmail = cleanEmail,
                        ReferralCode = cleanCode,
                        Status = "Invited",
                        RewardClaimed = false,
                        CreatedDate = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    _context.UserReferrals.Add(referral);
                }
            }

            if (referral == null) return false;

            var now = DateTime.UtcNow;
            referral.Status = "Registered";
            referral.ReferredUserId = newUserId;
            referral.RegisteredDate = now;
            referral.RewardClaimed = true;
            referral.RewardGrantedDate = now;
            referral.Updated = now;

            // Grant bonus quota to referrer (e.g. +25 job postings / 3 months extended)
            var referrerPlan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(usp => usp.UserId == referral.ReferrerUserId && usp.Active == true);

            if (referrerPlan != null)
            {
                referrerPlan.ActualJobPosting = (referrerPlan.ActualJobPosting ?? 15) + 25;
                referrerPlan.ActualDownloads = (referrerPlan.ActualDownloads ?? 10) + 15;
                referrerPlan.EndDate = (referrerPlan.EndDate ?? now).AddDays(90); // 3 months free extension
                referrerPlan.Updated = now;
                referrerPlan.UpdatedBy = referral.ReferrerUserId;
            }

            // Grant bonus quota to referee (new user)
            var refereePlan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(usp => usp.UserId == newUserId && usp.Active == true);

            if (refereePlan != null)
            {
                refereePlan.ActualJobPosting = (refereePlan.ActualJobPosting ?? 15) + 25;
                refereePlan.ActualDownloads = (refereePlan.ActualDownloads ?? 10) + 15;
                refereePlan.EndDate = (refereePlan.EndDate ?? now).AddDays(90);
                refereePlan.Updated = now;
                refereePlan.UpdatedBy = newUserId;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
