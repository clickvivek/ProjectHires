using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface IStatusRepository : IRepository<Status, short>
    {

    }
    public class StatusRepository : BaseRepository<Status, short>, IStatusRepository
    {
        public StatusRepository(EFContexts context) : base(context) { }

    }
    
}
