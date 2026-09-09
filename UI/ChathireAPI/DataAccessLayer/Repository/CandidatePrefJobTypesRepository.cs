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
    public interface ICandidatePrefJobTypesRepository : IRepository<CandidatePrefJobType, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class CandidatePrefJobTypesRepository : BaseRepository<CandidatePrefJobType, long>, ICandidatePrefJobTypesRepository
    {
        public CandidatePrefJobTypesRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            var cp = _context.CandidatePrefJobTypes.Where(a => a.CandidateProfileId == id).ToList();
            if (cp == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }

            _context.RemoveRange(cp);
            await _context.SaveChangesAsync();
        }
    }
}
