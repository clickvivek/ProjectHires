using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Diagnostics;
using Utility.DataAccess;

namespace DataAccessLayer.Repository
{
    public interface IJobOpeningRepository : IRepository<JobOpening, long>
    {
        //Task<long> CreateJobOpeningAsync(JobOpeningDto opening, UserContext userContext);

        Task<List<JobOpening>> GetJobOpeningsByConsultancyID(long ConsultancyId);

        Task<List<JobOpening>> GetJobOpeningsByConsultancyUserID(long ConsultancyUserId, string? publicprofileID);

        Task<List<JobOpeningProfileConsultancyComment>> GetJobOpeningProfileConsultancyComment(long JobopeningCandidateProfileMapId, UserContext userContext);
        List<JobOpeningForSearchResultsDto> SearchJobOpenings(JobOpeningForSearchDto job);

        public List<JobOpeningProfileMapSummary1> GetJobOpeningProfileMapSummary(long ConsultancyUserId);
        Task<JobOpening> GetJobOpeningsById(long? Id, UserContext userContext);

        Task<List<JobOpeningSummary>> GetCountJobsPostedByConsultancyUserId(long ConsultancyUserId, DateTime FromDate, DateTime ToDate);
        Task<List<JobOpeningSummary>> GetCountActiveJobsAsOfToday(long ConsultancyUserId, UserContext userContext);
        Task<int> ExpireOldJobOpeningsAsync(int daysThreshold = 30);
        Task<RecruiterStatsDto> GetRecruiterStats(long consultancyUserId, UserContext userContext);

    }

    public class JobOpeningRepository : BaseRepository<JobOpening, long>, IJobOpeningRepository
    {
        //protected IMapper mapper;

        public JobOpeningRepository(EFContexts context) : base(context) { }

        public static readonly SqlMetaData[] sqlMetaDataSearchString = new[]
          {
                  new SqlMetaData("SearchString", SqlDbType.VarChar,50),

                };

        public List<JobOpeningForSearchResultsDto> SearchJobOpenings(JobOpeningForSearchDto job)
        { 
            List<JobOpeningForSearchResultsDto> rtn = new List<JobOpeningForSearchResultsDto>();


            List<SqlParameter> parameters = new List<SqlParameter>();

            if (job.searchStrings != null)
            {
                SqlParameter param = new SqlParameter("@SearchString", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_StringType]",
                    Value = Util.PopulateDataTable(job.searchStrings.Cast<object>().ToList(), typeof(string))
                };
                parameters.Add(param);
            }

            if (job.cityIds != null)
            {
                SqlParameter param = new SqlParameter("@CityId", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_IntType]",
                    Value = Util.PopulateDataTable(job.cityIds.Cast<object>().ToList(), typeof(int))
                };
                parameters.Add(param);
            }

            if (job.skills != null)
            {
                SqlParameter param = new SqlParameter("@Skills", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_IntType]",
                    Value = Util.PopulateDataTable(job.skills.Cast<object>().ToList(), typeof(int))
                };
                parameters.Add(param);
            }

            if (job.visas != null)
            {
                SqlParameter param = new SqlParameter("@Visa", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_IntType]",
                    Value = Util.PopulateDataTable(job.visas.Cast<object>().ToList(), typeof(int))
                };
                parameters.Add(param);
            }

            if (job.employmentTypes != null)
            {
                SqlParameter param = new SqlParameter("@EmploymentType", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_IntType]",
                    Value = Util.PopulateDataTable(job.employmentTypes.Cast<object>().ToList(), typeof(int))
                };
                parameters.Add(param);
            }

            if (job.jobTypes != null)
            {
                SqlParameter param = new SqlParameter("@JobType", SqlDbType.Structured)
                {
                    TypeName = "[dbo].[UDT_IntType]",
                    Value = Util.PopulateDataTable(job.jobTypes.Cast<object>().ToList(), typeof(int))
                };
                parameters.Add(param);
            }

            if (job.startYearsOfExp > 0)
            {
                SqlParameter param = new SqlParameter("@StartYearsOfExp", SqlDbType.Int)
                {
                    Value = job.startYearsOfExp
                };
                parameters.Add(param);
            }

            if (job.endYearsOfExp > 0)
            {
                SqlParameter param = new SqlParameter("@EndYearsOfExp", SqlDbType.Int)
                {
                    Value = job.endYearsOfExp
                };
                parameters.Add(param);
            }

            if (job.RowsOfPage > 0)
            {
                SqlParameter param = new SqlParameter("@RowsOfPage", SqlDbType.Int)
                {
                    Value = job.RowsOfPage
                };
                parameters.Add(param);
            }

            if (job.PageNumber > 0)
            {
                SqlParameter param = new SqlParameter("@PageNumber", SqlDbType.Int)
                {
                    Value = job.PageNumber
                };
                parameters.Add(param);
            }

            if (job.PostedStartDate >= DateTime.Today.AddDays(-90))
            {
                SqlParameter param = new SqlParameter("@PostedStartDate", SqlDbType.DateTime)
                {
                    Value = job.PostedStartDate
                };
                parameters.Add(param);
            }

            if (job.PostedEndDate >= DateTime.Today.AddDays(-90))
            {
                SqlParameter param = new SqlParameter("@PostedEndDate", SqlDbType.DateTime)
                {
                    Value = job.PostedEndDate
                };
                parameters.Add(param);
            }

            if (job.ConsultancyUserId > 0)
            {
                SqlParameter param = new SqlParameter("@ConsultancyUserId", SqlDbType.Int)
                {
                    Value = job.ConsultancyUserId
                };
                parameters.Add(param);
            }

            if (job.PublicProfileUserName != null )
            {
                SqlParameter param = new SqlParameter("@PublicProfileUserName", SqlDbType.VarChar)
                {
                    Value = job.PublicProfileUserName
                };
                parameters.Add(param);
            }

            using (var cnn = _context.Database.GetDbConnection())
            {
                var cmm = cnn.CreateCommand();
                cmm.CommandType = System.Data.CommandType.StoredProcedure;
                cmm.CommandText = "[dbo].[GetJobOpeningSearch]";

                cmm.Parameters.AddRange(parameters.ToArray());
                cmm.Connection = cnn;
                cnn.Open();
                var reader = cmm.ExecuteReader();

                while (reader.Read())
                {
                    JobOpeningForSearchResultsDto result = new JobOpeningForSearchResultsDto();

                    result.JobOpeningId = reader["JobOpeningId"].ToString();
                    result.PostedDate = reader["PostedDate"].ToString();
                    result.LastDate = reader["LastDate"].ToString();
                    result.JobOpeningName = reader["JobOpeningName"].ToString();
                    result.CompanyName = reader["CompanyName"].ToString();
                    result.Joiningdays = reader["Joiningdays"].ToString();
                    result.JobDescription = reader["JobDescription"].ToString();
                    result.TotalExp = reader["TotalExp"].ToString();
                    result.NumberOfOpening = reader["NumberOfOpening"].ToString();
                    result.UserId = reader["UserId"].ToString();
                    result.UserFName = reader["UserFName"].ToString();
                    result.UserLName = reader["UserLName"].ToString();
                    result.UserName = reader["UserName"].ToString();
                    result.ProfilePic = reader["ProfilePic"].ToString();
                    result.Skills = reader["Skills"].ToString().Split('|').ToList();
                    result.Locations = reader["Locations"].ToString().Split('|').ToList();
                    result.Visas = reader["Visas"].ToString().Split('|').ToList();
                    result.EmploymentTypes = reader["EmploymentTypes"].ToString().Split('|').ToList();
                    result.JobTypes = reader["JobTypes"].ToString().Split('|').ToList();

                    rtn.Add(result);
                }
                return rtn;
            }

        }

        public async Task<List<JobOpening>> GetJobOpeningsByConsultancyID(long ConsultancyId)
        {
            var jobOpeningList = _context.JobOpenings
               .Include(o => o.ConsultancyUser)
               .Where(c => c.ConsultancyUser.ConsultancyId == ConsultancyId);

            return await jobOpeningList.ToListAsync();
        }

        public async Task<List<JobOpening>> GetJobOpeningsByConsultancyUserID(long ConsultancyUserId, string? publicprofileID)
        {
            var jobOpeningList = _context.JobOpenings
                .Include(o => o.JobOpeningSkills).ThenInclude(o => o.Skill)
                .Include(o => o.JobOpeningVisaMaps).ThenInclude(o => o.Visa)
                .Include(o => o.JobOpeningLocations).ThenInclude(o => o.City).ThenInclude(o => o.IdStateNavigation)
                .Include(o => o.JobOpeningEmploymentTypes).ThenInclude(o => o.EmploymentType)
                .Include(o => o.JobOpeningJobTypes).ThenInclude(o => o.JobType)
                .Where(c => c.ConsultancyUserId == ConsultancyUserId && c.ConsultancyUser.PublicProfileUserName== publicprofileID);

            return await jobOpeningList.ToListAsync();
        }


        public async Task<List<JobOpeningProfileConsultancyComment>> GetJobOpeningProfileConsultancyComment(long JobopeningCandidateProfileMapId, UserContext userContext)
        {
            var jobOpeningList = _context.JobOpeningProfileConsultancyComments
               .Where(c => c.JobopeningCandidateProfileMapId == JobopeningCandidateProfileMapId);

            return await jobOpeningList.ToListAsync();
        }

        public async Task<List<JobOpeningSummary>> GetCountJobsPostedByConsultancyUserId(long ConsultancyUserId, DateTime FromDate, DateTime ToDate)
        {
            var list = (from o in _context.JobOpenings
                        where o.ConsultancyUserId == ConsultancyUserId && o.PostedDate >= FromDate && o.PostedDate <= ToDate
                        group o by new { o.ConsultancyUserId } into g
                        
                        select new JobOpeningSummary
                        {
                            ConsultancyUserId = ConsultancyUserId,
                            Count = g.Count(),
                        });
           return await list.ToListAsync();
        }
        public async Task<List<JobOpeningSummary>> GetCountActiveJobsAsOfToday(long ConsultancyUserId, UserContext userContext)
        {
            var list = (from o in _context.JobOpenings
                        where o.ConsultancyUserId == ConsultancyUserId && o.Active == true
                        group o by new { o.ConsultancyUserId } into g
                        select new JobOpeningSummary
                        {
                            ConsultancyUserId = ConsultancyUserId,
                            Count = g.Count(),
                        });
            return await list.ToListAsync();

        }
        public List<JobOpeningProfileMapSummary1> GetJobOpeningProfileMapSummary(long ConsultancyUserId)
        {
            var jobOpenings = _context.JobOpenings
                .AsNoTracking()
                .Where(o => o.ConsultancyUserId == ConsultancyUserId)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Description,
                    o.PostedDate,
                    o.JobLocation,
                    o.Active
                })
                .ToList();

            if (!jobOpenings.Any())
            {
                return new List<JobOpeningProfileMapSummary1>();
            }

            var jobIds = jobOpenings.Select(o => o.Id).ToList();

            var statusCounts = (from p in _context.JobOpeningCandidateProfileMaps.AsNoTracking()
                                join s in _context.CandidateProfileMappingStatuses.AsNoTracking() on p.CandidateProfileMappingStatusId equals s.Id
                                where jobIds.Contains(p.JobOpeningId)
                                group p.Id by new { p.JobOpeningId, p.CandidateProfileMappingStatusId, s.Name } into g
                                select new
                                {
                                    JobOpeningId = g.Key.JobOpeningId,
                                    CandidateProfileMappingStatusId = g.Key.CandidateProfileMappingStatusId,
                                    CandidateProfileMappingStatusName = g.Key.Name,
                                    Count = g.Count()
                                }).ToList();

            var candidateProfileMappingStatuses = _context.CandidateProfileMappingStatuses
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToList();

            var result = new List<JobOpeningProfileMapSummary1>();

            foreach (var jo in jobOpenings)
            {
                var summaryList = new List<ProfileCountSummary>();
                foreach (var status in candidateProfileMappingStatuses)
                {
                    var match = statusCounts.FirstOrDefault(x => x.JobOpeningId == jo.Id && x.CandidateProfileMappingStatusId == status.Id);
                    summaryList.Add(new ProfileCountSummary
                    {
                        CandidateProfileMappingStatusId = status.Id,
                        CandidateProfileMappingStatusName = status.Name,
                        Count = match?.Count ?? 0
                    });
                }

                result.Add(new JobOpeningProfileMapSummary1
                {
                    JobOpeningId = jo.Id,
                    Name = jo.Name,
                    Description = jo.Description,
                    PostDate = jo.PostedDate,
                    JobLocation = jo.JobLocation,
                    Active = jo.Active,
                    profileCountSummary = summaryList
                });
            }

            return result;
        }

        public async Task<JobOpening> GetJobOpeningsById(long? Id, UserContext userContext)
        {
            var jobOpenings = _context.JobOpenings
           .Include(o => o.JobOpeningSkills)
           .ThenInclude(o => o.Skill)
           .Include(o => o.JobOpeningVisaMaps)
           .ThenInclude(o => o.Visa)
           .Include(o => o.JobOpeningLocations)
           .ThenInclude(o => o.City)
           .ThenInclude(o => o.IdStateNavigation)
           .Include(o => o.JobOpeningEmploymentTypes)
           .ThenInclude(o => o.EmploymentType)
           .Include(o => o.JobOpeningJobTypes)
           .ThenInclude(o => o.JobType)
           .Where(c => c.Id == Id);
            return await jobOpenings.FirstOrDefaultAsync();
        }

        public async Task<int> ExpireOldJobOpeningsAsync(int daysThreshold = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);
            return await _context.JobOpenings
                .Where(j => (j.IsExpired == null || j.IsExpired == false) && j.PostedDate != null && j.PostedDate < cutoffDate)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(j => j.IsExpired, true)
                    .SetProperty(j => j.Updated, DateTime.UtcNow));
        }

        public async Task<RecruiterStatsDto> GetRecruiterStats(long consultancyUserId, UserContext userContext)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var jobOpenings = await _context.JobOpenings
                .AsNoTracking()
                .Where(j => j.ConsultancyUserId == consultancyUserId)
                .Select(j => new { j.Id, j.Active, j.IsExpired, j.PostedDate })
                .ToListAsync();

            if (!jobOpenings.Any())
            {
                return new RecruiterStatsDto
                {
                    JobRequirementPosted = 0,
                    ExpiredPostings = 0,
                    ResumesReceived = 0
                };
            }

            var jobIds = jobOpenings.Select(j => j.Id).ToList();

            var resumesReceivedCount = await _context.JobOpeningCandidateProfileMaps
                .AsNoTracking()
                .CountAsync(m => jobIds.Contains(m.JobOpeningId));

            var activeJobsCount = jobOpenings.Count(j => (j.Active == null || j.Active == true) && (j.IsExpired == null || j.IsExpired == false));
            var expiredJobsCount = jobOpenings.Count(j => j.IsExpired == true || (j.PostedDate != null && j.PostedDate < cutoffDate));

            return new RecruiterStatsDto
            {
                JobRequirementPosted = activeJobsCount,
                ExpiredPostings = expiredJobsCount,
                ResumesReceived = resumesReceivedCount
            };
        }
    }

}
