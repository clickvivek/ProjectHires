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
            var list = (from o in _context.JobOpenings
                        join p in _context.JobOpeningCandidateProfileMaps on o.Id equals p.JobOpeningId into pr
                        from prresult in pr.DefaultIfEmpty()
                        join s in _context.CandidateProfileMappingStatuses on prresult.CandidateProfileMappingStatusId equals s.Id into statusr
                        from statusresult in statusr.DefaultIfEmpty()
                        where o.ConsultancyUserId == ConsultancyUserId
                        group prresult.Id by new { o.Id, prresult.CandidateProfileMappingStatusId, o.Name, o.Description, statusname= statusresult.Name, o.PostedDate, o.Active  } into g
                        //join jo in _context.JobOpenings on g.Key.JobOpeningId equals jo.Id
                        //join jopm in _context.CandidateProfileMappingStatuses on g.Key.CandidateProfileMappingStatusId equals jopm.Id
                        select new JobOpeningProfileMapSummary { 
                            JobOpeningId = g.Key.Id, 
                            CandidateProfileMappingStatusId = g.Key.CandidateProfileMappingStatusId, 
                            Count = g.Count(), 
                            Name = g.Key.Name , 
                            Description = g.Key.Description, 
                            CandidateProfileMappingStatusName = g.Key.statusname 
                            ,PostDate = g.Key.PostedDate, 
                            Active = g.Key.Active ,
                        });
            var summaryList = list.ToList();

            //var details = (from g in summaryList
            //               join jo in _context.JobOpenings on g.JobOpeningId equals jo.Id
            //               join jopm in _context.CandidateProfileMappingStatuses on g.CandidateProfileMappingStatusId equals jopm.Id into jopmr
            //               from jopmresult in jopmr.DefaultIfEmpty()
            //               select new JobOpeningProfileMapSummary
            //               {
            //                   JobOpeningId = g.JobOpeningId,
            //                   CandidateProfileMappingStatusId = g.CandidateProfileMappingStatusId,
            //                   Count = g.Count,
            //                   Name = jo.Name,
            //                   Description = jo.Description,
            //                   CandidateProfileMappingStatusName = jopmresult.Name
            //               ,
            //                   PostDate = jo.PostedDate,
            //                   JobLocation = jo.JobLocation,
            //                   Active = jo.Active
            //               }).ToList();

            var result = new List<JobOpeningProfileMapSummary1>();
            var candidateProfileMappingStatuses = _context.CandidateProfileMappingStatuses.ToList();

            foreach (var item in summaryList)
            {
                if (item != null)
                {
                    var resultItem = result.Where(r => r.JobOpeningId == item.JobOpeningId).FirstOrDefault();

                    if (resultItem == null && item.CandidateProfileMappingStatusId != null)
                    {
                        resultItem = new JobOpeningProfileMapSummary1
                        {
                            JobOpeningId = item.JobOpeningId,
                            JobLocation = item.JobLocation,
                            Active = item.Active,
                            Name = item.Name,
                            Description = item.Description,
                            PostDate = item.PostDate,
                            profileCountSummary = new List<ProfileCountSummary>() { new ProfileCountSummary()
                                        { CandidateProfileMappingStatusId = item.CandidateProfileMappingStatusId,
                                            CandidateProfileMappingStatusName = item.CandidateProfileMappingStatusName,
                                            Count = item.Count} },
                        };

                        result.Add(resultItem);
                    }
                    else if (item.CandidateProfileMappingStatusId != null)
                    {
                        resultItem.profileCountSummary.Add(new ProfileCountSummary()
                        {
                            CandidateProfileMappingStatusId = item.CandidateProfileMappingStatusId,
                            CandidateProfileMappingStatusName = item.CandidateProfileMappingStatusName,
                            Count = item.Count
                        });
                    }
                    else
                    {
                        resultItem = new JobOpeningProfileMapSummary1
                        {
                            JobOpeningId = item.JobOpeningId,
                            JobLocation = item.JobLocation,
                            Active = item.Active,
                            Name = item.Name,
                            Description = item.Description,
                            PostDate = item.PostDate,
                            //profileCountSummary = new List<ProfileCountSummary>() { new ProfileCountSummary()
                            //            { CandidateProfileMappingStatusId = item.CandidateProfileMappingStatusId,
                            //                CandidateProfileMappingStatusName = item.CandidateProfileMappingStatusName,
                            //                Count = item.Count} },
                        };

                        result.Add(resultItem);
                    }
                }
            }

            foreach(var item in result)
            {
                if(item.profileCountSummary == null)
                {
                    var ProfileCountSummary = new List<ProfileCountSummary>();
                    
                    foreach (var i in candidateProfileMappingStatuses)
                    {
                        if(item.profileCountSummary == null)
                        {
                            item.profileCountSummary = new List<ProfileCountSummary>(){ new ProfileCountSummary()
                            {
                                CandidateProfileMappingStatusId = i.Id,
                                CandidateProfileMappingStatusName = i.Name,
                                Count = 0
                            } };
                        }
                        else
                        {
                            var profileCountSummary = new ProfileCountSummary()
                            {
                                CandidateProfileMappingStatusId = i.Id,
                                CandidateProfileMappingStatusName = i.Name,
                                Count = 0
                            };
                            item.profileCountSummary.Add(profileCountSummary);
                        }
                        
                    }
                }
                else
                {
                    foreach (var i in candidateProfileMappingStatuses)
                    {
                        if(item.profileCountSummary.Where(x=>x.CandidateProfileMappingStatusId==i.Id).Count()==0)
                        {
                            var profileCountSummary = new ProfileCountSummary()
                            {
                                CandidateProfileMappingStatusId = i.Id,
                                CandidateProfileMappingStatusName = i.Name,
                                Count = 0
                            };
                            item.profileCountSummary.Add( profileCountSummary);
                        }
                    }
                }
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
    }

}
