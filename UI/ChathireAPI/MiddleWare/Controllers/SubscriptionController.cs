using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.Security;
using Middleware.Shared;
using Utility.Configuration;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : BaseCtrler<SubscriptionController>
    {
        EFContexts dbcontext;

        public SubscriptionController(IServiceProvider serviceProvider, ILogger<SubscriptionController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
            dbcontext = serviceProvider.GetService<EFContexts>();
        }

        [HttpGet]
        [Route("AllSubscriptionPlans")]
        public Task<Result<List<SubscriptionPlanDto>>> GetAllSubscriptionPlans()
        {
            return ExecuteAsync<List<SubscriptionPlanDto>>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();

                return await mgr.GetAllSubscriptionPlans(GetDummyUserContext());
            });
        }
        
        
        [HttpGet]
        [Route("UserSubscriptionPlanById")]
        public Task<Result<List<UserSubscriptionPlanDto>>> GetUserSubscriptionPlanById(long id)
        {
            return ExecuteAsync<List<UserSubscriptionPlanDto>>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();

                return await mgr.GetUserSubscriptionPlanById(id, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("UserSubscriptionPlanByUserId")]
        public Task<Result<List<UserSubscriptionPlanDto>>> GetUserSubscriptionPlanByUserId(long? userId)
        {
            return ExecuteAsync<List<UserSubscriptionPlanDto>>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();

                return await mgr.GetUserSubscriptionPlanByUserId(userId, GetDummyUserContext());
            });
        }


        [HttpPost]
        [Route("AssignSubscription")]
        //[ApiAuthorize("AssignSubscription")]
        public Task<Result<UserSubscriptionPlanDto>> AssignSubscriptionToUser(AssignSubscriptionDto assignSubscription)
        {
            return ExecuteAsync<UserSubscriptionPlanDto>(async () =>
            {
                var subscriptionManager = managerFactory.Get<ISubscriptionManager>();

                return await subscriptionManager.AssignSubscriptionToUser(assignSubscription, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("QuotaStatus")]
        public Task<Result<UserQuotaStatusDto>> GetQuotaStatus([FromQuery] long? userId)
        {
            return ExecuteAsync<UserQuotaStatusDto>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();
                long targetUserId = userId.GetValueOrDefault(0);
                UserContext context = null;
                if (TryGetUserId != null)
                {
                    try { context = GetUserContext(); } catch { }
                    if (targetUserId <= 0 && context != null)
                    {
                        targetUserId = context.UserId;
                    }
                }
                return await mgr.GetUserQuotaStatus(targetUserId, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("AllUserQuotas")]
        public Task<Result<UserQuotaListResponseDto>> GetAllUserQuotas([FromQuery] int page = 1, [FromQuery] int pageSize = 100, [FromQuery] string? search = null, [FromQuery] string? filter = null)
        {
            return ExecuteAsync<UserQuotaListResponseDto>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.GetAllUsersQuotas(page, pageSize, search, filter, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("UpdateUserQuota")]
        public Task<Result<bool>> UpdateUserQuota([FromBody] UpdateUserQuotaDto dto)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.UpdateUserQuota(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("InitiateChat")]
        public Task<Result<InitiateChatResultDto>> InitiateChat([FromBody] InitiateChatRequestDto dto)
        {
            return ExecuteAsync<InitiateChatResultDto>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();
                UserContext context = null;
                long userId = 0;
                try
                {
                    context = GetUserContext();
                    if (context != null)
                    {
                        userId = context.UserId;
                    }
                }
                catch { }

                if (userId <= 0 && dto?.UserId > 0)
                {
                    userId = dto.UserId.Value;
                }

                return await mgr.InitiateChat(userId, dto?.ChatUserId ?? 0, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("CheckDownloadResume")]
        public Task<Result<CanDownloadResumeResultDto>> CheckDownloadResume([FromQuery] string fileName, [FromQuery] long? userId = null)
        {
            return ExecuteAsync<CanDownloadResumeResultDto>(async () =>
            {
                var mgr = managerFactory.Get<ISubscriptionManager>();
                UserContext context = null;
                long targetUserId = 0;
                try
                {
                    context = GetUserContext();
                    if (context != null) targetUserId = context.UserId;
                }
                catch { }

                if (targetUserId <= 0 && userId.HasValue && userId.Value > 0)
                {
                    targetUserId = userId.Value;
                }

                return await mgr.CheckAndRecordDownloadAsync(targetUserId, fileName, context ?? GetDummyUserContext());
            });
        }
    }
}