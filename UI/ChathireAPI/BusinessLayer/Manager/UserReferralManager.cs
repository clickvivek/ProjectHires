using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using BusinessLayer.Services;
using DataAccessLayer.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface IUserReferralManager
    {
        Task<SubmitReferralResponseDto> SubmitReferrals(SubmitReferralRequestDto dto, UserContext userContext);
        Task<ReferralStatsDto> GetReferralStats(UserContext userContext);
        Task<bool> ProcessSignupReferral(string email, string referralCode, long newUserId);
    }

    public class UserReferralManager : BaseManager<UserReferralManager>, IUserReferralManager
    {
        public UserReferralManager(IServiceProvider provider, ILogger<UserReferralManager> logger, IMapper mapper)
            : base(provider, logger, mapper)
        {
        }

        public async Task<SubmitReferralResponseDto> SubmitReferrals(SubmitReferralRequestDto dto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IUserReferralRepository>();
                var response = await repo.SubmitReferrals(dto, userContext);

                if (response.Success && response.InvitedEmails != null && response.InvitedEmails.Count > 0)
                {
                    var emailService = serviceProvider.GetService<IResendEmailService>();
                    if (emailService != null)
                    {
                        var userRepo = repositoryFactory.Get<IUserRepository>();
                        var user = await userRepo.Get(userContext.UserId);
                        string referrerName = user != null ? $"{user.Fname} {user.Lname}".Trim() : "A colleague";
                        if (string.IsNullOrWhiteSpace(referrerName) && user != null)
                        {
                            referrerName = user.UserName ?? "A colleague";
                        }
                        string referralCode = $"REF{userContext.UserId}";
                        string baseUrl = "https://chathire.com";

                        foreach (var email in response.InvitedEmails)
                        {
                            string referralLink = $"{baseUrl}/#/signup?ref={referralCode}&email={Uri.EscapeDataString(email)}";
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await emailService.SendReferralInvitationEmailAsync(email, referrerName, referralLink, dto.CustomMessage);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to send referral email to {Email}", email);
                                }
                            });
                        }
                    }
                }

                return response;
            }, "SubmitReferrals", userContext);
        }

        public async Task<ReferralStatsDto> GetReferralStats(UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IUserReferralRepository>();
                return await repo.GetReferralStats(userContext);
            }, "GetReferralStats", userContext);
        }

        public async Task<bool> ProcessSignupReferral(string email, string referralCode, long newUserId)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IUserReferralRepository>();
                return await repo.ProcessSignupReferral(email, referralCode, newUserId);
            }, "ProcessSignupReferral", null);
        }
    }
}
