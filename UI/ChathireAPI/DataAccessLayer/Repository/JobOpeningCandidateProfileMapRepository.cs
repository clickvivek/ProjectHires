using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
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

    public interface IJobOpeningCandidateProfileMapRepository : IRepository<JobOpeningCandidateProfileMap, long>
    {
        Task<List<JobOpeningCandidateProfileMap>> GetAllResumeReceivedByJobId(long JobOpeningId);
        Task<JobOpeningCandidateProfileMap> GetJobOpeningCandidateProfileMapById(long Id, UserContext userContext);


    }
    public class JobOpeningCandidateProfileMapRepository : BaseRepository<JobOpeningCandidateProfileMap, long>, IJobOpeningCandidateProfileMapRepository
    {
        public JobOpeningCandidateProfileMapRepository(EFContexts context) : base(context) { }

        public async Task<List<JobOpeningCandidateProfileMap>> GetAllResumeReceivedByJobId(long JobOpeningId)
        {
            var jobOpeningResumeList = _context.JobOpeningCandidateProfileMaps
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileSkills)
              .ThenInclude(o=>o.Skill)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidatePrefJobTypes)
              .ThenInclude(o=>o.JobType)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidatePrefLocations)
              .ThenInclude(o=>o.City)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileDomains)
              .ThenInclude(o=>o.Domain)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileEmploymentTypes)
              .ThenInclude(o=>o.EmploymentType)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateDocuments)
              .Include(o => o.CandidateProfileMappingStatus)
              .Include(o => o.ConsultancyUser)
              .ThenInclude(o=>o.User)
              .Include(o => o.ConsultancyUser)
              .ThenInclude(o=>o.Consultancy)
              .Include(o=>o.CurrentLocationCity)
              .ThenInclude(o => o.IdStateNavigation)
              .Include(o => o.CandidateUser)
              .Where(c => c.JobOpeningId == JobOpeningId);

            return await jobOpeningResumeList.ToListAsync();

        }

        public async Task<JobOpeningCandidateProfileMap> GetJobOpeningCandidateProfileMapById(long Id, UserContext userContext)
        {
            var jobOpeningResumeList = _context.JobOpeningCandidateProfileMaps
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileSkills)
              .ThenInclude(o => o.Skill)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidatePrefJobTypes)
              .ThenInclude(o => o.JobType)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidatePrefLocations)
              .ThenInclude(o => o.City)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileDomains)
              .ThenInclude(o => o.Domain)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateProfileEmploymentTypes)
              .ThenInclude(o => o.EmploymentType)
              .Include(o => o.CandidateProfile)
              .ThenInclude(o => o.CandidateDocuments)
              .Include(o => o.CandidateProfileMappingStatus)
              .Include(o => o.ConsultancyUser)
              .ThenInclude(o => o.User)
              .Include(o => o.ConsultancyUser)
              .ThenInclude(o => o.Consultancy)
              .Include(o => o.CurrentLocationCity)
              .ThenInclude(o => o.IdStateNavigation)
              .Include(o => o.JobOpening)




              .Where(c => c.Id == Id);

            return await jobOpeningResumeList.FirstOrDefaultAsync();
        }
    }


}
