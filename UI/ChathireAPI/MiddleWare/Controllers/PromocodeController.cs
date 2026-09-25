using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Security;
using Middleware.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromocodeController : BaseCtrler<PromocodeController>
    {
        public PromocodeController(IServiceProvider serviceProvider, ILogger<PromocodeController> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        [HttpGet]
        [Route("All")]
        public Task<Result<List<PromocodeDto>>> GetAllPromocodes()
        {
            return ExecuteAsync<List<PromocodeDto>>(async () =>
            {
                var mgr = managerFactory.Get<IPromocodeManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.GetAllPromocodes(context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Create")]
        public Task<Result<PromocodeDto>> CreatePromocode([FromBody] CreatePromocodeDto dto)
        {
            return ExecuteAsync<PromocodeDto>(async () =>
            {
                var mgr = managerFactory.Get<IPromocodeManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.CreatePromocode(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<Result<bool>> DeletePromocode(long id)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IPromocodeManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.DeletePromocode(id, context ?? GetDummyUserContext());
            });
        }

        [HttpPut]
        [Route("{id}/toggle-status")]
        public Task<Result<bool>> ToggleActive(long id)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IPromocodeManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.ToggleActive(id, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Redeem")]
        public Task<Result<RedeemPromocodeResponseDto>> RedeemPromocode([FromBody] RedeemPromocodeDto dto)
        {
            return ExecuteAsync<RedeemPromocodeResponseDto>(async () =>
            {
                var mgr = managerFactory.Get<IPromocodeManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                if (dto.UserId.GetValueOrDefault(0) <= 0 && context != null && context.UserId > 0)
                {
                    dto.UserId = context.UserId;
                }
                return await mgr.RedeemPromocode(dto, context ?? GetDummyUserContext());
            });
        }
    }
}
