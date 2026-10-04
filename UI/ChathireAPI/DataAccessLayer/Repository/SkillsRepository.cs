using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface ISkillsRepository : IRepository<Skill, int>
    {
        Task<List<Skill>> GetSkills(string? skill);
        Task<Skill?> GetByName(string name);
        Task<Skill> EnsureSkillExists(string name, long? userId);

        Task<SkillsAdminSummaryDto> GetSkillsAdminSummaryAsync(bool? isUserDefined, bool? active, string? searchTerm);
        Task<BulkAddSkillsResultDto> BulkAddSkillsAsync(BulkAddSkillsRequestDto request, long? adminUserId);
        Task<Skill> AddSkillAsync(CreateSkillRequestDto request, long? adminUserId);
        Task<Skill?> UpdateSkillAsync(UpdateSkillRequestDto request, long? adminUserId);
        Task<bool> ToggleSkillStatusAsync(int id, bool active, long? adminUserId);
        Task<bool> ConvertSkillUserDefinedAsync(int id, bool isUserDefined, long? adminUserId);
        Task<DeleteSkillResultDto> DeleteSkillAsync(int id);
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

        public async Task<SkillsAdminSummaryDto> GetSkillsAdminSummaryAsync(bool? isUserDefined, bool? active, string? searchTerm)
        {
            var allQuery = _context.Skills.AsNoTracking();

            var totalCount = await allQuery.CountAsync();
            var systemDefinedCount = await allQuery.CountAsync(s => s.IsUserDefined == null || s.IsUserDefined == false);
            var userDefinedCount = await allQuery.CountAsync(s => s.IsUserDefined == true);
            var activeCount = await allQuery.CountAsync(s => s.Active == null || s.Active == true);
            var inactiveCount = await allQuery.CountAsync(s => s.Active == false);

            var query = allQuery;

            if (isUserDefined.HasValue)
            {
                if (isUserDefined.Value)
                {
                    query = query.Where(s => s.IsUserDefined == true);
                }
                else
                {
                    query = query.Where(s => s.IsUserDefined == null || s.IsUserDefined == false);
                }
            }

            if (active.HasValue)
            {
                if (active.Value)
                {
                    query = query.Where(s => s.Active == null || s.Active == true);
                }
                else
                {
                    query = query.Where(s => s.Active == false);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var clean = searchTerm.Trim().ToLower();
                query = query.Where(s => s.Name != null && s.Name.ToLower().Contains(clean));
            }

            var skillDtos = await query
                .OrderBy(s => s.Name)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Active = s.Active ?? true,
                    IsUserDefined = s.IsUserDefined ?? false,
                    Updated = s.Updated,
                    UpdatedBy = s.UpdatedBy
                })
                .ToListAsync();

            return new SkillsAdminSummaryDto
            {
                TotalCount = totalCount,
                SystemDefinedCount = systemDefinedCount,
                UserDefinedCount = userDefinedCount,
                ActiveCount = activeCount,
                InactiveCount = inactiveCount,
                Skills = skillDtos
            };
        }

        public async Task<BulkAddSkillsResultDto> BulkAddSkillsAsync(BulkAddSkillsRequestDto request, long? adminUserId)
        {
            var result = new BulkAddSkillsResultDto();

            if (string.IsNullOrWhiteSpace(request.SkillsText))
            {
                return result;
            }

            var rawTokens = request.SkillsText
                .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Regex.Replace(t, @"^[\d\.\-\*\•\s]+", "").Trim())
                .Where(t => t.Length >= 2)
                .ToList();

            result.TotalProcessed = rawTokens.Count;

            var existingNames = await _context.Skills
                .AsNoTracking()
                .Where(s => s.Name != null)
                .Select(s => s.Name!.ToLower())
                .ToListAsync();

            var existingSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
            var seenInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var skillsToInsert = new List<Skill>();

            foreach (var token in rawTokens)
            {
                if (existingSet.Contains(token) || seenInBatch.Contains(token))
                {
                    if (!result.SkippedSkills.Contains(token, StringComparer.OrdinalIgnoreCase))
                    {
                        result.SkippedSkills.Add(token);
                    }
                    continue;
                }

                seenInBatch.Add(token);
                result.AddedSkills.Add(token);

                skillsToInsert.Add(new Skill
                {
                    Name = token,
                    Active = request.Active,
                    IsUserDefined = request.IsUserDefined,
                    Updated = DateTime.UtcNow,
                    UpdatedBy = adminUserId
                });
            }

            if (skillsToInsert.Count > 0)
            {
                await _context.Skills.AddRangeAsync(skillsToInsert);
                await _context.SaveChangesAsync();
            }

            result.AddedCount = result.AddedSkills.Count;
            result.SkippedDuplicateCount = result.SkippedSkills.Count;

            return result;
        }

        public async Task<Skill> AddSkillAsync(CreateSkillRequestDto request, long? adminUserId)
        {
            var cleanName = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanName))
            {
                throw new ArgumentException("Skill name cannot be empty.");
            }

            var existing = await _context.Skills.FirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == cleanName.ToLower());
            if (existing != null)
            {
                throw new InvalidOperationException($"Skill '{cleanName}' already exists.");
            }

            var skill = new Skill
            {
                Name = cleanName,
                Active = request.Active,
                IsUserDefined = request.IsUserDefined,
                Updated = DateTime.UtcNow,
                UpdatedBy = adminUserId
            };

            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<Skill?> UpdateSkillAsync(UpdateSkillRequestDto request, long? adminUserId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == request.Id);
            if (skill == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var cleanName = request.Name.Trim();
                var duplicate = await _context.Skills.FirstOrDefaultAsync(s => s.Id != request.Id && s.Name != null && s.Name.ToLower() == cleanName.ToLower());
                if (duplicate != null)
                {
                    throw new InvalidOperationException($"Another skill with name '{cleanName}' already exists.");
                }
                skill.Name = cleanName;
            }

            if (request.Active.HasValue)
            {
                skill.Active = request.Active.Value;
            }

            if (request.IsUserDefined.HasValue)
            {
                skill.IsUserDefined = request.IsUserDefined.Value;
            }

            skill.Updated = DateTime.UtcNow;
            skill.UpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<bool> ToggleSkillStatusAsync(int id, bool active, long? adminUserId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (skill == null)
            {
                return false;
            }

            skill.Active = active;
            skill.Updated = DateTime.UtcNow;
            skill.UpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConvertSkillUserDefinedAsync(int id, bool isUserDefined, long? adminUserId)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (skill == null)
            {
                return false;
            }

            skill.IsUserDefined = isUserDefined;
            skill.Updated = DateTime.UtcNow;
            skill.UpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DeleteSkillResultDto> DeleteSkillAsync(int id)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (skill == null)
            {
                return new DeleteSkillResultDto
                {
                    Success = false,
                    WasDeactivated = false,
                    Message = "Skill not found."
                };
            }

            var usedInCandidate = await _context.CandidateProfileSkills.AnyAsync(c => c.SkillId == id);
            var usedInJob = await _context.JobOpeningSkills.AnyAsync(j => j.SkillId == id);

            if (usedInCandidate || usedInJob)
            {
                skill.Active = false;
                skill.Updated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new DeleteSkillResultDto
                {
                    Success = true,
                    WasDeactivated = true,
                    Message = $"Skill '{skill.Name}' is attached to existing candidate profiles or job openings. It has been deactivated instead of permanently deleted."
                };
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return new DeleteSkillResultDto
            {
                Success = true,
                WasDeactivated = false,
                Message = $"Skill '{skill.Name}' has been permanently deleted."
            };
        }
    }
}
