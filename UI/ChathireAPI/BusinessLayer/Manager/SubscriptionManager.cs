using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Manager
{
    public interface ISubscriptionManager
    {
        Task<List<BusinessEntityAndDTO.DTO.SubscriptionPlanDto>> GetAllSubscriptionPlans(UserContext userContext);
        Task<List<BusinessEntityAndDTO.DTO.UserSubscriptionPlanDto>> GetUserSubscriptionPlanById(long Id, UserContext userContext);
        Task<List<UserSubscriptionPlanDto>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext);
        Task<UserSubscriptionPlanDto> AssignSubscriptionToUser(AssignSubscriptionDto assignSubscription, UserContext userContext);
    }
    public class SubscriptionManager : BaseManager<SubscriptionManager>, ISubscriptionManager
    {
        public SubscriptionManager(IServiceProvider provider, ILogger<SubscriptionManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }
        public async Task<List<SubscriptionPlanDto>> GetAllSubscriptionPlans(UserContext userContext)
        {
            return await ExecuteAsync<List<SubscriptionPlanDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ISubscriptionRepository>();
                return mapper.Map<List<SubscriptionPlanDto>>(await repo.GetAllSubscriptionPlan(userContext));
            }, "GetAllJobOpening", userContext);
        }

        public async Task<SubscriptionPlanDto> GetSubscriptionPlanById(long id, UserContext userContext)
        {
            return await ExecuteAsync<SubscriptionPlanDto>(async () =>
            {
                var repo = repositoryFactory.Get<ISubscriptionRepository>();
                return mapper.Map<SubscriptionPlanDto>(await repo.GetSubscriptionPlanById(id, userContext));
            }, "GetAllJobOpening", userContext);
        }

        public async Task<List<UserSubscriptionPlanDto>> GetUserSubscriptionPlanById(long Id, UserContext userContext)
        {
            return await ExecuteAsync<List<UserSubscriptionPlanDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserSubscriptionPlanRepository>();
                return mapper.Map<List<UserSubscriptionPlanDto>>(await repo.GetUserSubscriptionPlanById(Id, userContext));
            }, "GetUserSubscriptionPlanById", userContext);

        }
        public async Task<List<UserSubscriptionPlanDto>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext)
        {
            return await ExecuteAsync<List<UserSubscriptionPlanDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserSubscriptionPlanRepository>();

                if (Id.GetValueOrDefault(0) == 0)
                    Id = userContext.UserId;

                return mapper.Map<List<UserSubscriptionPlanDto>>(await repo.GetUserSubscriptionPlanByUserId(Id, userContext));
            }, "GetUserSubscriptionPlanById", userContext);

        }

        public async Task<UserSubscriptionPlanDto> AssignSubscriptionToUser(AssignSubscriptionDto assignSubscription, UserContext userContext)
        {
            var subscriptionPlan = await GetSubscriptionPlanById(assignSubscription.SubscriptionPlanId, userContext);

            var userSubPlan = await GetUserSubscriptionPlanByUserId(assignSubscription.UserId, userContext);
            UserSubscriptionPlan _userSubscriptionPlan = new UserSubscriptionPlan();

            if (userSubPlan != null && userSubPlan.Count > 0)
            {
                _userSubscriptionPlan = mapper.Map<UserSubscriptionPlan>(userSubPlan.Where(o => o.SubscriptionPlanId == assignSubscription.SubscriptionPlanId).First());
            }

            if (_userSubscriptionPlan.Id == 0)
            {
                var result = await ExecuteAsync<UserSubscriptionPlan>(async () =>
                {
                    var date = DateTime.UtcNow;
                    _userSubscriptionPlan.UserId = assignSubscription.UserId;
                    _userSubscriptionPlan.SubscriptionPlanId = assignSubscription.SubscriptionPlanId;

                    var repo = repositoryFactory.Get<IUserSubscriptionPlanRepository>();
                    _userSubscriptionPlan.Updated = date;
                    _userSubscriptionPlan.UpdatedBy = userContext.UserId;
                    _userSubscriptionPlan.Active = true;
                    _userSubscriptionPlan.ActualDownloads = subscriptionPlan.NoOfDownloads;
                    _userSubscriptionPlan.ActualJobPosting = subscriptionPlan.NoOfJobPosting;
                    _userSubscriptionPlan.NoOfUsedDownloads = 0;
                    _userSubscriptionPlan.NoOfUsedJobPosting = 0;
                    _userSubscriptionPlan.NoOfUsers = subscriptionPlan.NoOfUsers;
                    _userSubscriptionPlan.Amount = subscriptionPlan.Amount;
                    _userSubscriptionPlan.DiscountAmount = subscriptionPlan.DiscountAmount;
                    _userSubscriptionPlan.StartDate = date;
                    _userSubscriptionPlan.EndDate = date.AddDays(Convert.ToInt64(subscriptionPlan.ValidityInDays));

                    return await repo.Post(_userSubscriptionPlan, true);
                }, "AssignSubscription", userContext);
                return mapper.Map<UserSubscriptionPlanDto>(result);
            }
            else
                return mapper.Map<UserSubscriptionPlanDto>(_userSubscriptionPlan);
        }
    }
}
