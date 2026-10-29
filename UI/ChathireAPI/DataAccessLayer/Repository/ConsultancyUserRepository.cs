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

    public interface IConsultancyUserRepository : IRepository<ConsultancyUser, long>
    {
        List<ConsultancyUser> CheckConsultancyUser(long? ConsultancyId, long? Userid);
        Task<bool> IsPublicProfileUserNameExists(string publicProfileUserName);
    }
    public class ConsultancyUserRepository : BaseRepository<ConsultancyUser, long>, IConsultancyUserRepository
    {
        public ConsultancyUserRepository(EFContexts context) : base(context) { }

        public List<ConsultancyUser> CheckConsultancyUser(long? ConsultancyId, long? Userid)
        {
            var list = _context.ConsultancyUsers.AsNoTracking().Where(s => s.UserId == Userid).ToList();
            if (list.Count > 0)
            {
                return list;
            }
            return null;
        }

        public async Task<bool> IsPublicProfileUserNameExists(string publicProfileUserName)
        {
            return await _context.ConsultancyUsers.AsNoTracking()
                .AnyAsync(s => s.PublicProfileUserName == publicProfileUserName);
        }
    }

}
