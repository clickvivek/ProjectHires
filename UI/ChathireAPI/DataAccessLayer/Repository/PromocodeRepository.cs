using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IPromocodeRepository : IRepository<Promocode, long>
    {
        Task<List<PromocodeDto>> GetAllPromocodes(UserContext userContext);
        Task<Promocode?> GetPromocodeById(long id, UserContext userContext);
        Task<Promocode?> GetPromocodeByCode(string code, UserContext userContext);
        Task<PromocodeDto> CreatePromocode(CreatePromocodeDto dto, UserContext userContext);
        Task<bool> DeletePromocode(long id, UserContext userContext);
        Task<bool> ToggleActive(long id, UserContext userContext);
        Task<RedeemPromocodeResponseDto> RedeemPromocode(RedeemPromocodeDto dto, UserContext userContext);
    }

    public class PromocodeRepository : BaseRepository<Promocode, long>, IPromocodeRepository
    {
        public PromocodeRepository(EFContexts context) : base(context) { }

        public async Task<List<PromocodeDto>> GetAllPromocodes(UserContext userContext)
        {
            var redemptions = await _context.UserSubscriptionPlans
                .Where(u => u.PromocodeId != null)
                .GroupBy(u => u.PromocodeId!.Value)
                .Select(g => new { PromocodeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PromocodeId, x => x.Count);

            var list = await _context.Promocodes
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var result = list.Select(p => new PromocodeDto
            {
                Id = p.Id,
                Promocode = p.Promocode1,
                Description = p.Description,
                NoOfFreeDownloads = p.NoOfFreeDownloads,
                NoOfFreeJobPosting = p.NoOfFreeJobPosting,
                DiscountAmount = p.DiscountAmount,
                DailyChatLimit = p.DailyChatLimit,
                IsSingleUse = p.IsSingleUse,
                MaxRedemptions = p.MaxRedemptions,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Active = p.Active,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RedemptionCount = redemptions.ContainsKey(p.Id) ? redemptions[p.Id] : 0
            }).ToList();

            return result;
        }

        public async Task<Promocode?> GetPromocodeById(long id, UserContext userContext)
        {
            return await _context.Promocodes.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Promocode?> GetPromocodeByCode(string code, UserContext userContext)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            string cleanCode = code.Trim().ToLower();
            return await _context.Promocodes.FirstOrDefaultAsync(p => p.Promocode1 != null && p.Promocode1.ToLower() == cleanCode);
        }

        public async Task<PromocodeDto> CreatePromocode(CreatePromocodeDto dto, UserContext userContext)
        {
            if (string.IsNullOrWhiteSpace(dto.Promocode))
            {
                throw new ArgumentException("Promo code cannot be empty.");
            }

            string cleanCode = dto.Promocode.Trim().ToUpper();

            var existing = await _context.Promocodes.FirstOrDefaultAsync(p => p.Promocode1 != null && p.Promocode1.ToUpper() == cleanCode);
            if (existing != null)
            {
                throw new InvalidOperationException($"Promo code '{cleanCode}' already exists.");
            }

            if (dto.EndDate < dto.StartDate)
            {
                throw new ArgumentException("End Date must be greater than or equal to Start Date.");
            }

            bool isSingleUse = dto.IsSingleUse ?? (dto.MaxRedemptions == 1);
            int? maxRedemptions = isSingleUse ? 1 : dto.MaxRedemptions;

            var promo = new Promocode
            {
                Promocode1 = cleanCode,
                Description = dto.Description?.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NoOfFreeDownloads = dto.NoOfFreeDownloads,
                NoOfFreeJobPosting = dto.NoOfFreeJobPosting,
                DiscountAmount = dto.DiscountAmount,
                DailyChatLimit = dto.DailyChatLimit,
                IsSingleUse = isSingleUse,
                MaxRedemptions = maxRedemptions,
                Active = dto.Active ?? true,
                Updated = DateTime.UtcNow,
                UpdatedBy = userContext?.UserId > 0 ? userContext.UserId : null
            };

            _context.Promocodes.Add(promo);
            await _context.SaveChangesAsync();

            return new PromocodeDto
            {
                Id = promo.Id,
                Promocode = promo.Promocode1,
                Description = promo.Description,
                NoOfFreeDownloads = promo.NoOfFreeDownloads,
                NoOfFreeJobPosting = promo.NoOfFreeJobPosting,
                DiscountAmount = promo.DiscountAmount,
                DailyChatLimit = promo.DailyChatLimit,
                IsSingleUse = promo.IsSingleUse,
                MaxRedemptions = promo.MaxRedemptions,
                StartDate = promo.StartDate,
                EndDate = promo.EndDate,
                Active = promo.Active,
                Updated = promo.Updated,
                UpdatedBy = promo.UpdatedBy,
                RedemptionCount = 0
            };
        }

        public async Task<bool> DeletePromocode(long id, UserContext userContext)
        {
            var promo = await _context.Promocodes.FirstOrDefaultAsync(p => p.Id == id);
            if (promo == null) return false;

            // Check if any user subscription plan references this promo code
            bool hasUsages = await _context.UserSubscriptionPlans.AnyAsync(u => u.PromocodeId == id);
            if (hasUsages)
            {
                // Soft delete / deactivate to preserve historical referential integrity
                promo.Active = false;
                promo.Updated = DateTime.UtcNow;
                promo.UpdatedBy = userContext?.UserId > 0 ? userContext.UserId : null;
            }
            else
            {
                _context.Promocodes.Remove(promo);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActive(long id, UserContext userContext)
        {
            var promo = await _context.Promocodes.FirstOrDefaultAsync(p => p.Id == id);
            if (promo == null) return false;

            promo.Active = !(promo.Active ?? false);
            promo.Updated = DateTime.UtcNow;
            promo.UpdatedBy = userContext?.UserId > 0 ? userContext.UserId : null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RedeemPromocodeResponseDto> RedeemPromocode(RedeemPromocodeDto dto, UserContext userContext)
        {
            if (string.IsNullOrWhiteSpace(dto.Promocode))
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = "Please enter a valid promo code."
                };
            }

            long targetUserId = dto.UserId.GetValueOrDefault(0) > 0 ? dto.UserId!.Value : userContext.UserId;
            if (targetUserId <= 0)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = "User authentication required to redeem promo code."
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (user == null)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = "User account not found."
                };
            }

            // Exclude Candidate users (UserTypeId == 5)
            if (user.UserTypeId == 5)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = "Promo codes are exclusively available for Recruiter & Employer accounts (not applicable for candidate profiles)."
                };
            }

            string cleanCode = dto.Promocode.Trim().ToUpper();
            var promo = await _context.Promocodes.FirstOrDefaultAsync(p => p.Promocode1 != null && p.Promocode1.ToUpper() == cleanCode);

            if (promo == null || promo.Active != true)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"Promo code '{cleanCode}' is invalid or inactive."
                };
            }

            var nowUtc = DateTime.UtcNow;
            var nowLocal = DateTime.Now;
            var now = nowUtc;
            if (promo.StartDate.HasValue && promo.StartDate.Value.Date > nowUtc.Date && promo.StartDate.Value.Date > nowLocal.Date)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"This promo code is not active yet. It will become active on {promo.StartDate.Value:MMM dd, yyyy}."
                };
            }

            if (promo.EndDate.HasValue && promo.EndDate.Value.Date < nowUtc.Date && promo.EndDate.Value.Date < nowLocal.Date)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"This promo code has expired on {promo.EndDate.Value:MMM dd, yyyy}."
                };
            }

            // Check if THIS user has already redeemed this code
            bool alreadyRedeemed = await _context.UserSubscriptionPlans
                .AnyAsync(usp => usp.UserId == targetUserId && usp.PromocodeId == promo.Id);

            if (alreadyRedeemed)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"You have already redeemed promo code '{cleanCode}'."
                };
            }

            // Check total redemptions for single-use / max redemption limit
            int totalClaims = await _context.UserSubscriptionPlans.CountAsync(usp => usp.PromocodeId == promo.Id);

            bool isSingleUse = promo.IsSingleUse ?? (promo.MaxRedemptions == 1);
            if (isSingleUse && totalClaims >= 1)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"This promo code '{cleanCode}' is a single-use code and has already been claimed."
                };
            }

            if (promo.MaxRedemptions.HasValue && promo.MaxRedemptions.Value > 0 && totalClaims >= promo.MaxRedemptions.Value)
            {
                return new RedeemPromocodeResponseDto
                {
                    Success = false,
                    Message = $"This promo code '{cleanCode}' has reached its maximum limit of {promo.MaxRedemptions.Value} redemptions."
                };
            }

            int downloadsToAdd = promo.NoOfFreeDownloads.GetValueOrDefault(0);
            int postingsToAdd = promo.NoOfFreeJobPosting.GetValueOrDefault(0);
            int chatLimitToSet = promo.DailyChatLimit.GetValueOrDefault(0);
            decimal discount = promo.DiscountAmount.GetValueOrDefault(0);

            var existingPlan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(usp => usp.UserId == targetUserId && usp.Active == true);

            DateTime effectiveEndDate = promo.EndDate ?? now.AddDays(30);

            if (existingPlan != null)
            {
                existingPlan.PromocodeId = promo.Id;
                if (postingsToAdd > 0)
                {
                    existingPlan.ActualJobPosting = (existingPlan.ActualJobPosting ?? 15) + postingsToAdd;
                }
                if (downloadsToAdd > 0)
                {
                    existingPlan.ActualDownloads = (existingPlan.ActualDownloads ?? 10) + downloadsToAdd;
                }
                if (chatLimitToSet > 0)
                {
                    existingPlan.DailyChatLimit = Math.Max(existingPlan.DailyChatLimit ?? 20, chatLimitToSet);
                }
                if (discount > 0)
                {
                    existingPlan.DiscountAmount = (existingPlan.DiscountAmount ?? 0) + discount;
                }
                if (promo.EndDate.HasValue && (existingPlan.EndDate == null || promo.EndDate.Value > existingPlan.EndDate.Value))
                {
                    existingPlan.EndDate = promo.EndDate.Value;
                }
                existingPlan.Updated = now;
                existingPlan.UpdatedBy = targetUserId;
            }
            else
            {
                var newPlan = new UserSubscriptionPlan
                {
                    UserId = targetUserId,
                    PromocodeId = promo.Id,
                    ActualJobPosting = 15 + postingsToAdd,
                    ActualDownloads = 10 + downloadsToAdd,
                    DailyChatLimit = chatLimitToSet > 0 ? chatLimitToSet : 20,
                    DiscountAmount = discount,
                    NoOfUsedDownloads = 0,
                    NoOfUsedJobPosting = 0,
                    NoOfUsers = 1,
                    IsFree = true,
                    Active = true,
                    StartDate = now,
                    EndDate = effectiveEndDate,
                    Updated = now,
                    UpdatedBy = targetUserId
                };
                _context.UserSubscriptionPlans.Add(newPlan);
            }

            await _context.SaveChangesAsync();

            var benefits = new List<string>();
            if (postingsToAdd > 0) benefits.Add($"+{postingsToAdd} Job Postings");
            if (downloadsToAdd > 0) benefits.Add($"+{downloadsToAdd} Resume Downloads");
            if (chatLimitToSet > 0) benefits.Add($"{chatLimitToSet} Daily Chats");
            if (discount > 0) benefits.Add($"${discount} Discount Credit");

            string benefitSummary = benefits.Count > 0 ? string.Join(", ", benefits) : "Standard rewards";

            return new RedeemPromocodeResponseDto
            {
                Success = true,
                Message = $"Promo code '{cleanCode}' redeemed successfully! Granted {benefitSummary}.",
                Promocode = cleanCode,
                FreeDownloadsGranted = downloadsToAdd,
                FreeJobPostingGranted = postingsToAdd,
                DailyChatLimitGranted = chatLimitToSet,
                DiscountAmount = discount,
                StartDate = promo.StartDate ?? now,
                EndDate = effectiveEndDate
            };
        }
    }
}
