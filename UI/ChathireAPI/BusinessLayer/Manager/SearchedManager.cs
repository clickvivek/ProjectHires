using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface ISearchedManager : IManager
    {
        Task LogSearchAsync(string searchType, IEnumerable<string>? searchStrings, string? location, IEnumerable<int>? cityIds, int totalResults, long? userId, string? ipAddress, string? extraFilters = null);
        Task<List<SearchedDto>> GetRecentSearchesAsync(string? searchType, int page = 1, int pageSize = 50);
        Task<int> GetSearchesCountAsync(string? searchType);
        Task<SearchedAnalyticsSummaryDto> GetSearchAnalyticsSummaryAsync();
    }

    public class SearchedManager : BaseManager<SearchedManager>, ISearchedManager
    {
        public SearchedManager(IServiceProvider serviceProvider, ILogger<SearchedManager> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        public async Task LogSearchAsync(
            string searchType,
            IEnumerable<string>? searchStrings,
            string? location,
            IEnumerable<int>? cityIds,
            int totalResults,
            long? userId,
            string? ipAddress,
            string? extraFilters = null)
        {
            try
            {
                var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
                if (scopeFactory != null)
                {
                    using var scope = scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ISearchedRepository>();
                    await repo.LogSearchAsync(searchType, searchStrings, location, cityIds, totalResults, userId, ipAddress, extraFilters);
                }
                else
                {
                    var searchedRepository = repositoryFactory.Get<ISearchedRepository>();
                    await searchedRepository.LogSearchAsync(searchType, searchStrings, location, cityIds, totalResults, userId, ipAddress, extraFilters);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error logging search for {SearchType}", searchType);
            }
        }

        public async Task<List<SearchedDto>> GetRecentSearchesAsync(string? searchType, int page = 1, int pageSize = 50)
        {
            var searchedRepository = repositoryFactory.Get<ISearchedRepository>();
            var context = serviceProvider.GetService<EFContexts>();
            var entities = await searchedRepository.GetRecentSearchesAsync(searchType, page, pageSize);
            
            // Map to DTOs and populate user details if available
            var userIds = entities.Where(e => e.UserId.HasValue).Select(e => e.UserId!.Value).Distinct().ToList();
            var usersMap = new Dictionary<long, (string Email, string Name)>();

            if (userIds.Any() && context != null)
            {
                try
                {
                    var users = await context.Users
                        .AsNoTracking()
                        .Where(u => userIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.Email, Name = (u.Fname + " " + (u.Lname ?? "")).Trim() })
                        .ToListAsync();

                    foreach (var u in users)
                    {
                        usersMap[u.Id] = (u.Email ?? "", u.Name);
                    }
                }
                catch { }
            }

            var dtos = entities.Select(e =>
            {
                var dto = new SearchedDto
                {
                    Id = e.Id,
                    SearchType = e.SearchType,
                    Keywords = e.Keywords,
                    Location = e.Location,
                    Filters = e.Filters,
                    TotalResults = e.TotalResults,
                    UserId = e.UserId,
                    IpAddress = e.IpAddress,
                    CreatedDate = e.CreatedDate,
                    Active = e.Active
                };

                if (e.UserId.HasValue && usersMap.TryGetValue(e.UserId.Value, out var uInfo))
                {
                    dto.UserEmail = uInfo.Email;
                    dto.UserName = uInfo.Name;
                }

                return dto;
            }).ToList();

            return dtos;
        }

        public async Task<int> GetSearchesCountAsync(string? searchType)
        {
            var searchedRepository = repositoryFactory.Get<ISearchedRepository>();
            return await searchedRepository.GetSearchesCountAsync(searchType);
        }

        public async Task<SearchedAnalyticsSummaryDto> GetSearchAnalyticsSummaryAsync()
        {
            var searchedRepository = repositoryFactory.Get<ISearchedRepository>();
            var context = serviceProvider.GetService<EFContexts>();
            await searchedRepository.EnsureTableExistsAsync();

            var todayUtc = DateTime.UtcNow.Date;

            var summary = new SearchedAnalyticsSummaryDto();

            if (context != null)
            {
                try
                {
                    var allSearches = await context.Searcheds
                        .AsNoTracking()
                        .Where(s => s.Active)
                        .OrderByDescending(s => s.CreatedDate)
                        .Take(1000)
                        .ToListAsync();

                    summary.TotalSearches = allSearches.Count;
                    summary.TotalJobSearches = allSearches.Count(s => s.SearchType.Equals("JobSearch", StringComparison.OrdinalIgnoreCase) || s.SearchType.Equals("Jobs", StringComparison.OrdinalIgnoreCase));
                    summary.TotalHotlistSearches = allSearches.Count(s => s.SearchType.Equals("HotlistSearch", StringComparison.OrdinalIgnoreCase) || s.SearchType.Equals("Hotlist", StringComparison.OrdinalIgnoreCase));
                    summary.SearchesToday = allSearches.Count(s => s.CreatedDate >= todayUtc);
                    summary.ZeroResultSearches = allSearches.Count(s => s.TotalResults == 0);

                    // Top Job Keywords
                    summary.TopJobKeywords = allSearches
                        .Where(s => (s.SearchType.Equals("JobSearch", StringComparison.OrdinalIgnoreCase) || s.SearchType.Equals("Jobs", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(s.Keywords) && !s.Keywords.StartsWith("All", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(s => s.Keywords!.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(g => new TopSearchKeywordDto
                        {
                            Keyword = g.Key,
                            Count = g.Count(),
                            AvgResults = Math.Round(g.Average(x => x.TotalResults), 1)
                        })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .ToList();

                    // Top Hotlist Keywords
                    summary.TopHotlistKeywords = allSearches
                        .Where(s => (s.SearchType.Equals("HotlistSearch", StringComparison.OrdinalIgnoreCase) || s.SearchType.Equals("Hotlist", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(s.Keywords) && !s.Keywords.StartsWith("All", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(s => s.Keywords!.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(g => new TopSearchKeywordDto
                        {
                            Keyword = g.Key,
                            Count = g.Count(),
                            AvgResults = Math.Round(g.Average(x => x.TotalResults), 1)
                        })
                        .OrderByDescending(x => x.Count)
                        .Take(10)
                        .ToList();
                }
                catch { }
            }

            return summary;
        }
    }
}
