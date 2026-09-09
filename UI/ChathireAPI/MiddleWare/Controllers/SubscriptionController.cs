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
        EFContexts dbcontext = new EFContexts();

        public SubscriptionController(IServiceProvider serviceProvider, ILogger<SubscriptionController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
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
    }
}