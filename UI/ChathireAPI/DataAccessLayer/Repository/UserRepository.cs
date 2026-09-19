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
    }
}
