using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface IConfigManager
    {
        Task<string> GetConfigValue(string key, string defaultValue = "");
        Task<int> GetConfigInt(string key, int defaultValue = 0);
        Task<bool> SetConfigValue(string key, string value, string? description = null, UserContext? userContext = null);
        Task<List<Config>> GetAllConfigs(UserContext? userContext = null);
        Task EnsureDefaultConfigs();
    }

    public class ConfigManager : BaseManager<ConfigManager>, IConfigManager
    {
        public ConfigManager(IServiceProvider provider, ILogger<ConfigManager> logger, IMapper mapper)
            : base(provider, logger, mapper)
        {
        }

        public async Task<string> GetConfigValue(string key, string defaultValue = "")
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IConfigRepository>();
                return await repo.GetConfigValueAsync(key, defaultValue);
            }, "GetConfigValue", null);
        }

        public async Task<int> GetConfigInt(string key, int defaultValue = 0)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IConfigRepository>();
                return await repo.GetConfigIntAsync(key, defaultValue);
            }, "GetConfigInt", null);
        }

        public async Task<bool> SetConfigValue(string key, string value, string? description = null, UserContext? userContext = null)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IConfigRepository>();
                return await repo.SetConfigValueAsync(key, value, description, userContext);
            }, "SetConfigValue", userContext);
        }

        public async Task<List<Config>> GetAllConfigs(UserContext? userContext = null)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IConfigRepository>();
                return await repo.GetAllConfigsAsync();
            }, "GetAllConfigs", userContext);
        }

        public async Task EnsureDefaultConfigs()
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IConfigRepository>();
                await repo.EnsureDefaultConfigsAsync();
                return true;
            }, "EnsureDefaultConfigs", null);
        }
    }
}
