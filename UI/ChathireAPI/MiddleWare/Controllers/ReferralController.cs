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
using System.Threading.Tasks;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralController : BaseCtrler<ReferralController>
    {
        public ReferralController(IServiceProvider serviceProvider, ILogger<ReferralController> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        [HttpPost]
        [Route("Submit")]
        public Task<Result<SubmitReferralResponseDto>> SubmitReferrals([FromBody] SubmitReferralRequestDto dto)
        {
            return ExecuteAsync<SubmitReferralResponseDto>(async () =>
            {
                var mgr = managerFactory.Get<IUserReferralManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.SubmitReferrals(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Stats")]
        public Task<Result<ReferralStatsDto>> GetReferralStats()
        {
            return ExecuteAsync<ReferralStatsDto>(async () =>
            {
                var mgr = managerFactory.Get<IUserReferralManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.GetReferralStats(context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ProcessSignup")]
        public Task<Result<bool>> ProcessSignupReferral([FromBody] ProcessSignupReferralDto? body = null, [FromQuery] string? email = null, [FromQuery] string? referralCode = null, [FromQuery] long? newUserId = null)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserReferralManager>();
                string targetEmail = body?.Email ?? email ?? "";
                string? targetCode = !string.IsNullOrEmpty(body?.ReferralCode) ? body.ReferralCode : referralCode;
                long targetUserId = (body != null && body.NewUserId > 0) ? body.NewUserId : (newUserId ?? 0);

                return await mgr.ProcessSignupReferral(targetEmail, targetCode, targetUserId);
            });
        }
    }
}
