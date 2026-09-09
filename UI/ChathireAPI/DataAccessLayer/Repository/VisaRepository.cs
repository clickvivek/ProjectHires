using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IVisaRepository : IRepository<Visa, short>
    {

    }
    public class VisaRepository : BaseRepository<Visa, short>, IVisaRepository
    {
        public VisaRepository(EFContexts context) : base(context) { }

    }

}
