using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IConfigRepository : IRepository<Config, long>
    {
        Task<string> GetConfigValueAsync(string key, string defaultValue = "");
        Task<int> GetConfigIntAsync(string key, int defaultValue = 0);
        Task<bool> SetConfigValueAsync(string key, string value, string? description = null, UserContext? userContext = null);
        Task<List<Config>> GetAllConfigsAsync();
        Task EnsureDefaultConfigsAsync();
    }

    public class ConfigRepository : BaseRepository<Config, long>, IConfigRepository
    {
        // Thread-safe in-memory cache with timestamp to minimize DB queries for hot configs
        private static readonly ConcurrentDictionary<string, (string Value, DateTime CachedAt)> Cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public ConfigRepository(EFContexts context) : base(context) { }

        public async Task<string> GetConfigValueAsync(string key, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(key)) return defaultValue;

            string cleanKey = key.Trim();

            if (Cache.TryGetValue(cleanKey, out var entry) && (DateTime.UtcNow - entry.CachedAt) < CacheTtl)
            {
                return entry.Value;
            }

            try
            {
                var record = await _context.Configs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ConfigKey == cleanKey && (c.Active == true || c.Active == null));

                if (record != null && !string.IsNullOrWhiteSpace(record.ConfigValue))
                {
                    Cache[cleanKey] = (record.ConfigValue.Trim(), DateTime.UtcNow);
                    return record.ConfigValue.Trim();
                }
            }
            catch (Exception)
            {
                // Table might not exist yet or connection issue
            }

            return defaultValue;
        }

        public async Task<int> GetConfigIntAsync(string key, int defaultValue = 0)
        {
            var val = await GetConfigValueAsync(key, defaultValue.ToString());
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        public async Task<bool> SetConfigValueAsync(string key, string value, string? description = null, UserContext? userContext = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            string cleanKey = key.Trim();
            string cleanVal = (value ?? "").Trim();
            var now = DateTime.UtcNow;
            long userId = userContext?.UserId ?? -1;

            var existing = await _context.Configs.FirstOrDefaultAsync(c => c.ConfigKey == cleanKey);

            if (existing != null)
            {
                existing.ConfigValue = cleanVal;
                if (!string.IsNullOrWhiteSpace(description)) existing.Description = description;
                existing.Active = true;
                existing.Updated = now;
                existing.UpdatedBy = userId > 0 ? userId : null;
            }
            else
            {
                var newConfig = new Config
                {
                    ConfigKey = cleanKey,
                    ConfigValue = cleanVal,
                    Description = description,
                    Active = true,
                    CreatedDate = now,
                    Updated = now,
                    UpdatedBy = userId > 0 ? userId : null
                };
                _context.Configs.Add(newConfig);
            }

            await _context.SaveChangesAsync();
            Cache[cleanKey] = (cleanVal, DateTime.UtcNow);
            return true;
        }

        public async Task<List<Config>> GetAllConfigsAsync()
        {
            return await _context.Configs
                .AsNoTracking()
                .OrderBy(c => c.ConfigKey)
                .ToListAsync();
        }

        public async Task EnsureDefaultConfigsAsync()
        {
            try
            {
                // Ensure table exists via raw SQL if needed
                string createTableSql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Config]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Config](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [ConfigKey] [varchar](100) NOT NULL,
        [ConfigValue] [varchar](500) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [Active] [bit] NOT NULL CONSTRAINT [DF_Config_Active] DEFAULT ((1)),
        [CreatedDate] [datetime] NULL CONSTRAINT [DF_Config_CreatedDate] DEFAULT (getutcdate()),
        [Updated] [datetime] NULL,
        [UpdatedBy] [bigint] NULL,
        CONSTRAINT [PK_Config] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Config_ConfigKey] UNIQUE NONCLUSTERED ([ConfigKey] ASC)
    );
END";
                await _context.Database.ExecuteSqlRawAsync(createTableSql);

                var defaults = new List<(string Key, string Value, string Description)>
                {
                    ("InactiveChatPurgeDays", "30", "Number of inactive days after which TalkJS conversations and active chat sessions are purged / archived"),
                    ("DefaultDailyChatLimit", "20", "Default daily chat limit for free tier users within 24-hour sliding window"),
                    ("BonusReferralChatLimit", "10", "Bonus chat limit granted immediately upon 10 company referral submissions"),
                    ("BonusReferralDurationDays", "10", "Validity in days for bonus referral chat sessions")
                };

                var existingKeys = await _context.Configs
                    .Select(c => c.ConfigKey)
                    .ToListAsync();

                var existingSet = new HashSet<string>(existingKeys, StringComparer.OrdinalIgnoreCase);
                bool addedAny = false;

                foreach (var (key, value, desc) in defaults)
                {
                    if (!existingSet.Contains(key))
                    {
                        _context.Configs.Add(new Config
                        {
                            ConfigKey = key,
                            ConfigValue = value,
                            Description = desc,
                            Active = true,
                            CreatedDate = DateTime.UtcNow,
                            Updated = DateTime.UtcNow
                        });
                        addedAny = true;
                    }
                }

                if (addedAny)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Fallback logging if table creation was skipped or already present
                Console.WriteLine($"[ConfigRepository] EnsureDefaultConfigs notice: {ex.Message}");
            }
        }
    }
}
