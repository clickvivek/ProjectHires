using BusinessEntityAndDTO.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface ICandidateProfileSkillsRepository : IRepository<CandidateProfileSkill, long>
    {
        Task RemoveRange(long? id, UserContext userContext);
    }
    public class CandidateProfileSkillsRepository : BaseRepository<CandidateProfileSkill, long>, ICandidateProfileSkillsRepository
    {
        public CandidateProfileSkillsRepository(EFContexts context) : base(context) { }

        public async Task RemoveRange(long? id, UserContext userContext)
        {
            if (id == null)
            {
                throw new KeyNotFoundException("Key Not Found : " + id);
            }
            await RemoveRange( _context.CandidateProfileSkills.Where(a => a.CandidateProfileid == id).ToList(), userContext);
        }
    }
}
