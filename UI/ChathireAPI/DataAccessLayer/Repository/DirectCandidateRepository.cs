using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IDirectCandidateRepository : IRepository<DirectCandidateDetail, long>
    {
        Task<User?> GetCandidateFullProfile(long userId, UserContext userContext);
        Task<DirectCandidateDetail> SaveDetails(long userId, DirectCandidateDetail details, UserContext userContext);
        Task<DirectCandidateResume> AddResume(DirectCandidateResume resume, UserContext userContext);
        Task<List<DirectCandidateResume>> GetResumes(long userId, UserContext userContext);
        Task<bool> SetPrimaryResume(long userId, long resumeId, UserContext userContext);
        Task<bool> DeleteResume(long userId, long resumeId, UserContext userContext);
        Task<DirectCandidateResume?> GetPrimaryResume(long userId, UserContext userContext);
        Task<DirectCandidateResume?> GetResumeById(long resumeId, UserContext userContext);

        Task<DirectCandidateExperience> AddExperience(DirectCandidateExperience exp, UserContext userContext);
        Task<DirectCandidateExperience?> UpdateExperience(DirectCandidateExperience exp, UserContext userContext);
        Task<bool> DeleteExperience(long userId, long expId, UserContext userContext);
        Task<List<DirectCandidateExperience>> GetExperiences(long userId, UserContext userContext);

        Task<DirectCandidateEducation> AddEducation(DirectCandidateEducation edu, UserContext userContext);
        Task<DirectCandidateEducation?> UpdateEducation(DirectCandidateEducation edu, UserContext userContext);
        Task<bool> DeleteEducation(long userId, long eduId, UserContext userContext);
        Task<List<DirectCandidateEducation>> GetEducations(long userId, UserContext userContext);

        Task<List<JobOpeningCandidateProfileMap>> GetCandidateApplications(long candidateUserId, UserContext userContext);
        Task<JobOpeningCandidateProfileMap> SubmitDirectApplication(JobOpeningCandidateProfileMap application, UserContext userContext);
        Task<bool> HasAlreadyApplied(long candidateUserId, long jobOpeningId);
    }

    public class DirectCandidateRepository : BaseRepository<DirectCandidateDetail, long>, IDirectCandidateRepository
    {
        public DirectCandidateRepository(EFContexts context) : base(context)
        {
        }

        public async Task<User?> GetCandidateFullProfile(long userId, UserContext userContext)
        {
            return await _context.Users
                .Include(u => u.City)
                    .ThenInclude(c => c.IdStateNavigation)
                .Include(u => u.DirectCandidateDetail)
                    .ThenInclude(d => d.Visa)
                .Include(u => u.DirectCandidateResumes)
                .Include(u => u.DirectCandidateExperiences)
                    .ThenInclude(e => e.City)
                .Include(u => u.DirectCandidateEducations)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<DirectCandidateDetail> SaveDetails(long userId, DirectCandidateDetail details, UserContext userContext)
        {
            var existing = await _context.DirectCandidateDetails.FirstOrDefaultAsync(d => d.CandidateUserId == userId);
            if (existing != null)
            {
                existing.Headline = details.Headline;
                existing.Summary = details.Summary;
                existing.VisaId = details.VisaId;
                existing.VisaExpiryDate = details.VisaExpiryDate;
                existing.TotalYearsOfExp = details.TotalYearsOfExp;
                existing.ExpectedAnnualSalary = details.ExpectedAnnualSalary;
                existing.ExpectedHourlyRate = details.ExpectedHourlyRate;
                existing.PreferredWorkType = details.PreferredWorkType;
                existing.NoticePeriodDays = details.NoticePeriodDays;
                existing.CanRelocate = details.CanRelocate;
                existing.RemoteOnly = details.RemoteOnly;
                existing.IsActivelyLooking = details.IsActivelyLooking;
                existing.GitHubUrl = details.GitHubUrl;
                existing.IsPublicProfileEnabled = details.IsPublicProfileEnabled;
                existing.PublicProfileSlug = details.PublicProfileSlug;
                existing.IsShowCompensationPublic = details.IsShowCompensationPublic;
                existing.PreferredLocations = details.PreferredLocations;
                existing.UpdatedDate = DateTime.UtcNow;

                _context.DirectCandidateDetails.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                details.CandidateUserId = userId;
                details.CreatedDate = DateTime.UtcNow;
                _context.DirectCandidateDetails.Add(details);
                await _context.SaveChangesAsync();
                return details;
            }
        }

        public async Task<DirectCandidateResume> AddResume(DirectCandidateResume resume, UserContext userContext)
        {
            if (resume.IsPrimary)
            {
                var existingPrimaries = await _context.DirectCandidateResumes
                    .Where(r => r.CandidateUserId == resume.CandidateUserId && r.IsPrimary)
                    .ToListAsync();
                foreach (var r in existingPrimaries)
                {
                    r.IsPrimary = false;
                }
            }
            else
            {
                // If this is the only resume for user, make it primary
                var count = await _context.DirectCandidateResumes.CountAsync(r => r.CandidateUserId == resume.CandidateUserId);
                if (count == 0)
                {
                    resume.IsPrimary = true;
                }
            }

            resume.UploadedDate = DateTime.UtcNow;
            _context.DirectCandidateResumes.Add(resume);
            await _context.SaveChangesAsync();
            return resume;
        }

        public async Task<List<DirectCandidateResume>> GetResumes(long userId, UserContext userContext)
        {
            return await _context.DirectCandidateResumes
                .Where(r => r.CandidateUserId == userId)
                .OrderByDescending(r => r.IsPrimary)
                .ThenByDescending(r => r.UploadedDate)
                .ToListAsync();
        }

        public async Task<bool> SetPrimaryResume(long userId, long resumeId, UserContext userContext)
        {
            var resumes = await _context.DirectCandidateResumes
                .Where(r => r.CandidateUserId == userId)
                .ToListAsync();

            var target = resumes.FirstOrDefault(r => r.Id == resumeId);
            if (target == null) return false;

            foreach (var r in resumes)
            {
                r.IsPrimary = (r.Id == resumeId);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteResume(long userId, long resumeId, UserContext userContext)
        {
            var resume = await _context.DirectCandidateResumes
                .FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateUserId == userId);
            if (resume == null) return false;

            bool wasPrimary = resume.IsPrimary;
            _context.DirectCandidateResumes.Remove(resume);
            await _context.SaveChangesAsync();

            if (wasPrimary)
            {
                var remaining = await _context.DirectCandidateResumes
                    .Where(r => r.CandidateUserId == userId)
                    .OrderByDescending(r => r.UploadedDate)
                    .FirstOrDefaultAsync();
                if (remaining != null)
                {
                    remaining.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<DirectCandidateResume?> GetPrimaryResume(long userId, UserContext userContext)
        {
            return await _context.DirectCandidateResumes
                .Where(r => r.CandidateUserId == userId && r.IsPrimary)
                .FirstOrDefaultAsync() ??
                await _context.DirectCandidateResumes
                .Where(r => r.CandidateUserId == userId)
                .OrderByDescending(r => r.UploadedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<DirectCandidateResume?> GetResumeById(long resumeId, UserContext userContext)
        {
            return await _context.DirectCandidateResumes.FirstOrDefaultAsync(r => r.Id == resumeId);
        }

        public async Task<DirectCandidateExperience> AddExperience(DirectCandidateExperience exp, UserContext userContext)
        {
            exp.CreatedDate = DateTime.UtcNow;
            _context.DirectCandidateExperiences.Add(exp);
            await _context.SaveChangesAsync();
            return exp;
        }

        public async Task<DirectCandidateExperience?> UpdateExperience(DirectCandidateExperience exp, UserContext userContext)
        {
            var existing = await _context.DirectCandidateExperiences
                .FirstOrDefaultAsync(e => e.Id == exp.Id && e.CandidateUserId == exp.CandidateUserId);
            if (existing == null) return null;

            existing.CompanyName = exp.CompanyName;
            existing.Title = exp.Title;
            existing.CityId = exp.CityId;
            existing.StartDate = exp.StartDate;
            existing.EndDate = exp.EndDate;
            existing.IsCurrent = exp.IsCurrent;
            existing.Description = exp.Description;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExperience(long userId, long expId, UserContext userContext)
        {
            var existing = await _context.DirectCandidateExperiences
                .FirstOrDefaultAsync(e => e.Id == expId && e.CandidateUserId == userId);
            if (existing == null) return false;

            _context.DirectCandidateExperiences.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DirectCandidateExperience>> GetExperiences(long userId, UserContext userContext)
        {
            return await _context.DirectCandidateExperiences
                .Include(e => e.City)
                .Where(e => e.CandidateUserId == userId)
                .OrderByDescending(e => e.IsCurrent)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<DirectCandidateEducation> AddEducation(DirectCandidateEducation edu, UserContext userContext)
        {
            edu.CreatedDate = DateTime.UtcNow;
            _context.DirectCandidateEducations.Add(edu);
            await _context.SaveChangesAsync();
            return edu;
        }

        public async Task<DirectCandidateEducation?> UpdateEducation(DirectCandidateEducation edu, UserContext userContext)
        {
            var existing = await _context.DirectCandidateEducations
                .FirstOrDefaultAsync(e => e.Id == edu.Id && e.CandidateUserId == edu.CandidateUserId);
            if (existing == null) return null;

            existing.Institution = edu.Institution;
            existing.Degree = edu.Degree;
            existing.Major = edu.Major;
            existing.StartYear = edu.StartYear;
            existing.GraduationYear = edu.GraduationYear;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteEducation(long userId, long eduId, UserContext userContext)
        {
            var existing = await _context.DirectCandidateEducations
                .FirstOrDefaultAsync(e => e.Id == eduId && e.CandidateUserId == userId);
            if (existing == null) return false;

            _context.DirectCandidateEducations.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DirectCandidateEducation>> GetEducations(long userId, UserContext userContext)
        {
            return await _context.DirectCandidateEducations
                .Where(e => e.CandidateUserId == userId)
                .OrderByDescending(e => e.GraduationYear)
                .ToListAsync();
        }

        public async Task<List<JobOpeningCandidateProfileMap>> GetCandidateApplications(long candidateUserId, UserContext userContext)
        {
            return await _context.JobOpeningCandidateProfileMaps
                .Include(m => m.JobOpening)
                    .ThenInclude(j => j.ConsultancyUser)
                        .ThenInclude(cu => cu.Consultancy)
                .Include(m => m.JobOpening)
                    .ThenInclude(j => j.JobOpeningLocations)
                        .ThenInclude(jl => jl.City)
                .Include(m => m.CandidateProfileMappingStatus)
                .Where(m => m.CandidateUserId == candidateUserId && m.Active != false)
                .OrderByDescending(m => m.AppliedDate)
                .ToListAsync();
        }

        public async Task<JobOpeningCandidateProfileMap> SubmitDirectApplication(JobOpeningCandidateProfileMap application, UserContext userContext)
        {
            application.AppliedDate = DateTime.UtcNow;
            application.Active = true;
            application.CandidateProfileMappingStatusId = 1; // 1 = Applied
            application.IsRead = false;

            _context.JobOpeningCandidateProfileMaps.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<bool> HasAlreadyApplied(long candidateUserId, long jobOpeningId)
        {
            return await _context.JobOpeningCandidateProfileMaps
                .AnyAsync(m => m.CandidateUserId == candidateUserId && m.JobOpeningId == jobOpeningId && m.Active != false);
        }
    }
}
