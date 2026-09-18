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
        Task<Skill?> GetByName(string name);
        Task<Skill> EnsureSkillExists(string name, long? userId);
    }
    public class SkillsRepository : BaseRepository<Skill, int>, ISkillsRepository
    {
        public SkillsRepository(EFContexts context) : base(context) { }

        public async Task<List<Skill>> GetSkills(string? skill)
        {
            var query = _context.Skills.AsNoTracking().Where(s => s.Active == null || s.Active == true);

            if (!string.IsNullOrWhiteSpace(skill))
            {
                var clean = skill.Trim();
                query = query.Where(s => s.Name != null && s.Name.StartsWith(clean));
            }

            return await query.OrderBy(s => s.Name).Take(25).ToListAsync();
        }

        public async Task<Skill?> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var clean = name.Trim();
            return await _context.Skills.FirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == clean.ToLower() && (s.Active == null || s.Active == true));
        }

        public async Task<Skill> EnsureSkillExists(string name, long? userId)
        {
            var clean = name.Trim();
            var existing = await GetByName(clean);
            if (existing != null)
            {
                return existing;
            }

            var newSkill = new Skill
            {
                Name = clean,
                Active = true,
                IsUserDefined = true,
                Updated = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _context.Skills.AddAsync(newSkill);
            await _context.SaveChangesAsync();
            return newSkill;
        }
    }
    
}
