using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IEmailJobPostingRepository : IRepository<EmailJobPostingQueue, long>
    {
        Task<List<EmailJobPostingQueueDto>> GetAllAsync(EmailJobPostingFilterDto filter, UserContext userContext);
        Task<int> GetTotalCountAsync(EmailJobPostingFilterDto filter, UserContext userContext);
        Task<EmailJobPostingStatsDto> GetStatsAsync(UserContext userContext);
        Task<EmailJobPostingQueue?> GetByIdAsync(long id, UserContext userContext);
        Task<EmailJobPostingQueue> InsertQueueItemAsync(EmailJobPostingQueue item, UserContext userContext);
        Task<bool> UpdateQueueItemAsync(EmailJobPostingQueue item, UserContext userContext);
        Task<bool> DeleteQueueItemAsync(long id, UserContext userContext);
    }

    public class EmailJobPostingRepository : BaseRepository<EmailJobPostingQueue, long>, IEmailJobPostingRepository
    {
        public EmailJobPostingRepository(EFContexts context) : base(context) { }

        public async Task<List<EmailJobPostingQueueDto>> GetAllAsync(EmailJobPostingFilterDto filter, UserContext userContext)
        {
            var query = _context.EmailJobPostingQueues.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status.ToLower() != "all")
            {
                query = query.Where(q => q.Status.ToLower() == filter.Status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string s = filter.Search.Trim().ToLower();
                query = query.Where(q => q.SenderEmail.ToLower().Contains(s)
                    || (q.SenderName != null && q.SenderName.ToLower().Contains(s))
                    || (q.EmailSubject != null && q.EmailSubject.ToLower().Contains(s)));
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(q => q.ReceivedDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(q => q.ReceivedDate <= filter.ToDate.Value);
            }

            int skip = (filter.Page - 1) * filter.PageSize;
            var list = await query
                .OrderByDescending(q => q.ReceivedDate)
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var userIds = list.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = ((u.Fname ?? "") + " " + (u.Lname ?? "")).Trim(), u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            var result = list.Select(item =>
            {
                ParsedJobDataDto? parsedJob = null;
                if (!string.IsNullOrWhiteSpace(item.ParsedJobJson))
                {
                    try
                    {
                        parsedJob = JsonSerializer.Deserialize<ParsedJobDataDto>(item.ParsedJobJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch { }
                }

                return new EmailJobPostingQueueDto
                {
                    Id = item.Id,
                    SenderEmail = item.SenderEmail,
                    SenderName = item.SenderName,
                    EmailSubject = item.EmailSubject,
                    RawEmailBodyText = item.RawEmailBodyText,
                    RawEmailBodyHtml = item.RawEmailBodyHtml,
                    UserId = item.UserId,
                    RecruiterName = item.UserId.HasValue && users.ContainsKey(item.UserId.Value) ? users[item.UserId.Value] : item.SenderName,
                    ConsultancyId = item.ConsultancyId,
                    MatchedEmailType = item.MatchedEmailType,
                    Status = item.Status,
                    ParsedJobJson = item.ParsedJobJson,
                    ParsedJob = parsedJob,
                    CreatedJobOpeningId = item.CreatedJobOpeningId,
                    ErrorMessage = item.ErrorMessage,
                    RetryCount = item.RetryCount,
                    ReceivedDate = item.ReceivedDate,
                    ProcessedDate = item.ProcessedDate,
                    UpdatedBy = item.UpdatedBy
                };
            }).ToList();

            return result;
        }

        public async Task<int> GetTotalCountAsync(EmailJobPostingFilterDto filter, UserContext userContext)
        {
            var query = _context.EmailJobPostingQueues.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status.ToLower() != "all")
            {
                query = query.Where(q => q.Status.ToLower() == filter.Status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string s = filter.Search.Trim().ToLower();
                query = query.Where(q => q.SenderEmail.ToLower().Contains(s)
                    || (q.SenderName != null && q.SenderName.ToLower().Contains(s))
                    || (q.EmailSubject != null && q.EmailSubject.ToLower().Contains(s)));
            }

            return await query.CountAsync();
        }

        public async Task<EmailJobPostingStatsDto> GetStatsAsync(UserContext userContext)
        {
            var all = await _context.EmailJobPostingQueues
                .AsNoTracking()
                .GroupBy(q => q.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var stats = new EmailJobPostingStatsDto();
            foreach (var item in all)
            {
                stats.TotalReceived += item.Count;
                switch (item.Status.ToLower())
                {
                    case "published": stats.PublishedCount += item.Count; break;
                    case "parsed": stats.ParsedCount += item.Count; break;
                    case "failed": stats.FailedCount += item.Count; break;
                    case "quotaexceeded": stats.QuotaExceededCount += item.Count; break;
                    case "rejected": stats.RejectedCount += item.Count; break;
                }
            }
            return stats;
        }

        public async Task<EmailJobPostingQueue?> GetByIdAsync(long id, UserContext userContext)
        {
            return await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<EmailJobPostingQueue> InsertQueueItemAsync(EmailJobPostingQueue item, UserContext userContext)
        {
            _context.EmailJobPostingQueues.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateQueueItemAsync(EmailJobPostingQueue item, UserContext userContext)
        {
            _context.EmailJobPostingQueues.Update(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteQueueItemAsync(long id, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == id);
            if (item == null) return false;
            _context.EmailJobPostingQueues.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
