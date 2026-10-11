using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IUserSubscriptionPlanRepository : IRepository<UserSubscriptionPlan, long>
    {
        Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanById(long? Id, UserContext userContext);
        Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext);
        Task<UserQuotaStatusDto> GetUserQuotaStatus(long userId, UserContext userContext);
        Task<UserQuotaListResponseDto> GetAllUsersQuotas(int page, int pageSize, string? search, string? filter, UserContext userContext);
        Task<bool> UpdateUserQuota(UpdateUserQuotaDto dto, UserContext userContext);
        Task<InitiateChatResultDto> InitiateChat(long userId, long chatUserId, UserContext userContext);
        Task<CanDownloadResumeResultDto> CheckAndRecordDownloadAsync(long userId, string fileName, UserContext userContext);
    }

    public class UserSubscriptionPlanRepository : BaseRepository<UserSubscriptionPlan, long>, IUserSubscriptionPlanRepository
    {
        public UserSubscriptionPlanRepository(EFContexts context) : base(context) { }

        public async Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanById(long? Id, UserContext userContext)
        {
            var userSubscriptionList = _context.UserSubscriptionPlans
               .Include(o => o.SubscriptionPlan)
               .ThenInclude(o=>o.SubscriptionPlanFeatures)
               .Include(o => o.User)
            .Where(c => c.Id == Id);

            return await userSubscriptionList.ToListAsync();
        }

        public async Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext)
        {
            var userSubscriptionList = _context.UserSubscriptionPlans
               .Include(o => o.SubscriptionPlan)
               .ThenInclude(o => o.SubscriptionPlanFeatures)
               .Include(o => o.User)
            .Where(c => c.UserId == Id);

            return await userSubscriptionList.ToListAsync();
        }

        public async Task<UserQuotaStatusDto> GetUserQuotaStatus(long userId, UserContext userContext)
        {
            var now = DateTime.UtcNow;

            if (userId <= 0)
            {
                return new UserQuotaStatusDto
                {
                    CycleStartDate = now,
                    CycleEndDate = now.AddDays(30),
                    DaysRemainingInCycle = 30,
                    MaxJobPostings = 15,
                    UsedJobPostings = 0,
                    RemainingJobPostings = 15,
                    MaxDownloads = 10,
                    UsedDownloads = 0,
                    RemainingDownloads = 10,
                    DailyChatLimit = 20,
                    UsedChatsToday = 0,
                    RemainingChatsToday = 20,
                    IsFreeTier = true,
                    PlanName = "Free Plan",
                    IsLimitReached = false,
                    PostingsPercentage = 0,
                    DownloadsPercentage = 0
                };
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            
            var subPlans = await _context.UserSubscriptionPlans
                .Include(o => o.SubscriptionPlan)
                .Where(c => c.UserId == userId && c.Active == true)
                .ToListAsync();

            var activePaidPlan = subPlans.FirstOrDefault(p => (p.ActualJobPosting > 15 || p.IsFree == false || (p.SubscriptionPlan != null && p.SubscriptionPlan.IsFree == false)) && (p.EndDate == null || p.EndDate >= now));

            DateTime baselineDate = (activePaidPlan != null && activePaidPlan.StartDate.HasValue) 
                ? activePaidPlan.StartDate.Value 
                : (user?.Updated ?? now);

            // Calculate 30-day rolling cycle
            var totalDays = (now - baselineDate).TotalDays;
            if (totalDays < 0) totalDays = 0;
            int cycleIndex = (int)Math.Floor(totalDays / 30.0);

            DateTime cycleStartDate = baselineDate.AddDays(cycleIndex * 30);
            DateTime cycleEndDate = cycleStartDate.AddDays(30);
            int daysRemaining = Math.Max(0, (int)Math.Ceiling((cycleEndDate - now).TotalDays));

            int maxJobPostings = 15;
            int maxDownloads = 10;
            int dailyChatLimit = 20;
            bool isFreeTier = true;
            string planName = "Free Plan";

            if (activePaidPlan != null)
            {
                maxJobPostings = activePaidPlan.ActualJobPosting ?? activePaidPlan.SubscriptionPlan?.NoOfJobPosting ?? 100;
                maxDownloads = activePaidPlan.ActualDownloads ?? activePaidPlan.SubscriptionPlan?.NoOfDownloads ?? 100;
                dailyChatLimit = (activePaidPlan.DailyChatLimit.HasValue && activePaidPlan.DailyChatLimit.Value > 0)
                    ? activePaidPlan.DailyChatLimit.Value
                    : (activePaidPlan.IsFree == false ? 500 : 20);
                isFreeTier = activePaidPlan.IsFree ?? false;
                planName = activePaidPlan.SubscriptionPlan?.Description ?? (isFreeTier ? "Free Plan" : "Pro Plan");
            }

            // Get user's consultancy user IDs
            var cuIds = await _context.ConsultancyUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.Id)
                .ToListAsync();

            // Count Job Postings in current 30-day window
            int usedJobPostings = await _context.JobOpenings
                .CountAsync(j => ((j.ConsultancyUserId.HasValue && cuIds.Contains(j.ConsultancyUserId.Value)) || j.UpdatedBy == userId) 
                                 && j.Updated >= cycleStartDate && j.Updated < cycleEndDate 
                                 && (j.Active == true || j.Active == null));

            // Count Downloads in current 30-day window
            int usedDownloads = 0;
            if (cuIds.Any())
            {
                usedDownloads = await _context.CandidateProfileViewHistories
                    .CountAsync(v => v.ConsultancyUserId.HasValue && cuIds.Contains(v.ConsultancyUserId.Value)
                                     && v.Updated >= cycleStartDate && v.Updated < cycleEndDate);
            }

            int remainingJobPostings = Math.Max(0, maxJobPostings - usedJobPostings);
            int remainingDownloads = Math.Max(0, maxDownloads - usedDownloads);

            double postingPct = maxJobPostings > 0 ? Math.Min(100.0, Math.Round((double)usedJobPostings / maxJobPostings * 100.0, 1)) : 0;
            double downloadPct = maxDownloads > 0 ? Math.Min(100.0, Math.Round((double)usedDownloads / maxDownloads * 100.0, 1)) : 0;

            var windowStart = now.AddHours(-24);
            var recentChats = await _context.ChatHistories
                .Where(c => c.UserId == userId && c.ChatTime >= windowStart)
                .OrderBy(c => c.ChatTime)
                .ToListAsync();
            int usedChatsToday = recentChats.Count;
            int remainingChatsToday = Math.Max(0, dailyChatLimit - usedChatsToday);

            DateTime? nextSlotAvailableAtUtc = null;
            long? nextSlotWaitSeconds = null;
            string nextSlotWaitText = string.Empty;

            if (usedChatsToday >= dailyChatLimit && recentChats.Any())
            {
                var earliest = recentChats.First();
                nextSlotAvailableAtUtc = earliest.ChatTime.AddHours(24);
                var wait = nextSlotAvailableAtUtc.Value > now ? (nextSlotAvailableAtUtc.Value - now) : TimeSpan.Zero;
                nextSlotWaitSeconds = (long)Math.Ceiling(wait.TotalSeconds);
                nextSlotWaitText = FormatWaitDuration(wait);
            }

            return new UserQuotaStatusDto
            {
                CycleStartDate = cycleStartDate,
                CycleEndDate = cycleEndDate,
                DaysRemainingInCycle = daysRemaining,
                MaxJobPostings = maxJobPostings,
                UsedJobPostings = usedJobPostings,
                RemainingJobPostings = remainingJobPostings,
                MaxDownloads = maxDownloads,
                UsedDownloads = usedDownloads,
                RemainingDownloads = remainingDownloads,
                DailyChatLimit = dailyChatLimit,
                UsedChatsToday = usedChatsToday,
                RemainingChatsToday = remainingChatsToday,
                NextSlotAvailableAtUtc = nextSlotAvailableAtUtc,
                NextSlotWaitSeconds = nextSlotWaitSeconds,
                NextSlotWaitText = nextSlotWaitText,
                IsFreeTier = isFreeTier,
                PlanName = planName,
                IsLimitReached = remainingJobPostings <= 0,
                PostingsPercentage = postingPct,
                DownloadsPercentage = downloadPct
            };
        }

        public async Task<UserQuotaListResponseDto> GetAllUsersQuotas(int page, int pageSize, string? search, string? filter, UserContext userContext)
        {
            var now = DateTime.UtcNow;
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;

            var query = _context.Users
                .Include(u => u.ConsultancyUsers)
                    .ThenInclude(cu => cu.Consultancy)
                .Include(u => u.UserSubscriptionPlans)
                    .ThenInclude(usp => usp.SubscriptionPlan)
                .AsNoTracking()
                .Where(u => u.Active == true || u.Active == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(u => (u.Fname != null && u.Fname.ToLower().Contains(s))
                                      || (u.Lname != null && u.Lname.ToLower().Contains(s))
                                      || (u.Email != null && u.Email.ToLower().Contains(s))
                                      || (u.ConsultancyUsers.Any(cu => cu.Consultancy != null && cu.Consultancy.Name != null && cu.Consultancy.Name.ToLower().Contains(s))));
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim().ToLower();
                if (f == "recruiter")
                {
                    query = query.Where(u => u.RoleRecruiter == true);
                }
                else if (f == "benchsales")
                {
                    query = query.Where(u => u.RoleBenchSales == true);
                }
                else if (f == "admin")
                {
                    query = query.Where(u => u.UserTypeId == 7);
                }
                else if (f == "pro")
                {
                    query = query.Where(u => u.UserSubscriptionPlans.Any(usp => usp.Active == true && usp.IsFree == false && (usp.EndDate == null || usp.EndDate >= now)));
                }
                else if (f == "free")
                {
                    query = query.Where(u => !u.UserSubscriptionPlans.Any(usp => usp.Active == true && usp.IsFree == false && (usp.EndDate == null || usp.EndDate >= now)));
                }
            }

            int totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var consultancyUsers = await _context.ConsultancyUsers
                .Where(cu => userIds.Contains(cu.UserId))
                .Select(cu => new { cu.Id, cu.UserId })
                .ToListAsync();

            var cuIds = consultancyUsers.Select(cu => cu.Id).ToList();

            var sixtyDaysAgo = now.AddDays(-60);
            var jobOpenings = await _context.JobOpenings
                .Where(j => ((j.ConsultancyUserId.HasValue && cuIds.Contains(j.ConsultancyUserId.Value)) || (j.UpdatedBy.HasValue && userIds.Contains(j.UpdatedBy.Value)))
                            && j.Updated >= sixtyDaysAgo && (j.Active == true || j.Active == null))
                .Select(j => new { j.ConsultancyUserId, j.UpdatedBy, j.Updated })
                .ToListAsync();

            var profileViews = await _context.CandidateProfileViewHistories
                .Where(v => v.ConsultancyUserId.HasValue && cuIds.Contains(v.ConsultancyUserId.Value) && v.Updated >= sixtyDaysAgo)
                .Select(v => new { v.ConsultancyUserId, v.Updated })
                .ToListAsync();

            var items = new List<UserQuotaDetailDto>();

            foreach (var user in users)
            {
                var activeSubPlan = user.UserSubscriptionPlans?.FirstOrDefault(p => p.Active == true && (p.EndDate == null || p.EndDate >= now));

                DateTime baselineDate = (activeSubPlan != null && activeSubPlan.StartDate.HasValue)
                    ? activeSubPlan.StartDate.Value
                    : (user.Updated ?? now);

                var totalDays = (now - baselineDate).TotalDays;
                if (totalDays < 0) totalDays = 0;
                int cycleIndex = (int)Math.Floor(totalDays / 30.0);

                DateTime cycleStartDate = baselineDate.AddDays(cycleIndex * 30);
                DateTime cycleEndDate = activeSubPlan?.EndDate ?? cycleStartDate.AddDays(30);
                int daysRemaining = Math.Max(0, (int)Math.Ceiling((cycleEndDate - now).TotalDays));

                int maxJobPostings = activeSubPlan?.ActualJobPosting ?? 15;
                int maxDownloads = activeSubPlan?.ActualDownloads ?? 10;
                int dailyChatLimit = (activeSubPlan?.DailyChatLimit.HasValue == true && activeSubPlan.DailyChatLimit.Value > 0)
                    ? activeSubPlan.DailyChatLimit.Value
                    : (activeSubPlan?.IsFree == false ? 500 : 20);
                bool isFree = activeSubPlan?.IsFree ?? true;
                string planName = activeSubPlan?.SubscriptionPlan?.Description ?? (isFree ? "Free Plan" : "Custom Pro Plan");

                var userCuIds = consultancyUsers.Where(cu => cu.UserId == user.Id).Select(cu => cu.Id).ToList();

                int usedJobPostings = jobOpenings
                    .Count(j => ((j.ConsultancyUserId.HasValue && userCuIds.Contains(j.ConsultancyUserId.Value)) || j.UpdatedBy == user.Id)
                                && j.Updated >= cycleStartDate && j.Updated < cycleEndDate);

                int usedDownloads = profileViews
                    .Count(v => v.ConsultancyUserId.HasValue && userCuIds.Contains(v.ConsultancyUserId.Value)
                                && v.Updated >= cycleStartDate && v.Updated < cycleEndDate);

                int remainingJobPostings = Math.Max(0, maxJobPostings - usedJobPostings);
                int remainingDownloads = Math.Max(0, maxDownloads - usedDownloads);

                string compName = user.ConsultancyUsers?.FirstOrDefault(cu => cu.Consultancy != null)?.Consultancy?.Name ?? "";
                string roleName = "Consultant";
                if (user.UserTypeId == 7) roleName = "Admin";
                else if (user.RoleRecruiter == true && user.RoleBenchSales == true) roleName = "Recruiter / Bench";
                else if (user.RoleRecruiter == true) roleName = "Recruiter";
                else if (user.RoleBenchSales == true) roleName = "Bench Sales";
                else if (user.UserTypeId == 5) roleName = "Candidate";

                string cleanFname = (user.Fname ?? "").Trim();
                string cleanLname = (user.Lname ?? "").Trim();
                string cleanFullName = string.IsNullOrWhiteSpace(cleanFname) && string.IsNullOrWhiteSpace(cleanLname)
                    ? (user.UserName ?? user.Email ?? "No Name")
                    : $"{cleanFname} {cleanLname}".Trim();

                items.Add(new UserQuotaDetailDto
                {
                    UserId = user.Id,
                    FullName = cleanFullName,
                    Email = user.Email ?? "",
                    CompanyName = compName,
                    UserTypeId = user.UserTypeId,
                    RoleName = roleName,
                    RoleRecruiter = user.RoleRecruiter ?? false,
                    RoleBenchSales = user.RoleBenchSales ?? false,
                    SignupDate = user.Updated,
                    UserSubscriptionPlanId = activeSubPlan?.Id,
                    PlanName = planName,
                    IsFree = isFree,
                    CycleStartDate = cycleStartDate,
                    CycleEndDate = cycleEndDate,
                    DaysRemainingInCycle = daysRemaining,
                    ActualJobPosting = maxJobPostings,
                    UsedJobPostings = usedJobPostings,
                    RemainingJobPostings = remainingJobPostings,
                    ActualDownloads = maxDownloads,
                    UsedDownloads = usedDownloads,
                    RemainingDownloads = remainingDownloads,
                    DailyChatLimit = dailyChatLimit,
                    IsLimitReached = remainingJobPostings <= 0,
                    Active = user.Active.GetValueOrDefault(true)
                });
            }

            return new UserQuotaListResponseDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<bool> UpdateUserQuota(UpdateUserQuotaDto dto, UserContext userContext)
        {
            var now = DateTime.UtcNow;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) throw new Exception("User not found");

            var existingPlan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.Active == true);

            if (existingPlan == null)
            {
                var newPlan = new UserSubscriptionPlan
                {
                    UserId = dto.UserId,
                    ActualJobPosting = dto.ActualJobPosting,
                    ActualDownloads = dto.ActualDownloads,
                    DailyChatLimit = dto.DailyChatLimit,
                    NoOfUsers = 1,
                    StartDate = dto.StartDate ?? now,
                    EndDate = dto.EndDate ?? now.AddDays(30),
                    IsFree = dto.IsFree,
                    Active = true,
                    NoOfUsedJobPosting = 0,
                    NoOfUsedDownloads = 0,
                    Updated = now,
                    UpdatedBy = userContext.UserId > 0 ? userContext.UserId : -1
                };
                _context.UserSubscriptionPlans.Add(newPlan);
            }
            else
            {
                existingPlan.ActualJobPosting = dto.ActualJobPosting;
                existingPlan.ActualDownloads = dto.ActualDownloads;
                existingPlan.DailyChatLimit = dto.DailyChatLimit;
                if (existingPlan.NoOfUsers == null || existingPlan.NoOfUsers > 50)
                {
                    existingPlan.NoOfUsers = 1;
                }
                if (dto.StartDate.HasValue) existingPlan.StartDate = dto.StartDate.Value;
                if (dto.EndDate.HasValue) existingPlan.EndDate = dto.EndDate.Value;
                existingPlan.IsFree = dto.IsFree;
                existingPlan.Active = true;
                existingPlan.Updated = now;
                existingPlan.UpdatedBy = userContext.UserId > 0 ? userContext.UserId : -1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static string FormatWaitDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds <= 0) return "a few seconds";

            int totalMinutes = (int)Math.Ceiling(timeSpan.TotalMinutes);
            int hours = (int)timeSpan.TotalHours;
            int minutes = timeSpan.Minutes;

            if (hours >= 1)
            {
                if (minutes > 0)
                {
                    return $"{hours} hr{(hours > 1 ? "s" : "")} {minutes} min{(minutes > 1 ? "s" : "")}";
                }
                return $"{hours} hr{(hours > 1 ? "s" : "")}";
            }

            if (totalMinutes > 1)
            {
                return $"{totalMinutes} mins";
            }

            if (totalMinutes == 1)
            {
                return "1 min";
            }

            int seconds = Math.Max(1, (int)timeSpan.TotalSeconds);
            return $"{seconds} sec{(seconds > 1 ? "s" : "")}";
        }

        public async Task<InitiateChatResultDto> InitiateChat(long userId, long chatUserId, UserContext userContext)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddHours(-24);

            var configRepo = new ConfigRepository(_context);
            int inactiveDays = await configRepo.GetConfigIntAsync("InactiveChatPurgeDays", 30);
            int defaultDailyLimit = await configRepo.GetConfigIntAsync("DefaultDailyChatLimit", 20);

            if (userId <= 0)
            {
                return new InitiateChatResultDto
                {
                    CanChat = false,
                    DailyChatLimit = defaultDailyLimit,
                    UsedChatsToday = 0,
                    RemainingChatsToday = 0,
                    Message = "Please log in to initiate chat."
                };
            }

            // Get user active plan
            var subPlans = await _context.UserSubscriptionPlans
                .Include(o => o.SubscriptionPlan)
                .Where(c => c.UserId == userId && c.Active == true)
                .ToListAsync();

            var activePaidPlan = subPlans.FirstOrDefault(p => (p.ActualJobPosting > 15 || p.IsFree == false || (p.SubscriptionPlan != null && p.SubscriptionPlan.IsFree == false)) && (p.EndDate == null || p.EndDate >= now));

            int dailyChatLimit = defaultDailyLimit;
            if (activePaidPlan != null)
            {
                dailyChatLimit = (activePaidPlan.DailyChatLimit.HasValue && activePaidPlan.DailyChatLimit.Value > 0)
                    ? activePaidPlan.DailyChatLimit.Value
                    : (activePaidPlan.IsFree == false ? 500 : defaultDailyLimit);
            }
            else
            {
                var freePlan = subPlans.FirstOrDefault();
                if (freePlan != null && freePlan.DailyChatLimit.HasValue && freePlan.DailyChatLimit.Value > 0)
                {
                    dailyChatLimit = freePlan.DailyChatLimit.Value;
                }
            }

            var recentChats = await _context.ChatHistories
                .Where(c => c.UserId == userId && c.ChatTime >= windowStart && (c.IsActive == true || c.IsActive == null))
                .OrderBy(c => c.ChatTime)
                .ToListAsync();

            int usedChatsInWindow = recentChats.Count;

            if (chatUserId <= 0 || chatUserId == userId)
            {
                return new InitiateChatResultDto
                {
                    CanChat = true,
                    DailyChatLimit = dailyChatLimit,
                    UsedChatsToday = usedChatsInWindow,
                    RemainingChatsToday = Math.Max(0, dailyChatLimit - usedChatsInWindow),
                    IsExistingConversationToday = true,
                    Message = "Valid session."
                };
            }

            // Check if conversation was already logged in the last 24 hours with this partner
            bool alreadyChattedInLast24Hours = recentChats.Any(c => c.ChatUserId == chatUserId);

            if (alreadyChattedInLast24Hours)
            {
                return new InitiateChatResultDto
                {
                    CanChat = true,
                    DailyChatLimit = dailyChatLimit,
                    UsedChatsToday = usedChatsInWindow,
                    RemainingChatsToday = Math.Max(0, dailyChatLimit - usedChatsInWindow),
                    IsExistingConversationToday = true,
                    Message = "Continuing existing conversation."
                };
            }

            // Check if there was any active chat within the configured inactiveDays (e.g. 30 days)
            var inactivityThreshold = now.AddDays(-inactiveDays);
            var lastChatWithPartner = await _context.ChatHistories
                .Where(c => c.UserId == userId && c.ChatUserId == chatUserId)
                .OrderByDescending(c => c.ChatTime)
                .FirstOrDefaultAsync();

            bool isChatOlderThanInactivityWindow = lastChatWithPartner == null 
                || lastChatWithPartner.ChatTime < inactivityThreshold 
                || lastChatWithPartner.IsActive == false;

            if (usedChatsInWindow >= dailyChatLimit)
            {
                var earliest = recentChats.First();
                var nextSlotUtc = earliest.ChatTime.AddHours(24);
                var wait = nextSlotUtc > now ? (nextSlotUtc - now) : TimeSpan.Zero;
                long waitSec = (long)Math.Ceiling(wait.TotalSeconds);
                string waitTxt = FormatWaitDuration(wait);

                return new InitiateChatResultDto
                {
                    CanChat = false,
                    DailyChatLimit = dailyChatLimit,
                    UsedChatsToday = usedChatsInWindow,
                    RemainingChatsToday = 0,
                    IsExistingConversationToday = false,
                    NextSlotAvailableAtUtc = nextSlotUtc,
                    NextSlotWaitSeconds = waitSec,
                    NextSlotWaitText = waitTxt,
                    Message = $"You have reached your limit of {dailyChatLimit} chats in the last 24 hours. Please wait {waitTxt} to initiate your next chat, or refer colleagues to earn +50 chats."
                };
            }

            // Insert new chat record
            var record = new ChatHistory
            {
                UserId = userId,
                ChatUserId = chatUserId,
                ChatTime = now,
                IsActive = true,
                Updated = now,
                UpdatedBy = userId
            };
            _context.ChatHistories.Add(record);
            await _context.SaveChangesAsync();

            usedChatsInWindow++;
            int remainingChatsToday = Math.Max(0, dailyChatLimit - usedChatsInWindow);

            return new InitiateChatResultDto
            {
                CanChat = true,
                DailyChatLimit = dailyChatLimit,
                UsedChatsToday = usedChatsInWindow,
                RemainingChatsToday = remainingChatsToday,
                IsExistingConversationToday = false,
                Message = "Chat initiated successfully."
            };
        }

        public async Task<CanDownloadResumeResultDto> CheckAndRecordDownloadAsync(long userId, string fileName, UserContext userContext)
        {
            var now = DateTime.UtcNow;

            if (userId <= 0)
            {
                return new CanDownloadResumeResultDto
                {
                    CanDownload = false,
                    MaxDownloads = 10,
                    UsedDownloads = 0,
                    RemainingDownloads = 0,
                    CycleEndDate = now.AddDays(30),
                    DaysRemainingInCycle = 30,
                    AlreadyDownloaded = false,
                    Message = "Please log in to download candidate resumes."
                };
            }

            // Check if user is Super Admin
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && user.UserTypeId == 7)
            {
                // Super Admin has unlimited downloads
                return new CanDownloadResumeResultDto
                {
                    CanDownload = true,
                    MaxDownloads = 9999,
                    UsedDownloads = 0,
                    RemainingDownloads = 9999,
                    CycleEndDate = now.AddYears(1),
                    DaysRemainingInCycle = 365,
                    AlreadyDownloaded = false,
                    Message = "Super Admin unlimited access."
                };
            }

            // Get user quota status
            var quota = await GetUserQuotaStatus(userId, userContext);
            int maxDownloads = quota.MaxDownloads;
            int usedDownloads = quota.UsedDownloads;
            int remainingDownloads = quota.RemainingDownloads;
            DateTime cycleEndDate = quota.CycleEndDate;
            int daysRemaining = quota.DaysRemainingInCycle;

            // Resolve CandidateProfileId from CandidateDocument if fileName matches
            long? candidateProfileId = null;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var cleanName = System.IO.Path.GetFileName(fileName);
                var candDoc = await _context.CandidateDocuments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cd => cd.Doc == cleanName || cd.Doc == fileName);
                if (candDoc != null)
                {
                    candidateProfileId = candDoc.CandidateProfileId;
                }
            }

            // Get user's primary ConsultancyUser ID
            var consultancyUser = await _context.ConsultancyUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId);
            long? cuId = consultancyUser?.Id;

            // If user previously downloaded/viewed this candidate profile in this cycle, don't charge another download
            if (candidateProfileId.HasValue && cuId.HasValue)
            {
                bool alreadyDownloadedInCycle = await _context.CandidateProfileViewHistories
                    .AnyAsync(v => v.CandidateProfileid == candidateProfileId.Value
                                   && v.ConsultancyUserId == cuId.Value
                                   && v.Updated >= quota.CycleStartDate && v.Updated < quota.CycleEndDate);

                if (alreadyDownloadedInCycle)
                {
                    return new CanDownloadResumeResultDto
                    {
                        CanDownload = true,
                        MaxDownloads = maxDownloads,
                        UsedDownloads = usedDownloads,
                        RemainingDownloads = remainingDownloads,
                        CycleEndDate = cycleEndDate,
                        DaysRemainingInCycle = daysRemaining,
                        AlreadyDownloaded = true,
                        Message = "Already downloaded in current billing cycle."
                    };
                }
            }

            // Check if limit is reached
            if (remainingDownloads <= 0)
            {
                return new CanDownloadResumeResultDto
                {
                    CanDownload = false,
                    MaxDownloads = maxDownloads,
                    UsedDownloads = usedDownloads,
                    RemainingDownloads = 0,
                    CycleEndDate = cycleEndDate,
                    DaysRemainingInCycle = daysRemaining,
                    AlreadyDownloaded = false,
                    Message = $"Resume Download Limit Reached: You have used all {maxDownloads} resume downloads for this 30-day period. Your quota resets on {cycleEndDate:MMM dd, yyyy} ({daysRemaining} days remaining). Please upgrade your plan or refer colleagues to get more downloads."
                };
            }

            // Record download in CandidateProfileViewHistory
            if (cuId.HasValue)
            {
                var viewRecord = new CandidateProfileViewHistory
                {
                    CandidateProfileid = candidateProfileId,
                    ConsultancyUserId = cuId.Value,
                    Active = true,
                    Updated = now,
                    UpdatedBy = userId
                };
                _context.CandidateProfileViewHistories.Add(viewRecord);
                await _context.SaveChangesAsync();
            }

            // Also update NoOfUsedDownloads on active UserSubscriptionPlan
            try
            {
                var activePlan = await _context.UserSubscriptionPlans
                    .Where(p => p.UserId == userId && p.Active == true)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();
                if (activePlan != null)
                {
                    activePlan.NoOfUsedDownloads = (activePlan.NoOfUsedDownloads ?? 0) + 1;
                    activePlan.Updated = now;
                    await _context.SaveChangesAsync();
                }
            }
            catch { }

            usedDownloads++;
            remainingDownloads = Math.Max(0, maxDownloads - usedDownloads);

            return new CanDownloadResumeResultDto
            {
                CanDownload = true,
                MaxDownloads = maxDownloads,
                UsedDownloads = usedDownloads,
                RemainingDownloads = remainingDownloads,
                CycleEndDate = cycleEndDate,
                DaysRemainingInCycle = daysRemaining,
                AlreadyDownloaded = false,
                Message = "Download authorized."
            };
        }
    }
}
