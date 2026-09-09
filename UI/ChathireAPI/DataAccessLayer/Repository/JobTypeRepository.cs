using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface IJobTypeRepository : IRepository<JobType, short>
    {

    }
    public class JobTypeRepository : BaseRepository<JobType, short> , IJobTypeRepository
    {
        public JobTypeRepository(EFContexts context) : base(context) { }

    }

}
