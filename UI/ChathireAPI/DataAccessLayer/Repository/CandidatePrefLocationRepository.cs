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
    public interface ICandidatePrefLocationRepository : IRepository<CandidatePrefLocation, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class CandidatePrefLocationRepository : BaseRepository<CandidatePrefLocation, long>, ICandidatePrefLocationRepository
    {
        public CandidatePrefLocationRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            var cp = _context.CandidatePrefLocations.Where(a => a.CandidateId == id).ToList();
            if (cp == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }

            _context.RemoveRange(cp);
            await _context.SaveChangesAsync();
        }
    }
}
