using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Security.Hashing;

namespace DataAccessLayer.Repository
{
    public interface IUserRepository : IRepository<User, long>
    {
        Task<Tuple<String, UserContext, String>> ValidateUser(string UserName, string password);
        Task<Tuple<String, UserContext, String>> ValidateOrCreateGoogleUser(string email, string fname, string lname, string profilePic);

        Task<List<String>> UserFunction(UserContext userContext);
        Task<List<User>> GetUserByUserName(string email);
        Task<List<User>> GetUserByPublicProfileId(string publicProfileId);
        List<User> GetUserById(long Id);
        Task RecordUserLogin(long userId, string ipAddress = null, string location = null);
        Task UpdateUserLastActive(long userId);
        Task<DauDashboardDto> GetDauDashboard(string timeframe, DateTime? startDate = null, DateTime? endDate = null);
    }
    public class UserRepository : BaseRepository<User, long>, IUserRepository
    {
        public UserRepository(EFContexts context) : base(context) { }

        public async Task<Tuple<String, UserContext, String>> ValidateUser(string UserName, string password)
        {
            try
            {
                var result = await _context.Users
                                    .Include(u => u.UserType)
                                    .Include(u => u.ConsultancyUsers)
                                    .Where(u => (u.Email == UserName || u.AlternateEmail == UserName
                                    )).FirstOrDefaultAsync();
                if (result != null)
                {

                    if (SecurePasswordHasher.IsHashSupported(result.Password))
                    {
                        if (SecurePasswordHasher.Verify(password, result.Password))
                        {
                            return new Tuple<string, UserContext, string>(result.UserName, new UserContext
                            {
                                UserId = result.Id,
                                UserTypeId = result.UserTypeId,
                                ResetPassword = result.ResetPassword,
                                ConsultancyId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().ConsultancyId : null,
                                ConsultancyUserId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().Id : null,
                            }, result.UserType.Name);
                        }
                    }
                    else
                    {
                        if (password.Equals(result.Password))
                        {
                            return new Tuple<string, UserContext, string>(result.UserName, new UserContext
                            {
                                UserId = result.Id,
                                UserTypeId = result.UserTypeId,
                                ConsultancyId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().ConsultancyId : null,
                                ResetPassword = result.ResetPassword,
                                ConsultancyUserId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().Id : null,
                            }, result.UserType.Name);
                        }
                    }
                }
                throw new ArgumentException("Invalid UserName/Password");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Tuple<String, UserContext, String>> ValidateOrCreateGoogleUser(string email, string fname, string lname, string profilePic)
        {
            var result = await _context.Users
                                .Include(u => u.UserType)
                                .Include(u => u.ConsultancyUsers)
                                .Where(u => u.Email == email || u.AlternateEmail == email)
                                .FirstOrDefaultAsync();

            if (result == null)
            {
                var defaultUserType = await _context.UserTypes.FirstOrDefaultAsync() ?? new UserType { Id = 1, Name = "Candidate" };
                result = new User
                {
                    Fname = !string.IsNullOrEmpty(fname) ? fname : email,
                    Lname = lname ?? "",
                    Email = email,
                    UserName = email,
                    ProfilePic = profilePic,
                    Active = true,
                    EmailVerified = true,
                    UserTypeId = defaultUserType.Id,
                    Updated = DateTime.UtcNow
                };
                _context.Users.Add(result);
                await _context.SaveChangesAsync();

                result.UserType = defaultUserType;
            }

            string userTypeName = result.UserType != null ? result.UserType.Name : "Candidate";
            string userName = result.UserName ?? result.Email;

            return new Tuple<string, UserContext, string>(userName, new UserContext
            {
                UserId = result.Id,
                UserTypeId = result.UserTypeId,
                ResetPassword = result.ResetPassword,
                ConsultancyId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().ConsultancyId : null,
                ConsultancyUserId = result.ConsultancyUsers.FirstOrDefault() != null ? result.ConsultancyUsers.FirstOrDefault().Id : null,
            }, userTypeName);
        }

        public async Task<List<String>> UserFunction(UserContext userContext)
        {
            var query = _context.UserAccesses
                                .Include(u => u.Bofunction)
                                .Where(u => u.UserTypeId == userContext.UserTypeId)
                                .Select(u => u.Bofunction.Name);

            return await query.ToListAsync();
        }

        public async Task<List<User>> GetUserByUserName(string email)
        {
            return await _context.Users
                .Include(o => o.ConsultancyUsers)
                .ThenInclude(o=>o.Consultancy)
                .Where(s => s.Email != null && s.Email.Equals(email)).ToListAsync();
        }

        public async Task<List<User>> GetUserByPublicProfileId(string? publicProfileId)
        {
            var a = await (from o in _context.Users
                    join p in _context.ConsultancyUsers on o.Id equals p.UserId
                    where p.PublicProfileUserName == publicProfileId
            select o).Include(o => o.ConsultancyUsers).ToListAsync();

            return a;
        }

        public List<User> GetUserById(long Id)
        {
            return _context.Users.Where(s => s.Id.Equals(Id)).ToList();
        }

        public async Task RecordUserLogin(long userId, string ipAddress = null, string location = null)
        {
            try
            {
                var loginRecord = new UserLogin
                {
                    UserId = userId,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    Location = location,
                    IsActive = true,
                    Updated = DateTime.UtcNow
                };

                await _context.UserLogins.AddAsync(loginRecord);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Non-blocking: failure to record login should not block user authentication
            }
        }

        public async Task UpdateUserLastActive(long userId)
        {
            try
            {
                var latestLogin = await _context.UserLogins
                    .Where(l => l.UserId == userId && l.IsActive == true)
                    .OrderByDescending(l => l.LoginTime)
                    .FirstOrDefaultAsync();

                if (latestLogin != null)
                {
                    latestLogin.Updated = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Non-blocking: background activity update failure should not impact user flow
            }
        }

        public async Task<DauDashboardDto> GetDauDashboard(string timeframe, DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = new DauDashboardDto();
            var nowUtc = DateTime.UtcNow;
            var todayStartUtc = nowUtc.Date;
            var sevenDaysAgoUtc = nowUtc.AddDays(-7);
            var thirtyDaysAgoUtc = nowUtc.AddDays(-30);
            var activeThreshold = nowUtc.AddMinutes(-15);

            // 1. Summary KPIs
            result.Summary.DauCount = await _context.UserLogins
                .Where(l => (l.LoginTime >= todayStartUtc || l.Updated >= todayStartUtc))
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            result.Summary.WauCount = await _context.UserLogins
                .Where(l => (l.LoginTime >= sevenDaysAgoUtc || l.Updated >= sevenDaysAgoUtc))
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            result.Summary.MauCount = await _context.UserLogins
                .Where(l => (l.LoginTime >= thirtyDaysAgoUtc || l.Updated >= thirtyDaysAgoUtc))
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            result.Summary.ActiveNowCount = await _context.UserLogins
                .Where(l => ((l.Updated.HasValue && l.Updated >= activeThreshold) || (l.LoginTime.HasValue && l.LoginTime >= activeThreshold)) && l.IsActive == true)
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            result.Summary.TotalRegisteredUsers = await _context.Users.CountAsync();

            // 2. Filter window for Trend & Drilldown based on timeframe
            DateTime filterStart;
            string normalizedTf = (timeframe ?? "day").ToLower();
            if (normalizedTf == "week")
            {
                filterStart = sevenDaysAgoUtc;
            }
            else if (normalizedTf == "month")
            {
                filterStart = thirtyDaysAgoUtc;
            }
            else
            {
                filterStart = todayStartUtc;
            }

            if (startDate.HasValue) filterStart = startDate.Value;
            var filterEnd = endDate.HasValue ? endDate.Value : nowUtc;

            // Fetch logins within range
            var loginsInRange = await _context.UserLogins
                .Include(l => l.User)
                .ThenInclude(u => u.UserType)
                .Where(l => (l.LoginTime >= filterStart || l.Updated >= filterStart) && (l.LoginTime <= filterEnd || l.Updated <= filterEnd))
                .OrderByDescending(l => l.Updated ?? l.LoginTime)
                .ToListAsync();

            result.Summary.TotalLoginsInPeriod = loginsInRange.Count;

            // 3. Generate Trend points
            if (normalizedTf == "day")
            {
                for (int h = 0; h < 24; h++)
                {
                    var hourStart = todayStartUtc.AddHours(h);
                    var hourEnd = hourStart.AddHours(1);
                    var hourLogins = loginsInRange.Where(l => (l.LoginTime >= hourStart && l.LoginTime < hourEnd) || (l.Updated >= hourStart && l.Updated < hourEnd)).ToList();
                    result.Trends.Add(new DauTrendPointDto
                    {
                        DateLabel = $"{h:D2}:00",
                        PeriodKey = hourStart.ToString("yyyy-MM-dd HH:00"),
                        UniqueUsers = hourLogins.Select(l => l.UserId).Distinct().Count(),
                        TotalLogins = hourLogins.Count
                    });
                }
            }
            else if (normalizedTf == "week")
            {
                for (int d = 6; d >= 0; d--)
                {
                    var dayStart = todayStartUtc.AddDays(-d);
                    var dayEnd = dayStart.AddDays(1);
                    var dayLogins = loginsInRange.Where(l => (l.LoginTime >= dayStart && l.LoginTime < dayEnd) || (l.Updated >= dayStart && l.Updated < dayEnd)).ToList();
                    result.Trends.Add(new DauTrendPointDto
                    {
                        DateLabel = dayStart.ToString("ddd, MMM dd"),
                        PeriodKey = dayStart.ToString("yyyy-MM-dd"),
                        UniqueUsers = dayLogins.Select(l => l.UserId).Distinct().Count(),
                        TotalLogins = dayLogins.Count
                    });
                }
            }
            else // month
            {
                for (int d = 29; d >= 0; d--)
                {
                    var dayStart = todayStartUtc.AddDays(-d);
                    var dayEnd = dayStart.AddDays(1);
                    var dayLogins = loginsInRange.Where(l => (l.LoginTime >= dayStart && l.LoginTime < dayEnd) || (l.Updated >= dayStart && l.Updated < dayEnd)).ToList();
                    result.Trends.Add(new DauTrendPointDto
                    {
                        DateLabel = dayStart.ToString("MMM dd"),
                        PeriodKey = dayStart.ToString("yyyy-MM-dd"),
                        UniqueUsers = dayLogins.Select(l => l.UserId).Distinct().Count(),
                        TotalLogins = dayLogins.Count
                    });
                }
            }

            // 4. User Drilldown records
            foreach (var login in loginsInRange)
            {
                var role = login.User?.UserType?.Name;
                if (string.IsNullOrWhiteSpace(role))
                {
                    if (login.User?.RoleRecruiter == true) role = "Recruiter";
                    else if (login.User?.RoleBenchSales == true) role = "Bench Sales";
                    else role = "Candidate";
                }

                var lastActive = login.Updated ?? login.LoginTime;
                result.UserActivities.Add(new UserActivityDrilldownDto
                {
                    Id = login.Id,
                    UserId = login.UserId ?? 0,
                    UserName = login.User?.UserName ?? login.User?.Email ?? "Anonymous",
                    FullName = $"{login.User?.Fname} {login.User?.Lname}".Trim(),
                    Email = login.User?.Email ?? "",
                    RoleName = role,
                    LoginTime = login.LoginTime,
                    LastActiveTime = lastActive,
                    IpAddress = login.IpAddress ?? "::1",
                    Location = login.Location ?? login.User?.Location ?? "Unknown",
                    IsActive = login.IsActive ?? true,
                    IsOnline = lastActive.HasValue && lastActive.Value >= activeThreshold
                });
            }

            return result;
        }
    }
}
