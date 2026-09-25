using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface IPromocodeManager
    {
        Task<List<PromocodeDto>> GetAllPromocodes(UserContext userContext);
        Task<PromocodeDto> CreatePromocode(CreatePromocodeDto dto, UserContext userContext);
        Task<bool> DeletePromocode(long id, UserContext userContext);
        Task<bool> ToggleActive(long id, UserContext userContext);
        Task<RedeemPromocodeResponseDto> RedeemPromocode(RedeemPromocodeDto dto, UserContext userContext);
    }

    public class PromocodeManager : BaseManager<PromocodeManager>, IPromocodeManager
    {
        public PromocodeManager(IServiceProvider provider, ILogger<PromocodeManager> logger, IMapper mapper)
            : base(provider, logger, mapper)
        {
        }

        public async Task<List<PromocodeDto>> GetAllPromocodes(UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IPromocodeRepository>();
                return await repo.GetAllPromocodes(userContext);
            }, "GetAllPromocodes", userContext);
        }

        public async Task<PromocodeDto> CreatePromocode(CreatePromocodeDto dto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IPromocodeRepository>();
                return await repo.CreatePromocode(dto, userContext);
            }, "CreatePromocode", userContext);
        }

        public async Task<bool> DeletePromocode(long id, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IPromocodeRepository>();
                return await repo.DeletePromocode(id, userContext);
            }, "DeletePromocode", userContext);
        }

        public async Task<bool> ToggleActive(long id, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IPromocodeRepository>();
                return await repo.ToggleActive(id, userContext);
            }, "ToggleActive", userContext);
        }

        public async Task<RedeemPromocodeResponseDto> RedeemPromocode(RedeemPromocodeDto dto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IPromocodeRepository>();
                return await repo.RedeemPromocode(dto, userContext);
            }, "RedeemPromocode", userContext);
        }
    }
}
