using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;


namespace DataAccessLayer.Repository
{
    public interface IJobOpeningEmploymentTypesRepository : IRepository<JobOpeningEmploymentType, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class JobOpeningEmploymentTypesRepository : BaseRepository<JobOpeningEmploymentType, long>, IJobOpeningEmploymentTypesRepository
    {
        public JobOpeningEmploymentTypesRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange(_context.JobOpeningEmploymentTypes.Where(a => a.JobOpeningId == id).ToList(), userContext);
        }
    }
}

