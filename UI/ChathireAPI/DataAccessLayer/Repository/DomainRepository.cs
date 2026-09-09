using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{

    public interface IDomainRepository : IRepository<Domain, short>
    {

    }
    public class DomainRepository : BaseRepository<Domain, short>, IDomainRepository
    {
        public DomainRepository(EFContexts context) : base(context) { }

    }

}
