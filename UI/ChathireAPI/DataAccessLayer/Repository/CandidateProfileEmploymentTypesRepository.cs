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

    public interface ICandidateProfileEmploymentTypesRepository : IRepository<CandidateProfileEmploymentType, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
        //Task<List<CandidateProfileEmploymentType>> AddRange(List<CandidateProfileEmploymentType> candidateProfileEmploymentTypes, UserContext userContext);
    }
    public class CandidateProfileEmploymentTypesRepository : BaseRepository<CandidateProfileEmploymentType, long>, ICandidateProfileEmploymentTypesRepository
    {
        public CandidateProfileEmploymentTypesRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            var cp = _context.CandidateProfileEmploymentTypes.Where(a => a.CandidateProfileId == id).ToList();
            if (cp == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }

            _context.RemoveRange(cp);
            await _context.SaveChangesAsync();
        }

        //public async Task<List<CandidateProfileEmploymentType>> AddRange(List<CandidateProfileEmploymentType> candidateProfileEmploymentTypes, UserContext userContext)
        //{
        //    if (candidateProfileEmploymentTypes == null)
        //    {
        //        throw new KeyNotFoundException("candidateProfileEmploymentTypes Not Found : ");
        //    }
        //    _context.AddRange(candidateProfileEmploymentTypes);
        //    await _context.SaveChangesAsync();
        //    return candidateProfileEmploymentTypes;
        //}
    }
}
