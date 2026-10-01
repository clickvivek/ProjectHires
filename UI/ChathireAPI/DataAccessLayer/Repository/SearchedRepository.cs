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
    public interface ISearchedRepository : IRepository<Searched, long>
    {
        Task LogSearchAsync(string searchType, IEnumerable<string>? searchStrings, string? location, IEnumerable<int>? cityIds, int totalResults, long? userId, string? ipAddress, string? extraFilters = null);
        Task<List<Searched>> GetRecentSearchesAsync(string? searchType, int page, int pageSize);
        Task<int> GetSearchesCountAsync(string? searchType);
        Task EnsureTableExistsAsync();
    }

    public class SearchedRepository : BaseRepository<Searched, long>, ISearchedRepository
    {
        private static bool _tableChecked = false;
        private static readonly object _tableLock = new object();

        public SearchedRepository(EFContexts context) : base(context) { }

        public async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;

            try
            {
                string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Searched]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Searched](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [SearchType] [varchar](50) NOT NULL,
        [Keywords] [nvarchar](500) NULL,
        [Location] [nvarchar](200) NULL,
        [Filters] [nvarchar](1000) NULL,
        [TotalResults] [int] NOT NULL CONSTRAINT [DF_Searched_TotalResults] DEFAULT ((0)),
        [UserId] [bigint] NULL,
        [IpAddress] [varchar](100) NULL,
        [CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_Searched_CreatedDate] DEFAULT (getutcdate()),
        [Active] [bit] NOT NULL CONSTRAINT [DF_Searched_Active] DEFAULT ((1)),
        [Updated] [datetime] NULL,
        [UpdatedBy] [bigint] NULL,
        CONSTRAINT [PK_Searched] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Searched]') AND name = 'Updated')
    BEGIN
        ALTER TABLE [dbo].[Searched] ADD [Updated] [datetime] NULL;
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Searched]') AND name = 'UpdatedBy')
    BEGIN
        ALTER TABLE [dbo].[Searched] ADD [UpdatedBy] [bigint] NULL;
    END
END";
                await _context.Database.ExecuteSqlRawAsync(sql);
                _tableChecked = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SearchedRepository] Table check notice: {ex.Message}");
            }
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
                await EnsureTableExistsAsync();

                // Format keywords
                string keywords = string.Empty;
                if (searchStrings != null && searchStrings.Any())
                {
                    keywords = string.Join(", ", searchStrings.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
                }

                // If no keywords and no location, don't clutter with empty initial loads unless relevant
                if (string.IsNullOrWhiteSpace(keywords) && string.IsNullOrWhiteSpace(location) && (cityIds == null || !cityIds.Any()) && string.IsNullOrWhiteSpace(extraFilters))
                {
                    keywords = "All (No Keywords)";
                }

                // Format location
                string locationStr = location?.Trim() ?? string.Empty;
                if (cityIds != null && cityIds.Any() && string.IsNullOrEmpty(locationStr))
                {
                    locationStr = $"CityIds: {string.Join(",", cityIds)}";
                }

                var record = new Searched
                {
                    SearchType = Truncate(searchType, 50),
                    Keywords = Truncate(keywords, 500),
                    Location = Truncate(locationStr, 200),
                    Filters = Truncate(extraFilters, 1000),
                    TotalResults = totalResults,
                    UserId = userId > 0 ? userId : null,
                    IpAddress = Truncate(ipAddress, 100),
                    CreatedDate = DateTime.UtcNow,
                    Active = true
                };

                _context.Searcheds.Add(record);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Silent catch to prevent search logging from impacting core search API functionality
            }
        }

        public async Task<List<Searched>> GetRecentSearchesAsync(string? searchType, int page, int pageSize)
        {
            await EnsureTableExistsAsync();

            var query = _context.Searcheds.AsNoTracking().Where(s => s.Active);

            if (!string.IsNullOrWhiteSpace(searchType) && !searchType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.SearchType == searchType.Trim());
            }

            int skip = Math.Max(0, (page - 1) * pageSize);
            return await query
                .OrderByDescending(s => s.CreatedDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSearchesCountAsync(string? searchType)
        {
            await EnsureTableExistsAsync();

            var query = _context.Searcheds.AsNoTracking().Where(s => s.Active);

            if (!string.IsNullOrWhiteSpace(searchType) && !searchType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.SearchType == searchType.Trim());
            }

            return await query.CountAsync();
        }

        private static string Truncate(string? val, int maxLen)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            return val.Length <= maxLen ? val : val.Substring(0, maxLen);
        }
    }
}
