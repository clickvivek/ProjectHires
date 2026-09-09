using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IUserTypeRepository : IRepository<UserType, long>
    {

    }
    public class UserTypeRepository : BaseRepository<UserType, long>, IUserTypeRepository
    {
        public UserTypeRepository(EFContexts context) : base(context) { }

    }
}
