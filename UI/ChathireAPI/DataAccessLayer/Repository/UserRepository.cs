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

        Task<List<String>> UserFunction(UserContext userContext);
        Task<List<User>> GetUserByUserName(string email);
        Task<List<User>> GetUserByPublicProfileId(string publicProfileId);
        List<User> GetUserById(long Id);

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
            }catch(Exception ex)
            {
                throw ex;
            }
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

    }
}
