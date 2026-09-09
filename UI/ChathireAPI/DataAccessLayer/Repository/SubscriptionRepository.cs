using BusinessEntityAndDTO.Common;
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

    public interface ISubscriptionRepository : IRepository<SubscriptionPlan, long>
    {
        Task<List<SubscriptionPlan>> GetAllSubscriptionPlan(UserContext userContext);
        Task<SubscriptionPlan> GetSubscriptionPlanById(long id, UserContext userContext);

    }
    public class SubscriptionRepository : BaseRepository<SubscriptionPlan, long>, ISubscriptionRepository
    {
        public SubscriptionRepository(EFContexts context) : base(context) { }

        public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlan(UserContext userContext)
        {
            var jobSubscriptionList = _context.SubscriptionPlans
               .Include(o => o.SubscriptionPlanFeatures);
            //.Where(c => c.ConsultancyUser.ConsultancyId == ConsultancyId);

            return await jobSubscriptionList.ToListAsync();
        }
        public async Task<SubscriptionPlan> GetSubscriptionPlanById(long id, UserContext userContext)
        {
            var jobSubscriptionList = _context.SubscriptionPlans
               .Include(o => o.SubscriptionPlanFeatures)
            .Where(o =>o.Id.Equals(id));
            
            return await jobSubscriptionList.FirstAsync();
        }
    }
}
