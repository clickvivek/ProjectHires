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
    public interface ISkillsRepository : IRepository<Skill, int>
    {
        Task<List<Skill>> GetSkills(string? skill);
    }
    public class SkillsRepository : BaseRepository<Skill, int>, ISkillsRepository
    {
        public SkillsRepository(EFContexts context) : base(context) { }

        public async Task<List<Skill>> GetSkills(string? skill)
        {
            if(skill == null)
            {
                return await _context.Skills.ToListAsync();
            }
            else
                return await _context.Skills.Where(s=> s.Name != null && s.Name.Contains(skill)).ToListAsync();
        }

    }
    
}
