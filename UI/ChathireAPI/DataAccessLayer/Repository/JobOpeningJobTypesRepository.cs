using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;


namespace DataAccessLayer.Repository
{
    public interface IJobOpeningJobTypesRepository : IRepository<JobOpeningJobType, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class JobOpeningJobTypesRepository : BaseRepository<JobOpeningJobType, long>, IJobOpeningJobTypesRepository
    {
        public JobOpeningJobTypesRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange(_context.JobOpeningJobTypes.Where(a => a.JobOpeningId == id).ToList(), userContext);
        }
    }
}
