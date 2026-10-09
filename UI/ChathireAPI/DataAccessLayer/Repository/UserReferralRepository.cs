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

        private static readonly HashSet<string> PublicEmailDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com", "googlemail.com", "yahoo.com", "yahoo.co.in", "yahoo.co.uk", "yahoo.ca", "ymail.com",
            "hotmail.com", "outlook.com", "live.com", "msn.com", "icloud.com", "me.com", "mac.com",
            "aol.com", "mail.com", "zoho.com", "protonmail.com", "proton.me", "yandex.com", "gmx.com", "gmx.net"
        };

        private static bool IsCompanyEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var parts = email.Split('@');
            if (parts.Length != 2) return false;
            string domain = parts[1].Trim().ToLower();
            return !PublicEmailDomains.Contains(domain) && domain.Contains('.');
        }

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

            bool isOnBehalf = dto.OnBehalfOfUserId.HasValue && dto.OnBehalfOfUserId.Value > 0;
            long effectiveReferrerId = isOnBehalf ? dto.OnBehalfOfUserId!.Value : userContext.UserId;

            var referrer = await _context.Users.FirstOrDefaultAsync(u => u.Id == effectiveReferrerId);
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
                    Message = isOnBehalf ? "Please provide at least one email address." : "Please provide 10 company email addresses."
                };
            }

            var rawEmails = dto.Emails
                .SelectMany(e => (e ?? "").Split(new[] { ',', ';', '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(e => e.Trim().ToLower())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            if (!isOnBehalf && rawEmails.Count < 10)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = $"Minimum 10 company email addresses are required to claim the +10 chat sessions award. You provided {rawEmails.Count} email(s)."
                };
            }
            else if (isOnBehalf && rawEmails.Count == 0)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = "Please provide at least one email address."
                };
            }

            var seenInBatch = new HashSet<string>();
            var validEmailsToProcess = new List<string>();
            var skippedDetails = new List<ReferralSkipDetailDto>();

            int invalidCount = 0;
            int nonCompanyCount = 0;
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

                if (!IsCompanyEmail(email))
                {
                    nonCompanyCount++;
                    var domain = email.Split('@')[1];
                    skippedDetails.Add(new ReferralSkipDetailDto
                    {
                        Email = email,
                        Reason = $"Personal email domain (@{domain}) not allowed. Please provide business/company email IDs."
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

            if (!isOnBehalf && validEmailsToProcess.Count < 10)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = $"At least 10 valid company email addresses are required. {validEmailsToProcess.Count} valid company email(s) found.",
                    TotalSubmitted = rawEmails.Count,
                    InvalidEmails = invalidCount,
                    NonCompanyEmails = nonCompanyCount,
                    SkippedDetails = skippedDetails
                };
            }
            else if (isOnBehalf && validEmailsToProcess.Count == 0)
            {
                return new SubmitReferralResponseDto
                {
                    Success = false,
                    Message = $"No valid company email addresses found.",
                    TotalSubmitted = rawEmails.Count,
                    InvalidEmails = invalidCount,
                    NonCompanyEmails = nonCompanyCount,
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
                .Where(r => r.ReferrerUserId == effectiveReferrerId && validEmailsToProcess.Contains(r.ReferredEmail.ToLower()))
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
                    ReferrerUserId = effectiveReferrerId,
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

            // If not on behalf, immediately grant +10 daily chat sessions for the next 10 days
            var referrerPlan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(usp => usp.UserId == effectiveReferrerId && usp.Active == true);

            int newDailyChatLimit = 30;
            if (!isOnBehalf)
            {
                if (referrerPlan != null)
                {
                    referrerPlan.DailyChatLimit = (referrerPlan.DailyChatLimit.HasValue && referrerPlan.DailyChatLimit.Value > 0)
                        ? referrerPlan.DailyChatLimit.Value + 10
                        : 30;

                    if (referrerPlan.EndDate == null || referrerPlan.EndDate < now.AddDays(10))
                    {
                        referrerPlan.EndDate = now.AddDays(10);
                    }
                    referrerPlan.Updated = now;
                    referrerPlan.UpdatedBy = userContext.UserId;
                    newDailyChatLimit = referrerPlan.DailyChatLimit.Value;
                }
                else
                {
                    referrerPlan = new UserSubscriptionPlan
                    {
                        UserId = effectiveReferrerId,
                        SubscriptionPlanId = 1,
                        ActualJobPosting = 15,
                        ActualDownloads = 10,
                        DailyChatLimit = 30, // 20 standard + 10 bonus
                        NoOfUsers = 1,
                        StartDate = now,
                        EndDate = now.AddDays(10),
                        IsFree = true,
                        Active = true,
                        NoOfUsedJobPosting = 0,
                        NoOfUsedDownloads = 0,
                        Updated = now,
                        UpdatedBy = userContext.UserId
                    };
                    _context.UserSubscriptionPlans.Add(referrerPlan);
                    newDailyChatLimit = 30;
                }
            }
            else if (referrerPlan != null && referrerPlan.DailyChatLimit.HasValue)
            {
                newDailyChatLimit = referrerPlan.DailyChatLimit.Value;
            }

            await _context.SaveChangesAsync();

            string successMessage = isOnBehalf
                ? $"Invitations sent successfully to {newlyInvitedEmails.Count} colleague(s) on behalf of {referrerName}."
                : $"🎉 Instant Award Activated! You have been granted +10 Daily Chat Sessions for the next 10 days (Your daily limit is now {newDailyChatLimit} chats). Invitations sent to {newlyInvitedEmails.Count} colleague(s). When they sign up, you will also receive bonus free job postings!";

            return new SubmitReferralResponseDto
            {
                Success = newlyInvitedEmails.Count > 0,
                Message = successMessage,
                TotalSubmitted = rawEmails.Count,
                SuccessfullyInvited = newlyInvitedEmails.Count,
                AlreadyRegistered = alreadyRegisteredCount,
                AlreadyInvited = alreadyInvitedCount,
                InvalidEmails = invalidCount,
                NonCompanyEmails = nonCompanyCount,
                BonusChatsGranted = isOnBehalf ? 0 : 10,
                NewDailyChatLimit = newDailyChatLimit,
                BonusDurationDays = isOnBehalf ? 0 : 10,
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
            int freeMonthsEarned = totalRegistered > 0 ? 3 : 0; // Capped to 3 months max from start date

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

            // 1. Match all referrals by email
            var matchingReferrals = await _context.UserReferrals
                .Where(r => r.ReferredEmail.ToLower() == cleanEmail)
                .ToListAsync();

            // 2. If not found by email, but signed up via referral link/code
            if (matchingReferrals.Count == 0 && !string.IsNullOrEmpty(cleanCode))
            {
                long referrerUserId = 0;
                if (cleanCode.StartsWith("REF") && long.TryParse(cleanCode.Substring(3), out long parsedId))
                {
                    referrerUserId = parsedId;
                }

                if (referrerUserId > 0 && referrerUserId != newUserId)
                {
                    var newReferral = new UserReferral
                    {
                        ReferrerUserId = referrerUserId,
                        ReferredEmail = cleanEmail,
                        ReferralCode = cleanCode,
                        Status = "Invited",
                        RewardClaimed = false,
                        CreatedDate = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    _context.UserReferrals.Add(newReferral);
                    matchingReferrals.Add(newReferral);
                }
            }

            if (matchingReferrals.Count == 0) return false;

            var now = DateTime.UtcNow;
            bool updatedAny = false;

            foreach (var referral in matchingReferrals)
            {
                if (referral.Status != "Registered" || referral.ReferredUserId == null)
                {
                    referral.Status = "Registered";
                    referral.ReferredUserId = newUserId;
                    referral.RegisteredDate = now;
                    referral.RewardClaimed = true;
                    referral.RewardGrantedDate = now;
                    referral.Updated = now;
                    updatedAny = true;

                    // Grant bonus quota to referrer (e.g. +25 job postings / 3 months extended max)
                    var referrerPlan = await _context.UserSubscriptionPlans
                        .FirstOrDefaultAsync(usp => usp.UserId == referral.ReferrerUserId && usp.Active == true);

                    if (referrerPlan != null)
                    {
                        referrerPlan.ActualJobPosting = (referrerPlan.ActualJobPosting ?? 15) + 25;
                        referrerPlan.ActualDownloads = (referrerPlan.ActualDownloads ?? 10) + 15;
                        
                        // Set EndDate to current date + 90 days
                        referrerPlan.EndDate = now.AddDays(90);

                        referrerPlan.Updated = now;
                        referrerPlan.UpdatedBy = referral.ReferrerUserId;
                    }
                    else
                    {
                        referrerPlan = new UserSubscriptionPlan
                        {
                            UserId = referral.ReferrerUserId,
                            SubscriptionPlanId = 1,
                            ActualJobPosting = 15 + 25,
                            ActualDownloads = 10 + 15,
                            DailyChatLimit = 20,
                            NoOfUsers = 1,
                            StartDate = now,
                            EndDate = now.AddDays(90),
                            IsFree = true,
                            Active = true,
                            NoOfUsedJobPosting = 0,
                            NoOfUsedDownloads = 0,
                            Updated = now,
                            UpdatedBy = referral.ReferrerUserId
                        };
                        _context.UserSubscriptionPlans.Add(referrerPlan);
                    }
                }
            }

            // Grant bonus quota to referee (new user) if at least one referral was processed
            if (updatedAny)
            {
                var refereePlan = await _context.UserSubscriptionPlans
                    .FirstOrDefaultAsync(usp => usp.UserId == newUserId && usp.Active == true);

                if (refereePlan != null)
                {
                    refereePlan.ActualJobPosting = (refereePlan.ActualJobPosting ?? 15) + 25;
                    refereePlan.ActualDownloads = (refereePlan.ActualDownloads ?? 10) + 15;
                    
                    // Set EndDate to current date + 90 days
                    refereePlan.EndDate = now.AddDays(90);

                    refereePlan.Updated = now;
                    refereePlan.UpdatedBy = newUserId;
                }
                else
                {
                    refereePlan = new UserSubscriptionPlan
                    {
                        UserId = newUserId,
                        SubscriptionPlanId = 1,
                        ActualJobPosting = 15 + 25,
                        ActualDownloads = 10 + 15,
                        DailyChatLimit = 20,
                        NoOfUsers = 1,
                        StartDate = now,
                        EndDate = now.AddDays(90),
                        IsFree = true,
                        Active = true,
                        NoOfUsedJobPosting = 0,
                        NoOfUsedDownloads = 0,
                        Updated = now,
                        UpdatedBy = newUserId
                    };
                    _context.UserSubscriptionPlans.Add(refereePlan);
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
