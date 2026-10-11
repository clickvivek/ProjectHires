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
        //List<ConsultancyUser> CheckConsultancyUser(long? ConsultancyId, long? Userid);
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
    }

}
