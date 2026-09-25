using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utility.Configuration;

namespace BusinessLayer.Manager
{
    public interface IDirectCandidateManager
    {
        Task<DirectCandidateProfileCompositeDto> GetCandidateFullProfile(long userId, UserContext userContext);
        Task<DirectCandidateDetailDto> SaveDetails(long userId, DirectCandidateDetailDtoForUpdate detailsDto, UserContext userContext);
        Task<DirectCandidateResumeDto> UploadResume(long userId, IFormFile file, bool isPrimary, UserContext userContext);
        Task<List<DirectCandidateResumeDto>> GetResumes(long userId, UserContext userContext);
        Task<bool> SetPrimaryResume(long userId, long resumeId, UserContext userContext);
        Task<bool> DeleteResume(long userId, long resumeId, UserContext userContext);

        Task<DirectCandidateExperienceDto> AddExperience(long userId, DirectCandidateExperienceForInsertDto expDto, UserContext userContext);
        Task<DirectCandidateExperienceDto?> UpdateExperience(long userId, long expId, DirectCandidateExperienceForInsertDto expDto, UserContext userContext);
        Task<bool> DeleteExperience(long userId, long expId, UserContext userContext);
        Task<List<DirectCandidateExperienceDto>> GetExperiences(long userId, UserContext userContext);

        Task<DirectCandidateEducationDto> AddEducation(long userId, DirectCandidateEducationForInsertDto eduDto, UserContext userContext);
        Task<DirectCandidateEducationDto?> UpdateEducation(long userId, long eduId, DirectCandidateEducationForInsertDto eduDto, UserContext userContext);
        Task<bool> DeleteEducation(long userId, long eduId, UserContext userContext);
        Task<List<DirectCandidateEducationDto>> GetEducations(long userId, UserContext userContext);

        Task<List<CandidateAppliedJobDto>> GetCandidateApplications(long candidateUserId, UserContext userContext);
        Task<JobOpeningCandidateProfileMapDto> ApplyDirectJob(long candidateUserId, DirectCandidateApplyJobDto applyDto, IConfigurationValueProvider? authValueProvider, UserContext userContext);
    }

    public class DirectCandidateManager : BaseManager<DirectCandidateManager>, IDirectCandidateManager
    {
        public DirectCandidateManager(IServiceProvider provider, ILogger<DirectCandidateManager> logger, IMapper mapper)
            : base(provider, logger, mapper)
        {
        }

        public async Task<DirectCandidateProfileCompositeDto> GetCandidateFullProfile(long userId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var user = await repo.GetCandidateFullProfile(userId, userContext);
                if (user == null)
                {
                    return new DirectCandidateProfileCompositeDto { UserId = userId };
                }

                var composite = new DirectCandidateProfileCompositeDto
                {
                    UserId = user.Id,
                    Fname = user.Fname,
                    Lname = user.Lname,
                    Email = user.Email,
                    Phone = user.Phone,
                    Linkedin = user.Linkedin,
                    CityId = user.CityId,
                    CityName = user.City != null ? (user.City.City1 + (user.City.IdStateNavigation != null ? $", {user.City.IdStateNavigation.StateCode}" : "")) : null,
                    ProfilePic = user.ProfilePic
                };

                if (user.DirectCandidateDetail != null)
                {
                    composite.Details = mapper.Map<DirectCandidateDetailDto>(user.DirectCandidateDetail);
                }

                if (user.DirectCandidateResumes != null && user.DirectCandidateResumes.Any())
                {
                    composite.Resumes = mapper.Map<List<DirectCandidateResumeDto>>(
                        user.DirectCandidateResumes.OrderByDescending(r => r.IsPrimary).ThenByDescending(r => r.UploadedDate).ToList());
                }

                if (user.DirectCandidateExperiences != null && user.DirectCandidateExperiences.Any())
                {
                    composite.Experiences = mapper.Map<List<DirectCandidateExperienceDto>>(
                        user.DirectCandidateExperiences.OrderByDescending(e => e.IsCurrent).ThenByDescending(e => e.StartDate).ToList());
                }

                if (user.DirectCandidateEducations != null && user.DirectCandidateEducations.Any())
                {
                    composite.Educations = mapper.Map<List<DirectCandidateEducationDto>>(
                        user.DirectCandidateEducations.OrderByDescending(e => e.GraduationYear).ToList());
                }

                return composite;
            }, "GetCandidateFullProfile", userContext);
        }

        public async Task<DirectCandidateDetailDto> SaveDetails(long userId, DirectCandidateDetailDtoForUpdate detailsDto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var entity = mapper.Map<DirectCandidateDetail>(detailsDto);
                var saved = await repo.SaveDetails(userId, entity, userContext);
                return mapper.Map<DirectCandidateDetailDto>(saved);
            }, "SaveDetails", userContext);
        }

        public async Task<DirectCandidateResumeDto> UploadResume(long userId, IFormFile file, bool isPrimary, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var fileManager = managerFactory.Get<IFileManager>();
                var fileModel = new FileModel { ImageFile = file };
                string uploadedFileName = await fileManager.Upload(fileModel, "resumes");

                var resume = new DirectCandidateResume
                {
                    CandidateUserId = userId,
                    FileName = file.FileName,
                    BlobUrl = uploadedFileName,
                    FileSizeInKb = (int)(file.Length / 1024),
                    IsPrimary = isPrimary,
                    UploadedDate = DateTime.UtcNow
                };

                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var saved = await repo.AddResume(resume, userContext);
                return mapper.Map<DirectCandidateResumeDto>(saved);
            }, "UploadResume", userContext);
        }

        public async Task<List<DirectCandidateResumeDto>> GetResumes(long userId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var list = await repo.GetResumes(userId, userContext);
                return mapper.Map<List<DirectCandidateResumeDto>>(list);
            }, "GetResumes", userContext);
        }

        public async Task<bool> SetPrimaryResume(long userId, long resumeId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                return await repo.SetPrimaryResume(userId, resumeId, userContext);
            }, "SetPrimaryResume", userContext);
        }

        public async Task<bool> DeleteResume(long userId, long resumeId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var resume = await repo.GetResumeById(resumeId, userContext);
                if (resume != null && !string.IsNullOrEmpty(resume.BlobUrl))
                {
                    try
                    {
                        var fileManager = managerFactory.Get<IFileManager>();
                        await fileManager.Delete(resume.BlobUrl, "resumes");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete resume blob {BlobUrl}", resume.BlobUrl);
                    }
                }
                return await repo.DeleteResume(userId, resumeId, userContext);
            }, "DeleteResume", userContext);
        }

        public async Task<DirectCandidateExperienceDto> AddExperience(long userId, DirectCandidateExperienceForInsertDto expDto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var entity = mapper.Map<DirectCandidateExperience>(expDto);
                entity.CandidateUserId = userId;
                var saved = await repo.AddExperience(entity, userContext);
                return mapper.Map<DirectCandidateExperienceDto>(saved);
            }, "AddExperience", userContext);
        }

        public async Task<DirectCandidateExperienceDto?> UpdateExperience(long userId, long expId, DirectCandidateExperienceForInsertDto expDto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var entity = mapper.Map<DirectCandidateExperience>(expDto);
                entity.Id = expId;
                entity.CandidateUserId = userId;
                var updated = await repo.UpdateExperience(entity, userContext);
                return updated != null ? mapper.Map<DirectCandidateExperienceDto>(updated) : null;
            }, "UpdateExperience", userContext);
        }

        public async Task<bool> DeleteExperience(long userId, long expId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                return await repo.DeleteExperience(userId, expId, userContext);
            }, "DeleteExperience", userContext);
        }

        public async Task<List<DirectCandidateExperienceDto>> GetExperiences(long userId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var list = await repo.GetExperiences(userId, userContext);
                return mapper.Map<List<DirectCandidateExperienceDto>>(list);
            }, "GetExperiences", userContext);
        }

        public async Task<DirectCandidateEducationDto> AddEducation(long userId, DirectCandidateEducationForInsertDto eduDto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var entity = mapper.Map<DirectCandidateEducation>(eduDto);
                entity.CandidateUserId = userId;
                var saved = await repo.AddEducation(entity, userContext);
                return mapper.Map<DirectCandidateEducationDto>(saved);
            }, "AddEducation", userContext);
        }

        public async Task<DirectCandidateEducationDto?> UpdateEducation(long userId, long eduId, DirectCandidateEducationForInsertDto eduDto, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var entity = mapper.Map<DirectCandidateEducation>(eduDto);
                entity.Id = eduId;
                entity.CandidateUserId = userId;
                var updated = await repo.UpdateEducation(entity, userContext);
                return updated != null ? mapper.Map<DirectCandidateEducationDto>(updated) : null;
            }, "UpdateEducation", userContext);
        }

        public async Task<bool> DeleteEducation(long userId, long eduId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                return await repo.DeleteEducation(userId, eduId, userContext);
            }, "DeleteEducation", userContext);
        }

        public async Task<List<DirectCandidateEducationDto>> GetEducations(long userId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var list = await repo.GetEducations(userId, userContext);
                return mapper.Map<List<DirectCandidateEducationDto>>(list);
            }, "GetEducations", userContext);
        }

        public async Task<List<CandidateAppliedJobDto>> GetCandidateApplications(long candidateUserId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var apps = await repo.GetCandidateApplications(candidateUserId, userContext);

                return apps.Select(a => new CandidateAppliedJobDto
                {
                    Id = a.Id,
                    JobOpeningId = a.JobOpeningId,
                    JobTitle = a.JobOpening?.Name ?? "Job Opening",
                    CompanyName = a.JobOpening?.ConsultancyUser?.Consultancy?.Name ?? "Hiring Company",
                    CompanyLogo = a.JobOpening?.ConsultancyUser?.Consultancy?.Logo,
                    JobLocation = a.JobOpening?.JobLocation ?? a.JobOpening?.JobOpeningLocations?.FirstOrDefault()?.City?.City1,
                    FromAmt = a.JobOpening?.FromAmt,
                    ToAmt = a.JobOpening?.ToAmt,
                    StatusId = a.CandidateProfileMappingStatusId,
                    StatusName = a.CandidateProfileMappingStatus?.Name ?? "Applied",
                    ResumeDoc = a.Doc,
                    Comment = a.Comment,
                    AppliedDate = a.AppliedDate,
                    IsRead = a.IsRead
                }).ToList();
            }, "GetCandidateApplications", userContext);
        }

        public async Task<JobOpeningCandidateProfileMapDto> ApplyDirectJob(long candidateUserId, DirectCandidateApplyJobDto applyDto, IConfigurationValueProvider? authValueProvider, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IDirectCandidateRepository>();
                var candidateUser = await repo.GetCandidateFullProfile(candidateUserId, userContext);
                if (candidateUser == null)
                {
                    throw new Exception("Candidate profile not found.");
                }

                bool alreadyApplied = await repo.HasAlreadyApplied(candidateUserId, applyDto.JobOpeningId);
                if (alreadyApplied)
                {
                    throw new Exception("You have already applied for this job.");
                }

                // Resolve resume doc
                string? resumeBlob = null;
                if (applyDto.ResumeId.HasValue && applyDto.ResumeId.Value > 0)
                {
                    var selectedResume = await repo.GetResumeById(applyDto.ResumeId.Value, userContext);
                    resumeBlob = selectedResume?.BlobUrl;
                }
                else
                {
                    var primaryResume = await repo.GetPrimaryResume(candidateUserId, userContext);
                    resumeBlob = primaryResume?.BlobUrl;
                }

                var mapDtoForInsert = new JobOpeningCandidateProfileMapDtoForInsert
                {
                    JobOpeningId = applyDto.JobOpeningId,
                    CandidateUserId = candidateUserId,
                    CandidateProfileId = null,
                    CandidateName = $"{candidateUser.Fname} {candidateUser.Lname}".Trim(),
                    LinkedIn = candidateUser.Linkedin ?? candidateUser.DirectCandidateDetail?.GitHubUrl,
                    TotalYearsOfExp = candidateUser.DirectCandidateDetail?.TotalYearsOfExp.HasValue == true ? (int?)Math.Round(candidateUser.DirectCandidateDetail.TotalYearsOfExp.Value) : null,
                    CurrentLocationCityid = candidateUser.CityId,
                    Comment = applyDto.CoverNote,
                    Doc = resumeBlob,
                    OriginalDocName = "CandidateResume.pdf"
                };

                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();
                return await jobOpeningManager.ApplyJob(mapDtoForInsert, authValueProvider, userContext);
            }, "ApplyDirectJob", userContext);
        }
    }
}
