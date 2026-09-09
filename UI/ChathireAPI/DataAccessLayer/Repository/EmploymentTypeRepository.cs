using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface IEmploymentTypeRepository : IRepository<EmploymentType, short>
    {

    }
    public class EmploymentTypeRepository : BaseRepository<EmploymentType, short>, IEmploymentTypeRepository
    {
        public EmploymentTypeRepository(EFContexts context) : base(context) { }

    }

}
