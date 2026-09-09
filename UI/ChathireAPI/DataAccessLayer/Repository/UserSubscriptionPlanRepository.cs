using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IUserSubscriptionPlanRepository : IRepository<UserSubscriptionPlan, long>
    {
        Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanById(long? Id, UserContext userContext);
        Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext);
        

    }

    public class UserSubscriptionPlanRepository : BaseRepository<UserSubscriptionPlan, long>, IUserSubscriptionPlanRepository
    {
        public UserSubscriptionPlanRepository(EFContexts context) : base(context) { }

        public async Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanById(long? Id, UserContext userContext)
        {
            var userSubscriptionList = _context.UserSubscriptionPlans
               .Include(o => o.SubscriptionPlan)
               .ThenInclude(o=>o.SubscriptionPlanFeatures)
               .Include(o => o.User)
            .Where(c => c.Id == Id);

            return await userSubscriptionList.ToListAsync();
        }

        public async Task<List<UserSubscriptionPlan>> GetUserSubscriptionPlanByUserId(long? Id, UserContext userContext)
        {
            var userSubscriptionList = _context.UserSubscriptionPlans
               .Include(o => o.SubscriptionPlan)
               .ThenInclude(o => o.SubscriptionPlanFeatures)
               .Include(o => o.User)
            .Where(c => c.UserId == Id);

            return await userSubscriptionList.ToListAsync();
        }
    }
}
