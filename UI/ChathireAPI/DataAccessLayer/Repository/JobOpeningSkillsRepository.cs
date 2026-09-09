using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;


namespace DataAccessLayer.Repository
{
    public interface IJobOpeningSkillsRepository : IRepository<JobOpeningSkill, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class JobOpeningSkillsRepository : BaseRepository<JobOpeningSkill, long>, IJobOpeningSkillsRepository
    {
        public JobOpeningSkillsRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange(_context.JobOpeningSkills.Where(a => a.JobId == id).ToList(), userContext);
        }
    }
}

