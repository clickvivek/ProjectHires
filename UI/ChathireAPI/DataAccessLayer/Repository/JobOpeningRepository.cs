using Utility.Common;
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
        Task<List<JobOpening>> GetExpiredJobsToProcessAsync(int daysThreshold = 30);
        Task<RecruiterStatsDto> GetRecruiterStats(long consultancyUserId, UserContext userContext);
        Task<JobOpeningAdminResponseDto> GetJobOpeningsForAdminAsync(string? search, string? status, string? source, int page, int pageSize);

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

                        // Parse boolean search expression (supports AND, OR, NOT, quotes, etc.)
            BooleanSearchExpression booleanExpr = BooleanSearchExpression.Parse(job.searchStrings);

            if (job.searchStrings != null)
            {
                // When boolean logic is present, query database for candidate matching terms
                var termsToQuery = (booleanExpr != null && booleanExpr.HasBooleanLogic && booleanExpr.CandidateDbTerms.Any())
                    ? booleanExpr.CandidateDbTerms
                    : job.searchStrings.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();

                var cleanSearchStrings = termsToQuery
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Cast<object>()
                    .ToList();

                if (cleanSearchStrings.Any())
                {
                    SqlParameter param = new SqlParameter("@SearchString", SqlDbType.Structured)
                    {
                        TypeName = "[dbo].[UDT_StringType]",
                        Value = Util.PopulateDataTable(cleanSearchStrings, typeof(string))
                    };
                    parameters.Add(param);
                }
            }

            // Determine if a StateId was passed explicitly or if cityIds contains a state-level City record (IsState = 1)
            int? effectiveStateId = job.StateId;

            if (job.cityIds != null && job.cityIds.Any())
            {
                // Check if any of the passed cityIds is a state-level record in the Cities table (IsState = 1)
                var stateCity = _context.Cities.AsNoTracking().FirstOrDefault(c => job.cityIds.Contains(c.Id) && c.IsState == true);
                if (stateCity != null)
                {
                    effectiveStateId = stateCity.IdState;
                }
                else
                {
                    SqlParameter param = new SqlParameter("@CityId", SqlDbType.Structured)
                    {
                        TypeName = "[dbo].[UDT_IntType]",
                        Value = Util.PopulateDataTable(job.cityIds.Cast<object>().ToList(), typeof(int))
                    };
                    parameters.Add(param);
                }
            }
            else if (!string.IsNullOrWhiteSpace(job.LocationString))
            {
                // Solution B fallback: User typed a location (e.g. "new jersey" or "georgia") without picking from dropdown
                var locTrimmed = job.LocationString.Trim().ToLower();
                var matchedState = _context.States.AsNoTracking().FirstOrDefault(s =>
                    s.StateName.ToLower() == locTrimmed ||
                    s.StateCode.ToLower() == locTrimmed);

                if (matchedState != null)
                {
                    effectiveStateId = matchedState.Id;
                }
            }

            // Only enforce strict StateId filter if CityId was NOT selected.
            // When a city is selected, we perform radius-based matching (e.g. 50/100/150/200 miles)
            // which should naturally include nearby jobs across state borders (e.g. NYC / Philly / NJ).
            bool hasCitySearch = job.cityIds != null && job.cityIds.Any();
            if (!hasCitySearch && effectiveStateId.HasValue && effectiveStateId.Value > 0)
            {
                SqlParameter param = new SqlParameter("@StateId", SqlDbType.Int)
                {
                    Value = effectiveStateId.Value
                };
                parameters.Add(param);
            }

            // Radius in miles (defaults to 50 in SP if omitted or <= 0)
            if (job.RadiusMiles.HasValue && job.RadiusMiles.Value > 0)
            {
                SqlParameter param = new SqlParameter("@RadiusMiles", SqlDbType.Decimal)
                {
                    Value = job.RadiusMiles.Value
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

                var colNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    colNames.Add(reader.GetName(i));
                }

                while (reader.Read())
                {
                    // Check IsExpired column if present
                    if (colNames.Contains("IsExpired") && reader["IsExpired"] != DBNull.Value && Convert.ToBoolean(reader["IsExpired"]))
                    {
                        continue;
                    }

                    // Check Active column if present
                    if (colNames.Contains("Active") && reader["Active"] != DBNull.Value && !Convert.ToBoolean(reader["Active"]))
                    {
                        continue;
                    }

                    string postedDateStr = reader["PostedDate"]?.ToString() ?? "";
                    string lastDateStr = reader["LastDate"]?.ToString() ?? "";

                    // Expiration check: LastDate in the past
                    if (DateTime.TryParse(lastDateStr, out DateTime lastDate) && lastDate.Date < DateTime.Today)
                    {
                        continue;
                    }

                    // Expiration check: PostedDate older than 30 days (unless custom search start date specified)
                    if (DateTime.TryParse(postedDateStr, out DateTime postedDate))
                    {
                        if (job.PostedStartDate.HasValue && job.PostedStartDate.Value > DateTime.MinValue)
                        {
                            if (postedDate.Date < job.PostedStartDate.Value.Date)
                            {
                                continue;
                            }
                        }
                        else if (postedDate.Date < DateTime.Today.AddDays(-30))
                        {
                            continue;
                        }
                    }

                    JobOpeningForSearchResultsDto result = new JobOpeningForSearchResultsDto();

                    result.JobOpeningId = reader["JobOpeningId"].ToString();
                    result.PostedDate = postedDateStr;
                    result.LastDate = lastDateStr;
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
                    result.CompanyLogo = colNames.Contains("CompanyLogo") && reader["CompanyLogo"] != DBNull.Value ? reader["CompanyLogo"].ToString() : null;
                    result.ProjectDurationmonths = colNames.Contains("ProjectDurationmonths") && reader["ProjectDurationmonths"] != DBNull.Value ? reader["ProjectDurationmonths"].ToString() : null;
                    result.AboutCompany = colNames.Contains("AboutCompany") && reader["AboutCompany"] != DBNull.Value ? reader["AboutCompany"].ToString() : null;
                    result.CompanyWebsite = colNames.Contains("CompanyWebsite") && reader["CompanyWebsite"] != DBNull.Value ? reader["CompanyWebsite"].ToString() : null;
                    result.CompanyLinkedin = colNames.Contains("CompanyLinkedin") && reader["CompanyLinkedin"] != DBNull.Value ? reader["CompanyLinkedin"].ToString() : null;
                    result.Skills = reader["Skills"].ToString().Split('|').ToList();
                    result.Locations = reader["Locations"].ToString().Split('|').ToList();
                    result.Visas = reader["Visas"].ToString().Split('|').ToList();
                    result.EmploymentTypes = reader["EmploymentTypes"].ToString().Split('|').ToList();
                    result.JobTypes = reader["JobTypes"].ToString().Split('|').ToList();

                    rtn.Add(result);
                }

                // If Boolean logic (AND, NOT, OR) was used in the search, filter results strictly
                if (booleanExpr != null && booleanExpr.HasBooleanLogic && !booleanExpr.IsEmpty)
                {
                    rtn = rtn.Where(r =>
                    {
                        var fullText = string.Join(" ", new[]
                        {
                            r.JobOpeningName,
                            r.JobDescription,
                            r.CompanyName,
                            r.Skills != null ? string.Join(" ", r.Skills) : "",
                            r.Locations != null ? string.Join(" ", r.Locations) : ""
                        });
                        return booleanExpr.IsMatch(fullText);
                    }).ToList();
                }

                return rtn;
            }

        }

        private static bool HasColumn(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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
            var today = DateTime.UtcNow.Date;
            return await _context.JobOpenings
                .Where(j => (j.IsExpired == null || j.IsExpired == false) &&
                            ((j.PostedDate != null && j.PostedDate < cutoffDate) || (j.LastDate != null && j.LastDate < today)))
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(j => j.IsExpired, true)
                    .SetProperty(j => j.Active, false)
                    .SetProperty(j => j.Updated, DateTime.UtcNow));
        }

        public async Task<List<JobOpening>> GetExpiredJobsToProcessAsync(int daysThreshold = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);
            var today = DateTime.UtcNow.Date;
            return await _context.JobOpenings
                .Include(j => j.ConsultancyUser)
                    .ThenInclude(cu => cu.User)
                .Include(j => j.ConsultancyUser)
                    .ThenInclude(cu => cu.Consultancy)
                .Where(j => (j.IsExpired == null || j.IsExpired == false) &&
                            ((j.PostedDate != null && j.PostedDate < cutoffDate) || (j.LastDate != null && j.LastDate < today)))
                .ToListAsync();
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
    
        public async Task<JobOpeningAdminResponseDto> GetJobOpeningsForAdminAsync(string? search, string? status, string? source, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 500;

            var query = _context.JobOpenings
                .AsNoTracking()
                .Include(j => j.ConsultancyUser)
                    .ThenInclude(cu => cu.User)
                .Include(j => j.ConsultancyUser)
                    .ThenInclude(cu => cu.Consultancy)
                .Include(j => j.JobOpeningSkills)
                    .ThenInclude(jos => jos.Skill)
                .Include(j => j.JobOpeningCandidateProfileMaps)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(j =>
                    (j.Name != null && EF.Functions.Like(j.Name, "%" + s + "%")) ||
                    (j.ConsultancyUser != null && j.ConsultancyUser.Consultancy != null && j.ConsultancyUser.Consultancy.Name != null && EF.Functions.Like(j.ConsultancyUser.Consultancy.Name, "%" + s + "%")) ||
                    (j.ConsultancyUser != null && j.ConsultancyUser.User != null && j.ConsultancyUser.User.UserName != null && EF.Functions.Like(j.ConsultancyUser.User.UserName, "%" + s + "%")) ||
                    (j.ConsultancyUser != null && j.ConsultancyUser.User != null && j.ConsultancyUser.User.Fname != null && EF.Functions.Like(j.ConsultancyUser.User.Fname, "%" + s + "%")) ||
                    (j.ConsultancyUser != null && j.ConsultancyUser.User != null && j.ConsultancyUser.User.Lname != null && EF.Functions.Like(j.ConsultancyUser.User.Lname, "%" + s + "%")) ||
                    (j.JobLocation != null && EF.Functions.Like(j.JobLocation, "%" + s + "%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim().ToLowerInvariant();
                if (st == "active")
                {
                    query = query.Where(j => (j.Active == null || j.Active == true) && (j.IsExpired == null || j.IsExpired == false));
                }
                else if (st == "inactive")
                {
                    query = query.Where(j => j.Active == false && (j.IsExpired == null || j.IsExpired == false));
                }
                else if (st == "expired")
                {
                    query = query.Where(j => j.IsExpired == true);
                }
            }

            if (!string.IsNullOrWhiteSpace(source) && !source.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var src = source.Trim();
                if (src.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(j => j.Source != null && (j.Source == "Email" || j.Source == "EMAIL"));
                }
                else if (src.Equals("portal", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(j => j.Source == null || j.Source == "PORTAL" || j.Source == "Portal");
                }
                else
                {
                    query = query.Where(j => j.Source != null && j.Source == src);
                }
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(j => j.PostedDate)
                .ThenByDescending(j => j.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new JobOpeningAdminItemDto
                {
                    Id = j.Id,
                    JobTitle = j.Name,
                    CompanyName = j.ConsultancyUser != null && j.ConsultancyUser.Consultancy != null ? j.ConsultancyUser.Consultancy.Name : null,
                    CompanyId = j.ConsultancyUser != null ? j.ConsultancyUser.ConsultancyId : null,
                    CompanyLogo = j.ConsultancyUser != null && j.ConsultancyUser.Consultancy != null ? j.ConsultancyUser.Consultancy.Logo : null,
                    PostedByUserName = j.ConsultancyUser != null && j.ConsultancyUser.User != null ? j.ConsultancyUser.User.UserName : null,
                    PostedByFullName = j.ConsultancyUser != null && j.ConsultancyUser.User != null ? (j.ConsultancyUser.User.Fname + " " + (j.ConsultancyUser.User.Lname ?? "")).Trim() : null,
                    PostedByEmail = j.ConsultancyUser != null && j.ConsultancyUser.User != null ? j.ConsultancyUser.User.Email : null,
                    PostedByUserId = j.ConsultancyUser != null ? (long?)j.ConsultancyUser.UserId : null,
                    ConsultancyUserId = j.ConsultancyUserId,
                    PostedDate = j.PostedDate,
                    LastDate = j.LastDate,
                    Updated = j.Updated,
                    Active = j.Active,
                    IsExpired = j.IsExpired,
                    JobLocation = j.JobLocation,
                    Country = j.Country,
                    TotalExp = j.TotalExp,
                    NumberOfOpening = j.NumberOfOpening,
                    SalaryDisplay = (j.FromAmt != null && j.ToAmt != null) ? (j.FromAmt + " - " + j.ToAmt) : null,
                    ApplicationCount = j.JobOpeningCandidateProfileMaps.Count,
                    Source = j.Source ?? "PORTAL",
                    Skills = j.JobOpeningSkills.Select(s => s.Skill.Name).ToList()
                })
                .ToListAsync();

            return new JobOpeningAdminResponseDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
