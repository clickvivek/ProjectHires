using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;


namespace DataAccessLayer.Repository
{
    public interface IJobOpeningVisaMapsRepository : IRepository<JobOpeningVisaMap, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class JobOpeningVisaMapsRepository : BaseRepository<JobOpeningVisaMap, long>, IJobOpeningVisaMapsRepository
    {
        public JobOpeningVisaMapsRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange(_context.JobOpeningVisaMaps.Where(a => a.JobOpeningId == id).ToList(), userContext);
        }
    }
}

