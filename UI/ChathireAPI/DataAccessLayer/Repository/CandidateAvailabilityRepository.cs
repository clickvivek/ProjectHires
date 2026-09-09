using DataAccessLayer.Common;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface ICandidateAvailabilityRepository : IRepository<CandidateAvailability, short>
    {

    }
    public class CandidateAvailabilityRepository : BaseRepository<CandidateAvailability, short>, ICandidateAvailabilityRepository
    {
        public CandidateAvailabilityRepository(EFContexts context) : base(context) { }

    }
    
}
