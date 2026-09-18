using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface ICandidateProfileRepository : IRepository<CandidateProfile, long>
    {
        Task<List<CandidateProfile>> GetCandidateProfileByUser(long? userId, long? Id, UserContext userContext);
        Task<List<CandidateProfile>> GetByConsultancyUserID(long? Id, string? publicprofileID, UserContext userContext);
        Task<List<CandidateProfile>> GetByConsultancyUserSimplelist(long? Id, bool? isActive, short? statusId, UserContext userContext);
        Task<bool> ActivateOrDeActivateCandidateProfile(long CandidateProfileId, bool? Active, short? StatusId, UserContext userContext);
        List<CandidateProfileForSearchResultsDto> SearchCandidateProfile(CandidateProfileForSearchDto candidateProfile);
        Task<CandidateProfile> GetCandidateProfile(long? Id, UserContext userContext);
        Task<int> GetCountResumesReceivedTodayByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
        Task<int> GetCountResumesReceivedByDateByConsultancyUserID(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext);
        Task<int> GetCountUnreadResumesByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
        Task<int> GetActiveCountHotListByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
        Task<int> GetActiveCountHotListByUser(long userId, long? consultancyUserId, UserContext userContext);
        Task<int> GetCountResumesSubmittedLast30Days(long userId, long? consultancyUserId, UserContext userContext);
        Task<List<BenchSalesCandidateSummaryDto>> GetRecentBenchSalesCandidates(long userId, long? consultancyUserId, int count, UserContext userContext);
    }

    public class CandidateProfileRepository : BaseRepository<CandidateProfile, long>, ICandidateProfileRepository
    {
        public CandidateProfileRepository(EFContexts context) : base(context) { }

        public async Task<List<CandidateProfile>> GetCandidateProfileByUser(long? userId, long? Id, UserContext userContext)
        {
            IQueryable<CandidateProfile> candidateProfiles;


            if (userId != null)
            {
                candidateProfiles = _context.CandidateProfiles
               .Include(o => o.CandidatePrefJobTypes)
               .ThenInclude(o => o.JobType)
               .Include(o => o.CandidatePrefLocations)
               .ThenInclude(o => o.City)
               .Include(o => o.CandidateProfileDomains)
               .ThenInclude(o => o.Domain)
               .Include(o => o.CandidateProfileEmploymentTypes)
               .ThenInclude(o => o.EmploymentType)
               .Include(o => o.CandidateProfileSkills)
               .ThenInclude(o => o.Skill)
               .Include(o => o.City)
               .ThenInclude(o => o.IdStateNavigation)
               .Where(c => c.UserId == userId);
            }
            else
            {
                candidateProfiles = _context.CandidateProfiles
               .Include(o => o.CandidatePrefJobTypes)
               .ThenInclude(o => o.JobType)
               .Include(o => o.CandidatePrefLocations)
               .ThenInclude(o => o.City)
               .Include(o => o.CandidateProfileDomains)
               .ThenInclude(o => o.Domain)
               .Include(o => o.CandidateProfileEmploymentTypes)
               .ThenInclude(o => o.EmploymentType)
               .Include(o => o.CandidateProfileSkills)
               .ThenInclude(o => o.Skill)
               .Include(o => o.City)
               .ThenInclude(o => o.IdStateNavigation)
               .Where(c => c.Id == Id);
            }


            return await candidateProfiles.ToListAsync();
        }

        public async Task<int> GetCountResumesReceivedTodayByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var count = await _context.JobOpeningCandidateProfileMaps
                .Where(o => o.ConsultancyUserId == ConsultancyUserId && o.Updated > DateTime.Today)
                .CountAsync();
            return count;
        }

        public async Task<int> GetCountResumesReceivedByDateByConsultancyUserID(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext)
        {
            var count = await _context.JobOpeningCandidateProfileMaps
                .Where(o => o.ConsultancyUserId == ConsultancyUserId && o.Updated >= FromDate && o.Updated >= ToDate)
                .CountAsync();
            return count;
        }

        public async Task<int> GetCountUnreadResumesByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var count = await _context.JobOpeningCandidateProfileMaps
                .Where(o => o.ConsultancyUserId == ConsultancyUserId && o.IsRead == false)
                .CountAsync();
            return count;
        }

        public async Task<int> GetActiveCountHotListByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var count = await _context.CandidateProfiles
                .Where(o => o.ConsultancyUserId == ConsultancyUserId && o.Active == true)
                .CountAsync();
            return count;
        }

        public async Task<int> GetActiveCountHotListByUser(long userId, long? consultancyUserId, UserContext userContext)
        {
            var query = _context.CandidateProfiles.Where(o => o.Active == true);
            if (consultancyUserId.HasValue && consultancyUserId.Value > 0)
            {
                query = query.Where(o => o.ConsultancyUserId == consultancyUserId.Value || o.UserId == userId);
            }
            else
            {
                query = query.Where(o => o.UserId == userId);
            }
            return await query.CountAsync();
        }

        public async Task<int> GetCountResumesSubmittedLast30Days(long userId, long? consultancyUserId, UserContext userContext)
        {
            var fromDate = DateTime.UtcNow.AddDays(-30);

            var query = _context.JobOpeningCandidateProfileMaps
                .Include(m => m.CandidateProfile)
                .Where(m => (m.AppliedDate.HasValue && m.AppliedDate.Value >= fromDate) || (m.Updated.HasValue && m.Updated.Value >= fromDate));

            if (consultancyUserId.HasValue && consultancyUserId.Value > 0)
            {
                query = query.Where(m => m.CandidateUserId == userId || m.UpdatedBy == userId || (m.CandidateProfile != null && (m.CandidateProfile.ConsultancyUserId == consultancyUserId.Value || m.CandidateProfile.UserId == userId)));
            }
            else
            {
                query = query.Where(m => m.CandidateUserId == userId || m.UpdatedBy == userId || (m.CandidateProfile != null && m.CandidateProfile.UserId == userId));
            }

            return await query.CountAsync();
        }

        public async Task<List<BenchSalesCandidateSummaryDto>> GetRecentBenchSalesCandidates(long userId, long? consultancyUserId, int count, UserContext userContext)
        {
            var query = _context.CandidateProfiles
                .Include(c => c.CandidateProfileSkills)
                .ThenInclude(s => s.Skill)
                .Where(c => c.Active == true);

            if (consultancyUserId.HasValue && consultancyUserId.Value > 0)
            {
                query = query.Where(o => o.ConsultancyUserId == consultancyUserId.Value || o.UserId == userId);
            }
            else
            {
                query = query.Where(o => o.UserId == userId);
            }

            var candidates = await query
                .OrderByDescending(c => c.Id)
                .Take(count)
                .ToListAsync();

            var candidateIds = candidates.Select(c => c.Id).ToList();
            var appliedJobCounts = await _context.JobOpeningCandidateProfileMaps
                .Where(m => m.CandidateProfileId.HasValue && candidateIds.Contains(m.CandidateProfileId.Value))
                .GroupBy(m => m.CandidateProfileId.Value)
                .Select(g => new { CandidateProfileId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CandidateProfileId, g => g.Count);

            return candidates.Select(c => new BenchSalesCandidateSummaryDto
            {
                Id = c.Id,
                Name = c.CandidateName ?? string.Empty,
                Title = c.Title ?? string.Empty,
                PrimarySkill = c.CandidateProfileSkills?.Select(s => s.Skill?.Name).FirstOrDefault(n => !string.IsNullOrEmpty(n))
                               ?? c.Title ?? string.Empty,
                AppliedJobs = appliedJobCounts.ContainsKey(c.Id) ? appliedJobCounts[c.Id] : 0,
                NewMatchingJobs = 0
            }).ToList();
        }

        public async Task<List<CandidateProfile>> GetByConsultancyUserID(long? Id, string? publicprofileID, UserContext userContext)
        {

            var candidateProfiles = _context.CandidateProfiles
               .Include(o => o.CandidatePrefJobTypes)
               .ThenInclude(o => o.JobType)
               .Include(o => o.CandidatePrefLocations)
               .ThenInclude(o => o.City)
               .Include(o => o.CandidateProfileDomains)
               .ThenInclude(o => o.Domain)
               .Include(o => o.CandidateProfileEmploymentTypes)
               .ThenInclude(o => o.EmploymentType)
               .Include(o => o.CandidateProfileSkills)
               .ThenInclude(o => o.Skill)
               .Include(o => o.City)
               .ThenInclude(o => o.IdStateNavigation)
               .Include(o => o.CandidateDocuments)
               .ThenInclude(o => o.Document)
               .Where(c => c.ConsultancyUserId == Id && c.ConsultancyUser.PublicProfileUserName == publicprofileID);
            return await candidateProfiles.ToListAsync();
        }

        public async Task<List<CandidateProfile>> GetByConsultancyUserSimplelist(long? Id, bool? isActive, short? statusId, UserContext userContext)
        {
            var candidateProfiles = _context.CandidateProfiles
           .Include(o => o.CandidatePrefJobTypes)
           .ThenInclude(o => o.JobType)
           .Include(o => o.CandidatePrefLocations)
           .ThenInclude(o => o.City)
           .Include(o => o.CandidateProfileDomains)
           .ThenInclude(o => o.Domain)
           .Include(o => o.CandidateProfileEmploymentTypes)
           .ThenInclude(o => o.EmploymentType)
           .Include(o => o.CandidateProfileSkills)
           .ThenInclude(o => o.Skill)
           .Include(o => o.City)
           .ThenInclude(o => o.IdStateNavigation)
           .Include(o => o.Visa)
           .Include(o => o.CandidateDocuments)
           .Where(c => c.ConsultancyUserId == Id)
           .Where(a => (isActive != null) ? a.Active == isActive : true)
           .Where(a => (statusId != null) ? a.StatusId == statusId : true);
            return await candidateProfiles.ToListAsync();
        }

        public async Task<CandidateProfile> GetCandidateProfile(long? Id, UserContext userContext)
        {
            var candidateProfiles = _context.CandidateProfiles
           .Include(o => o.CandidatePrefJobTypes)
           .ThenInclude(o => o.JobType)
           .Include(o => o.CandidatePrefLocations)
           .ThenInclude(o => o.City)
           .ThenInclude(o => o.IdStateNavigation)
           .Include(o => o.CandidateProfileDomains)
           .ThenInclude(o => o.Domain)
           .Include(o => o.CandidateProfileEmploymentTypes)
           .ThenInclude(o => o.EmploymentType)
           .Include(o => o.CandidateProfileSkills)
           .ThenInclude(o => o.Skill)
           .Include(o => o.City)
           .ThenInclude(o => o.IdStateNavigation)
           .ThenInclude(o => o.CountryCodeNavigation)
           .Include(o => o.Visa)
           .Include(o => o.CandidateDocuments)
               .ThenInclude(o => o.Document)
           .Where(c => c.Id == Id);
            return await candidateProfiles.FirstOrDefaultAsync();
        }

        public List<CandidateProfileForSearchResultsDto> SearchCandidateProfile(CandidateProfileForSearchDto candidateProfile)
        {
            List<CandidateProfileForSearchResultsDto> rtn = new List<CandidateProfileForSearchResultsDto>();


            List<SqlParameter> parameters = new List<SqlParameter>();

            var searchStringList = candidateProfile.SearchString?
                .Where(s => !string.IsNullOrWhiteSpace(s) && s != "undefined")
                .Cast<object>().ToList() ?? new List<object>();

            SqlParameter paramSearch = new SqlParameter("@SearchString", SqlDbType.Structured)
            {
                TypeName = "[dbo].[UDT_StringType]",
                Value = Util.PopulateDataTable(searchStringList, typeof(string))
            };
            parameters.Add(paramSearch);

            var skillsList = candidateProfile.skills?.Cast<object>().ToList() ?? new List<object>();
            SqlParameter paramSkills = new SqlParameter("@Skills", SqlDbType.Structured)
            {
                TypeName = "[dbo].[UDT_IntType]",
                Value = Util.PopulateDataTable(skillsList, typeof(int))
            };
            parameters.Add(paramSkills);

            var cityList = candidateProfile.cityIds?.Cast<object>().ToList() ?? new List<object>();
            SqlParameter paramCity = new SqlParameter("@CityId", SqlDbType.Structured)
            {
                TypeName = "[dbo].[UDT_IntType]",
                Value = Util.PopulateDataTable(cityList, typeof(int))
            };
            parameters.Add(paramCity);

            var stateList = candidateProfile.stateIds?.Cast<object>().ToList() ?? new List<object>();
            SqlParameter paramState = new SqlParameter("@StateId", SqlDbType.Structured)
            {
                TypeName = "[dbo].[UDT_IntType]",
                Value = Util.PopulateDataTable(stateList, typeof(int))
            };
            parameters.Add(paramState);

            var visaList = candidateProfile.visas?.Cast<object>().ToList() ?? new List<object>();
            SqlParameter paramVisa = new SqlParameter("@Visa", SqlDbType.Structured)
            {
                TypeName = "[dbo].[UDT_IntType]",
                Value = Util.PopulateDataTable(visaList, typeof(int))
            };
            parameters.Add(paramVisa);

            //if (candidateProfile.employmentTypes != null)
            //{
            //    SqlParameter param = new SqlParameter("@EmploymentType", SqlDbType.Structured)
            //    {
            //        TypeName = "[dbo].[UDT_IntType]",
            //        Value = Util.PopulateDataTable(candidateProfile.employmentTypes.Cast<object>().ToList(), typeof(int))
            //    };
            //    parameters.Add(param);
            //}

            //if (candidateProfile.jobTypes != null)
            //{
            //    SqlParameter param = new SqlParameter("@JobType", SqlDbType.Structured)
            //    {
            //        TypeName = "[dbo].[UDT_IntType]",
            //        Value = Util.PopulateDataTable(candidateProfile.jobTypes.Cast<object>().ToList(), typeof(int))
            //    };
            //    parameters.Add(param);
            //}

            if (candidateProfile.startYearsOfExp > 0)
            {
                SqlParameter param = new SqlParameter("@StartYearsOfExp", SqlDbType.SmallInt)
                {
                    Value = candidateProfile.startYearsOfExp
                };
                parameters.Add(param);
            }

            if (candidateProfile.endYearsOfExp > 0)
            {
                SqlParameter param = new SqlParameter("@EndYearsOfExp", SqlDbType.SmallInt)
                {
                    Value = candidateProfile.endYearsOfExp
                };
                parameters.Add(param);
            }

            if (candidateProfile.RowsOfPage > 0)
            {
                SqlParameter param = new SqlParameter("@RowsOfPage", SqlDbType.Int)
                {
                    Value = candidateProfile.RowsOfPage
                };
                parameters.Add(param);
            }

            if (candidateProfile.PageNumber > 0)
            {
                SqlParameter param = new SqlParameter("@PageNumber", SqlDbType.Int)
                {
                    Value = candidateProfile.PageNumber
                };
                parameters.Add(param);
            }

            //if (candidateProfile.PostedStartDate >= DateTime.Today.AddDays(-90))
            //{
            //    SqlParameter param = new SqlParameter("@PostedStartDate", SqlDbType.DateTime)
            //    {
            //        Value = candidateProfile.PostedStartDate
            //    };
            //    parameters.Add(param);
            //}

            //if (candidateProfile.PostedEndDate >= DateTime.Today.AddDays(-90))
            //{
            //    SqlParameter param = new SqlParameter("@PostedEndDate", SqlDbType.DateTime)
            //    {
            //        Value = candidateProfile.PostedEndDate
            //    };
            //    parameters.Add(param);
            //}



            using (var cnn = _context.Database.GetDbConnection())
            {
                var cmm = cnn.CreateCommand();
                cmm.CommandType = System.Data.CommandType.StoredProcedure;
                cmm.CommandText = "[dbo].[GetCandidateSearch_updated]";

                cmm.Parameters.AddRange(parameters.ToArray());
                cmm.Connection = cnn;
                cnn.Open();
                var reader = cmm.ExecuteReader();

                while (reader.Read())
                {
                    CandidateProfileForSearchResultsDto result = new CandidateProfileForSearchResultsDto();

                    result.CandidateProfileId = reader["Id"].ToString();
                    result.CandidateName = reader["CandidateName"].ToString();
                    result.Title = reader["Title"].ToString();
                    result.VisaId = reader["VisaId"].ToString();
                    result.Visa = reader["Visa"].ToString();
                    result.CandidateAvailability = reader["CandidateAvailability"].ToString();
                    result.CurrentLocationCity = reader["CurrentLocationCity"].ToString();
                    result.CurrentLocationState = reader["CurrentLocationState"].ToString();
                    result.TotalExp = reader["TotalExp"].ToString();
                    result.CanRelocate = reader["CanRelocate"].ToString();
                    result.RemoteOnly = reader["RemoteOnly"].ToString();
                    result.ConsultancyUserId = reader["ConsultancyUserId"].ToString();
                    result.ConsultancyUserFName = reader["ConsultancyUserFName"].ToString();
                    result.ConsultancyUserLName = reader["ConsultancyUserLName"].ToString();
                    result.ConsultancyName = reader["ConsultancyName"].ToString();
                    result.Skills = (reader["Skills"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["Skills"].ToString()))
                        ? reader["Skills"].ToString().Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                        : new List<string>();
                    result.Locations = (reader["Locations"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["Locations"].ToString()))
                        ? reader["Locations"].ToString().Split('|', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                        : new List<string>();
                    result.EmploymentTypeName = (reader != null && reader["EmploymentTypeName"] != null) ? reader["EmploymentTypeName"].ToString() : String.Empty;
                    result.JobTypeName = (reader != null && reader["JobTypename"] != null) ? reader["JobTypename"].ToString() : String.Empty;

                    result.ProfilePic = reader["ProfilePic"].ToString();
                    result.Resume = reader["Resume"].ToString();
                    result.PublicProfileUserName = reader["PublicProfileUserName"].ToString();
                    rtn.Add(result);
                }
                return rtn;
            }

        }

        public async Task<bool> ActivateOrDeActivateCandidateProfile(long CandidateProfileId, bool? Active, short? StatusId, UserContext userContext)
        {
            var profile = await _context.CandidateProfiles.Where(o => o.Id == CandidateProfileId).FirstOrDefaultAsync();
            if (profile != null)
            {
                if (Active != null)
                    profile.Active = Active;
                profile.UserId = userContext.UserId;
                if (StatusId != null)
                    profile.StatusId = (short)StatusId;
                await this.Put(profile.Id, profile, true);
                return true;
            }
            return false;
        }
    }
}
