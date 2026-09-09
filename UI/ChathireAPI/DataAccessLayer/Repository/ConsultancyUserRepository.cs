using DataAccessLayer.Common;
using DataAccessLayer.Models;
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
            
              
            if(_context.ConsultancyUsers.Where(s => s.UserId == Userid).Count()>0)
            {
                return _context.ConsultancyUsers.Where(s => s.UserId == Userid).ToList();
            }
            return null;
        }
    }

}
