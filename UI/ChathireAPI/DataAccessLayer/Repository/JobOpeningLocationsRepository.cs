using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repository
{
    public interface IJobOpeningLocationsRepository : IRepository<JobOpeningLocation, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
        Task<List<JobOpeningLocation>> GetJobOpeningLocationByJobId(long JobId);
        Task<List<JobOpeningLocation>> GetJobOpeningLocationsByJobIds(List<long> JobIds);
    }
    public class JobOpeningLocationsRepository : BaseRepository<JobOpeningLocation, long>, IJobOpeningLocationsRepository
    {
        public JobOpeningLocationsRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange(_context.JobOpeningLocations.Where(a => a.JobOpeningId == id).ToList(), userContext);
        }

        public async Task<List<JobOpeningLocation>> GetJobOpeningLocationByJobId(long JobId)
        {
            var jobOpeningLocations =  _context.JobOpeningLocations 
                   .Include(o => o.City)
                   .ThenInclude(o=>o.IdStateNavigation)
                   .Where(c => c.JobOpeningId == JobId);

            return await jobOpeningLocations.ToListAsync();
        }

        public async Task<List<JobOpeningLocation>> GetJobOpeningLocationsByJobIds(List<long> JobIds)
        {
            if (JobIds == null || !JobIds.Any())
            {
                return new List<JobOpeningLocation>();
            }

            var jobOpeningLocations = _context.JobOpeningLocations
                   .Include(o => o.City)
                   .ThenInclude(o => o.IdStateNavigation)
                   .Where(c => JobIds.Contains(c.JobOpeningId));

            return await jobOpeningLocations.ToListAsync();
        }

    }
}

