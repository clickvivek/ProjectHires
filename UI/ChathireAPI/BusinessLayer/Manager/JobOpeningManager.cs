using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Configuration;
using BusinessLayer.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLayer.Manager
{
    public interface IJobOpeningManager
    {
        Task<JobOpeningCandidateProfileMapDto> ApplyJob(JobOpeningCandidateProfileMapDtoForInsert? jobOpeningCandidateProfile, IConfigurationValueProvider? authValueProvider, UserContext userContext);
        Task<JobOpeningCandidateProfileMapDto> UpdateJobStatus(long Id, short CandidateProfileMappingStatusId, UserContext userContext);
        Task<JobOpeningCandidateProfileMapDto> UpdateProfileAsRead(long Id, UserContext userContext);
        Task<JobOpeningProfileConsultancyCommentDto> AddJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDtoForInsert jobOpeningProfileConsultancyComment, UserContext userContext);
        Task<JobOpeningProfileConsultancyCommentDto> UpdateJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDto jobOpeningProfileConsultancyComment, UserContext userContext);
        Task<JobOpeningDto> AddJobOpening(JobOpeningForInsertDto jobOpening,List<UserSubscriptionPlanDto> userSubscriptionPlans , UserContext userContext);

        Task<JobOpeningDto> UpdateJobOpening(JobOpeningDtoForUpdate jobOpening, UserContext userContext);

        Task<List<JobOpeningDto>> GetAllJobOpening(UserContext userContext);

        Task<JobOpeningDto> GetJobOpeningById(long Id, UserContext userContext);

        Task<List<JobOpeningDto>> GetJobOpeningsByConsultancyID(long ConsultancyId, UserContext userContext);
        Task<List<JobOpeningDto>> GetJobOpeningsByConsultancyUserID(long ConsultancyUserId, string? publicprofileID, UserContext userContext);

        Task<List<JobOpeningProfileConsultancyCommentDto>> GetJobOpeningProfileConsultancyComment(long JobopeningCandidateProfileMapId, UserContext userContext);

        List<JobOpeningForSearchResultsDto> SearchJobOpenings(JobOpeningForSearchDto job, UserContext userContext);

        Task DeleteJobOpeningSkill(long id, UserContext userContext);
        Task DeleteJobOpeningEmploymentType(long id, UserContext userContext);
        Task DeleteJobOpeningVisaMap(long id, UserContext userContext);
        Task DeleteJobOpeningJobType(long id, UserContext userContext);
        Task DeleteJobOpeningLocation(long id, UserContext userContext);
        Task DeleteJobOpening(long id, UserContext userContext);
        Task DeleteJobProfileComment(long Id, UserContext userContext);

        Task<List<JobOpeningCandidateProfileDetailMapDto>> GetAllResumeReceivedByJobId(long JobOpeningId, UserContext userContext);
        Task<RecruiterStatsDto> GetRecruiterStats(long consultancyUserId, UserContext userContext);
        Task<List<JobOpeningProfileMapSummary1>> GetJobOpeningProfileMapSummary(long ConsultancyUserId, UserContext userContext);
        Task<JobOpeningDto> ActivateDeactivateJob(long Id, bool ActiveDeactive, UserContext userContext);

        Task<List<JobOpeningSummary>> GetCountJobsPostedByConsultancyUserId(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext);
        Task<List<JobOpeningSummary>> GetCountActiveJobsAsOfToday(long ConsultancyUserId, UserContext userContext);
        Task<JobOpeningCandidateProfileMap> GetJobOpeningCandidateProfileMapById(long Id, UserContext userContext);
        Task DeleteJobOpeningSkillsByJobId(long id, UserContext userContext);
        Task DeleteJobOpeningVisaMapsByJobId(long id, UserContext userContext);
        Task DeleteJobOpeningLocationsByJobId(long id, UserContext userContext);
        Task DeleteJobOpeningEmploymentTypesByJobId(long id, UserContext userContext);
        Task DeleteJobOpeningJobTypesByJobId(long id, UserContext userContext);
        Task<List<JobOpeningSkill>> AddJobOpeningSkills(List<JobOpeningSkill> jobOpeningSkills, UserContext userContext);
        Task<List<JobOpeningVisaMap>> AddJobOpeningVisaMaps(List<JobOpeningVisaMap> jobOpeningVisaMaps, UserContext userContext);
        Task<List<JobOpeningLocation>> AddJobOpeningLocations(List<JobOpeningLocation> jobOpeningLocations, UserContext userContext);
        Task<List<JobOpeningEmploymentType>> AddJobOpeningEmploymentTypes(List<JobOpeningEmploymentType> jobOpeningEmploymentTypes, UserContext userContext);
        Task<List<JobOpeningJobType>> AddJobOpeningJobTypes(List<JobOpeningJobType> jobOpeningJobTypes, UserContext userContext);
       
    }
    public class JobOpeningManager : BaseManager<JobOpeningManager>, IJobOpeningManager
    {


        public JobOpeningManager(IServiceProvider provider, ILogger<JobOpeningManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task DeleteJobOpeningSkill(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningSkillsRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpeningSkill", userContext);
        }

        public async Task DeleteJobOpeningEmploymentType(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningEmploymentTypesRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpeningEmploymentType", userContext);
        }

        public async Task DeleteJobOpeningVisaMap(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningVisaMapsRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpeningVisaMap", userContext);
        }

        public async Task DeleteJobOpeningJobType(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningJobTypesRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpeningJobType", userContext);
        }

        public async Task DeleteJobOpeningLocation(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningLocationsRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpeningLocation", userContext);
        }

        public async Task DeleteJobOpening(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobOpening", userContext);
        }

        public async Task DeleteJobProfileComment(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningProfileConsultancyCommentRepository>();
                await repo.Delete(id, true);
            }, "DeleteJobProfileComment", userContext);
        }

        public async Task<JobOpeningDto> AddJobOpening(JobOpeningForInsertDto jobOpening, List<UserSubscriptionPlanDto>? userSubscriptionPlans, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpening>(async () =>
            {
                await ResolveCustomSkillsForInsert(jobOpening, userContext);

                var date = DateTime.UtcNow;
                var _jobOpening = mapper.Map<JobOpening>(jobOpening);

                _jobOpening.JobOpeningSkills.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _jobOpening.JobOpeningVisaMaps.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _jobOpening.JobOpeningLocations.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _jobOpening.JobOpeningEmploymentTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _jobOpening.JobOpeningJobTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });

            //    var repo = repositoryFactory.Get<IJobOpeningRepository>();
            //    _jobOpening.Updated = date;
            //    _jobOpening.UpdatedBy = userContext.UserId;
            //    return await repo.Post(_jobOpening, true);
            //}, "AddJobOpening", userContext);

            //var result1 = await ExecuteAsync<UserSubscriptionPlan>(async () =>
            //{
            //    var date = DateTime.UtcNow;
            //    var _jobOpening = mapper.Map<UserSubscriptionPlan>(jobOpening);

            //    var repo = repositoryFactory.Get<IJobOpeningRepository>();
            //    return mapper.Map<JobOpeningDto>(await repo.GetJobOpeningsById(Id, userContext));

            //    _jobOpening.JobOpeningSkills.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
            //    _jobOpening.JobOpeningVisaMaps.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
            //    _jobOpening.JobOpeningLocations.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
            //    _jobOpening.JobOpeningEmploymentTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
            //    _jobOpening.JobOpeningJobTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });

                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                _jobOpening.Updated = date;
                _jobOpening.UpdatedBy = userContext.UserId;
                return await repo.Post(_jobOpening, true);
            }, "AddJobOpening", userContext);

            //if(userSubscriptionPlans != null)
            //{
            //    var result1 = await ExecuteAsync<bool>(async () =>
            //    {
            //        var date = DateTime.UtcNow;
            //        var _userSubscriptionPlan = mapper.Map<UserSubscriptionPlan>(userSubscriptionPlans);
            //        var repo = repositoryFactory.Get<IUserSubscriptionPlanRepository>();
            //        _userSubscriptionPlan.NoOfUsedJobPosting = _userSubscriptionPlan.NoOfUsedJobPosting + 1;
            //        _userSubscriptionPlan.Updated = date;
            //        _userSubscriptionPlan.UpdatedBy = userContext.UserId;
            //        await repo.Put(_userSubscriptionPlan.Id, _userSubscriptionPlan, true);
            //        return true;
            //    }, "UpdateUserSubscriptionPlanCount", userContext);

            //}
            
            return mapper.Map<JobOpeningDto>(result);
        }

        public async Task<JobOpeningDto> UpdateJobOpening(JobOpeningDtoForUpdate jobOpening, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpening>(async () =>
            {
                await ResolveCustomSkillsForUpdate(jobOpening, userContext);

                var date = DateTime.UtcNow;
                var _jobOpeningUpdate = mapper.Map<JobOpening>(jobOpening);

                long jobId = 0;
                if (jobOpening.Id != null || jobOpening.Id > 0)
                {
                    jobId = (long)jobOpening.Id;
                }
                else
                {
                    throw new KeyNotFoundException("Job Id invalid !");
                }

                _jobOpeningUpdate.JobOpeningSkills.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.JobId = jobOpening.Id; });
                await DeleteJobOpeningSkillsByJobId(jobId, userContext);
                var addJobOpeningSkills = await AddJobOpeningSkills(_jobOpeningUpdate.JobOpeningSkills.ToList(), userContext);
                var _jobOpeningSkills = mapper.Map<List<JobOpeningSkillForUpdateDto>>(addJobOpeningSkills);

                _jobOpeningUpdate.JobOpeningVisaMaps.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.JobOpeningId = jobId; });
                await DeleteJobOpeningVisaMapsByJobId(jobId, userContext);
                var addJobOpeningVisaMaps = await AddJobOpeningVisaMaps(_jobOpeningUpdate.JobOpeningVisaMaps.ToList(), userContext);
                var _jobOpeningVisaMaps = mapper.Map<List<JobOpeningVisaMapForUpdateDto>>(addJobOpeningVisaMaps);

                _jobOpeningUpdate.JobOpeningLocations.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.JobOpeningId = jobId; });
                await DeleteJobOpeningLocationsByJobId(jobId, userContext);
                var addJobOpeningLocations = await AddJobOpeningLocations(_jobOpeningUpdate.JobOpeningLocations.ToList(), userContext);
                var _jobOpeningLocations = mapper.Map<List<JobOpeningLocationForUpdateDto>>(addJobOpeningLocations);

                _jobOpeningUpdate.JobOpeningEmploymentTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.JobOpeningId = jobId; });
                await DeleteJobOpeningEmploymentTypesByJobId(jobId, userContext);
                var addJobOpeningEmploymentTypes = await AddJobOpeningEmploymentTypes(_jobOpeningUpdate.JobOpeningEmploymentTypes.ToList(), userContext);
                var _jobOpeningEmploymentTypes = mapper.Map<List<JobOpeningEmploymentTypeForUpdateDto>>(addJobOpeningEmploymentTypes);

                _jobOpeningUpdate.JobOpeningJobTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.JobOpeningId = jobId; });
                await DeleteJobOpeningJobTypesByJobId(jobId, userContext);
                var addJobOpeningJobTypes = await AddJobOpeningJobTypes(_jobOpeningUpdate.JobOpeningJobTypes.ToList(), userContext);
                var _jobOpeningJobTypes = mapper.Map<List<JobOpeningJobTypeForUpdateDto>>(addJobOpeningJobTypes);

                jobOpening.JobOpeningSkills = _jobOpeningSkills;
                jobOpening.JobOpeningVisaMaps = _jobOpeningVisaMaps;
                jobOpening.JobOpeningLocations = _jobOpeningLocations;
                jobOpening.JobOpeningEmploymentTypes = _jobOpeningEmploymentTypes;
                jobOpening.JobOpeningJobTypes = _jobOpeningJobTypes;
                var _jobOpening = mapper.Map<JobOpening>(jobOpening);

                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                _jobOpening.Updated = date;
                _jobOpening.UpdatedBy = userContext.UserId;
                await repo.Put(_jobOpening.Id, _jobOpening, true);
                return _jobOpening;
            }, "UpdateJobOpening", userContext);

            return mapper.Map<JobOpeningDto>(result);
        }

        public async Task<List<JobOpeningDto>> GetAllJobOpening(UserContext userContext)
        {
            return await ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return mapper.Map<List<JobOpeningDto>>(await repo.GetAll<JobOpening>());
            }, "GetAllJobOpening", userContext);
        }

        public async Task<JobOpeningDto> GetJobOpeningById(long Id, UserContext userContext)
        {
            return await ExecuteAsync<JobOpeningDto>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return mapper.Map<JobOpeningDto>(await repo.GetJobOpeningsById(Id, userContext));
            }, "GetAllJobOpening", userContext);
        }
        

        public async Task<JobOpeningCandidateProfileMap> GetJobOpeningCandidateProfileMapById(long Id, UserContext userContext)
        {
            return await ExecuteAsync<JobOpeningCandidateProfileMap>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningCandidateProfileMapRepository>();
                return mapper.Map<JobOpeningCandidateProfileMap>(await repo.GetJobOpeningCandidateProfileMapById(Id, userContext));
            }, "GetAllJobOpening", userContext);
        }
        public List<JobOpeningForSearchResultsDto> SearchJobOpenings(JobOpeningForSearchDto job, UserContext userContext)
        {
            var jobRepo = repositoryFactory.Get<IJobOpeningRepository>();
            return jobRepo.SearchJobOpenings(job);
        }

        public async Task<JobOpeningCandidateProfileMapDto> ApplyJob(JobOpeningCandidateProfileMapDtoForInsert? jobOpeningCandidateProfile, IConfigurationValueProvider? authValueProvider, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpeningCandidateProfileMap>(async () =>
            {
                var date = DateTime.UtcNow;
                var _jobOpeningCandidateProfile = mapper.Map<JobOpeningCandidateProfileMap>(jobOpeningCandidateProfile);

                var repo = repositoryFactory.Get<IJobOpeningCandidateProfileMapRepository>();
                _jobOpeningCandidateProfile.Updated = date;
                _jobOpeningCandidateProfile.UpdatedBy = userContext.UserId;
                return await repo.Post(_jobOpeningCandidateProfile, true);
            }, "ApplyJob", userContext);

            try
            {
                var r = await GetJobOpeningCandidateProfileMapById(result.Id, userContext);
                if (r != null && r.JobOpening != null)
                {
                    bool notifyWithResume = r.JobOpening.NotifyWithResume == true;
                    bool notifyOnMap = r.JobOpening.NotifyOnCandidateProfileMap == true;

                    if (notifyWithResume || notifyOnMap)
                    {
                        await SendCandidateApplicationNotification(r, jobOpeningCandidateProfile, includeAttachment: notifyWithResume);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification for JobOpeningCandidateProfileMap Id: {Id}", result.Id);
            }

            return mapper.Map<JobOpeningCandidateProfileMapDto>(result);
        }
        public async Task<JobOpeningCandidateProfileMapDto> UpdateJobStatus(long Id, short CandidateProfileMappingStatusId, UserContext userContext)
        {
            
            var result = await ExecuteAsync<JobOpeningCandidateProfileMap>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo1 = repositoryFactory.Get<IJobOpeningCandidateProfileMapRepository>();

                var _jobOpeningCandidateProfile = await repo1.Get(Id);

                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                _jobOpeningCandidateProfile.Updated = date;
                _jobOpeningCandidateProfile.UpdatedBy = userContext.UserId;
                _jobOpeningCandidateProfile.CandidateProfileMappingStatusId= CandidateProfileMappingStatusId;
                await repo.Put(Id, _jobOpeningCandidateProfile, true);
                return _jobOpeningCandidateProfile;
            }, "UpdateJobStatus", userContext);

            return mapper.Map<JobOpeningCandidateProfileMapDto>(result);

        }
        public async Task<JobOpeningCandidateProfileMapDto> UpdateProfileAsRead(long Id, UserContext userContext)
        {

            var result = await ExecuteAsync<JobOpeningCandidateProfileMap>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo1 = repositoryFactory.Get<IJobOpeningCandidateProfileMapRepository>();

                var _jobOpeningCandidateProfile = await repo1.Get(Id);

                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                _jobOpeningCandidateProfile.Updated = date;
                _jobOpeningCandidateProfile.UpdatedBy = userContext.UserId;
                _jobOpeningCandidateProfile.IsRead = true;
                await repo.Put(Id, _jobOpeningCandidateProfile, true);
                return _jobOpeningCandidateProfile;
            }, "UpdateProfileAsRead", userContext);

            return mapper.Map<JobOpeningCandidateProfileMapDto>(result);

        }
        
        public async Task<JobOpeningDto> ActivateDeactivateJob(long Id, bool ActiveDeactive, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpening>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo = repositoryFactory.Get<IJobOpeningRepository>();

                var _jobOpening = await repo.Get(Id);

                _jobOpening.Updated = date;
                _jobOpening.UpdatedBy = userContext.UserId;
                _jobOpening.Active = ActiveDeactive;
                await repo.Put(Id, _jobOpening, true);
                return _jobOpening;
            }, "ActivateDeactivateJob", userContext);

            return mapper.Map<JobOpeningDto>(result);
        }

        public async Task<JobOpeningProfileConsultancyCommentDto> AddJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDtoForInsert jobOpeningProfileConsultancyComment, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpeningProfileConsultancyComment>(async () =>
            {
                var date = DateTime.UtcNow;
                var _job = mapper.Map<JobOpeningProfileConsultancyComment>(jobOpeningProfileConsultancyComment);

                var repo = repositoryFactory.Get<IJobOpeningProfileConsultancyCommentRepository>();
                _job.Updated = date;
                _job.UpdatedBy = userContext.UserId;
                return await repo.Post(_job, true);
            }, "AddJobOpeningProfileConsultancyComment", userContext);

            return mapper.Map<JobOpeningProfileConsultancyCommentDto>(result);
        }

        public async Task<JobOpeningProfileConsultancyCommentDto> UpdateJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDto jobOpeningProfileConsultancyComment, UserContext userContext)
        {
            var result = await ExecuteAsync<JobOpeningProfileConsultancyComment>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo1 = repositoryFactory.Get<IJobOpeningProfileConsultancyCommentRepository>();

                var _jobOpeningProfileConsultancyComment = mapper.Map<JobOpeningProfileConsultancyComment>(repo1.Get(jobOpeningProfileConsultancyComment.Id));

                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                _jobOpeningProfileConsultancyComment.Updated = date;
                _jobOpeningProfileConsultancyComment.UpdatedBy = userContext.UserId;
                _jobOpeningProfileConsultancyComment.Comment = jobOpeningProfileConsultancyComment.Comment;
                await repo.Put(jobOpeningProfileConsultancyComment.Id, _jobOpeningProfileConsultancyComment, true);
                return _jobOpeningProfileConsultancyComment;
            }, "UpdateJobOpeningProfileConsultancyComment", userContext);

            return mapper.Map<JobOpeningProfileConsultancyCommentDto>(result);
        }


        public async Task<List<JobOpeningDto>> GetJobOpeningsByConsultancyID(long ConsultancyId, UserContext userContext)
        {
            return await ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return mapper.Map<List<JobOpeningDto>>(await repo.GetJobOpeningsByConsultancyID(ConsultancyId));
            }, "GetJobOpeningsByConsultancyID", userContext);
        }

        public async Task<List<JobOpeningDto>> GetJobOpeningsByConsultancyUserID(long ConsultancyUserId, string? publicprofileID, UserContext userContext)
        {
            return await ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return mapper.Map<List<JobOpeningDto>>(await repo.GetJobOpeningsByConsultancyUserID(ConsultancyUserId, publicprofileID));
            }, "GetJobOpeningsByConsultancyUserID", userContext);
        }

        public async Task<List<JobOpeningProfileConsultancyCommentDto>> GetJobOpeningProfileConsultancyComment(long JobopeningCandidateProfileMapId, UserContext userContext)
        {
            return await ExecuteAsync<List<JobOpeningProfileConsultancyCommentDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return mapper.Map<List<JobOpeningProfileConsultancyCommentDto>>(await repo.GetJobOpeningProfileConsultancyComment(JobopeningCandidateProfileMapId, userContext));
            }, "GetJobOpeningProfileConsultancyComment", userContext);
        }

        public async Task<List<JobOpeningProfileMapSummary1>> GetJobOpeningProfileMapSummary(long ConsultancyUserId, UserContext userContext)
        {
            var repo = repositoryFactory.Get<IJobOpeningRepository>();
            var locationRepo = repositoryFactory.Get<IJobOpeningLocationsRepository>();
            var details = repo.GetJobOpeningProfileMapSummary(ConsultancyUserId);

            if (details != null && details.Count > 0)
            {
                var jobIds = details.Select(x => x.JobOpeningId).Distinct().ToList();
                var locations = await locationRepo.GetJobOpeningLocationsByJobIds(jobIds);
                var locationDtos = mapper.Map<List<JobOpeningLocationDto>>(locations);
                var locationLookup = locationDtos.GroupBy(x => x.JobOpeningId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in details)
                {
                    if (locationLookup.TryGetValue(item.JobOpeningId, out var locs))
                    {
                        item.JobOpeningLocation = locs;
                    }
                    else
                    {
                        item.JobOpeningLocation = new List<JobOpeningLocationDto>();
                    }
                }
            }

            return details ?? new List<JobOpeningProfileMapSummary1>();
        }

        public async Task<List<JobOpeningSummary>> GetCountJobsPostedByConsultancyUserId(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext)
        {
            var repo = repositoryFactory.Get<IJobOpeningRepository>();
            var locationRepo = repositoryFactory.Get<IJobOpeningRepository>();
            return await repo.GetCountJobsPostedByConsultancyUserId(ConsultancyUserId, FromDate, ToDate);
           
        }

        public async Task<List<JobOpeningSummary>> GetCountActiveJobsAsOfToday(long ConsultancyUserId, UserContext userContext)
        {
            var repo = repositoryFactory.Get<IJobOpeningRepository>();
            return await repo.GetCountActiveJobsAsOfToday(ConsultancyUserId, userContext);
        }
        public async Task<List<JobOpeningCandidateProfileDetailMapDto>> GetAllResumeReceivedByJobId(long JobOpeningId, UserContext userContext)
        {
            return await ExecuteAsync<List<JobOpeningCandidateProfileDetailMapDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningCandidateProfileMapRepository>();
                return mapper.Map<List<JobOpeningCandidateProfileDetailMapDto>>(await repo.GetAllResumeReceivedByJobId(JobOpeningId));
            }, "GetAllResumeReceivedByJobId", userContext);
        }

        public async Task DeleteJobOpeningSkillsByJobId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningSkillsRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidateProfileSkillByProfileId", userContext);
        }
        public async Task DeleteJobOpeningVisaMapsByJobId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningVisaMapsRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteJobOpeningVisaMapsByJobId", userContext);
        }

        public async Task DeleteJobOpeningLocationsByJobId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningLocationsRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteJobOpeningLocationsByJobId", userContext);
        }
       
        public async Task DeleteJobOpeningEmploymentTypesByJobId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningEmploymentTypesRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteJobOpeningEmploymentTypesByJobId", userContext);
        }

        public async Task DeleteJobOpeningJobTypesByJobId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningJobTypesRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteJobOpeningJobTypesByJobId", userContext);
        }

        public async Task<List<JobOpeningSkill>> AddJobOpeningSkills(List<JobOpeningSkill> jobOpeningSkills, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningSkillsRepository>();
                return await skillRepo.AddRange(jobOpeningSkills, userContext);
            }, "AddJobOpeningSkills", userContext);
        }

        public async Task<List<JobOpeningVisaMap>> AddJobOpeningVisaMaps(List<JobOpeningVisaMap> jobOpeningVisaMaps, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningVisaMapsRepository>();
                return await skillRepo.AddRange(jobOpeningVisaMaps, userContext);
            }, "AddJobOpeningVisaMaps", userContext);
        }

        public async Task<List<JobOpeningLocation>> AddJobOpeningLocations(List<JobOpeningLocation> jobOpeningLocations, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningLocationsRepository>();
                return await skillRepo.AddRange(jobOpeningLocations, userContext);
            }, "AddJobOpeningLocations", userContext);
        }

        public async Task<List<JobOpeningEmploymentType>> AddJobOpeningEmploymentTypes(List<JobOpeningEmploymentType> jobOpeningEmploymentTypes, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningEmploymentTypesRepository>();
                return await skillRepo.AddRange(jobOpeningEmploymentTypes, userContext);
            }, "AddJobOpeningEmploymentTypes", userContext);
        }
        public async Task<List<JobOpeningJobType>> AddJobOpeningJobTypes(List<JobOpeningJobType> jobOpeningJobTypes, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<IJobOpeningJobTypesRepository>();
                return await skillRepo.AddRange(jobOpeningJobTypes, userContext);
            }, "AddJobOpeningJobTypes", userContext);
        }

        public async Task<RecruiterStatsDto> GetRecruiterStats(long consultancyUserId, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<IJobOpeningRepository>();
                return await repo.GetRecruiterStats(consultancyUserId, userContext);
            }, "GetRecruiterStats", userContext);
        }

        private async Task ResolveCustomSkillsForInsert(JobOpeningForInsertDto jobOpening, UserContext userContext)
        {
            if (jobOpening.JobOpeningSkills == null || !jobOpening.JobOpeningSkills.Any()) return;
            var skillsRepo = repositoryFactory.Get<ISkillsRepository>();
            foreach (var skillDto in jobOpening.JobOpeningSkills)
            {
                if ((skillDto.SkillId == null || skillDto.SkillId == 0) && !string.IsNullOrWhiteSpace(skillDto.Name))
                {
                    var skill = await skillsRepo.EnsureSkillExists(skillDto.Name, userContext.UserId);
                    skillDto.SkillId = (int)skill.Id;
                }
            }
        }

        private async Task ResolveCustomSkillsForUpdate(JobOpeningDtoForUpdate jobOpening, UserContext userContext)
        {
            if (jobOpening.JobOpeningSkills == null || !jobOpening.JobOpeningSkills.Any()) return;
            var skillsRepo = repositoryFactory.Get<ISkillsRepository>();
            foreach (var skillDto in jobOpening.JobOpeningSkills)
            {
                if ((skillDto.SkillId == null || skillDto.SkillId == 0) && !string.IsNullOrWhiteSpace(skillDto.Name))
                {
                    var skill = await skillsRepo.EnsureSkillExists(skillDto.Name, userContext.UserId);
                    skillDto.SkillId = (int)skill.Id;
                }
            }
        }

        private async Task SendCandidateApplicationNotification(JobOpeningCandidateProfileMap r, JobOpeningCandidateProfileMapDtoForInsert? insertDto, bool includeAttachment)
        {
            var emailService = serviceProvider?.GetService<IResendEmailService>();
            if (emailService == null) return;

            string? recruiterEmail = r.ConsultancyUser?.User?.Email;
            if (string.IsNullOrWhiteSpace(recruiterEmail)) return;

            string jobTitle = !string.IsNullOrWhiteSpace(r.JobOpening?.Name) ? r.JobOpening.Name : $"Job #{r.JobOpeningId}";
            string candidateName = r.CandidateProfile != null && !string.IsNullOrWhiteSpace(r.CandidateProfile.CandidateName)
                ? r.CandidateProfile.CandidateName
                : "A Candidate";

            byte[]? resumeBytes = null;
            string? attachmentName = null;

            if (includeAttachment)
            {
                // Resolve resume file name safely
                string fileName = "";
                if (insertDto != null && !string.IsNullOrWhiteSpace(insertDto.Doc))
                {
                    fileName = insertDto.Doc;
                }
                else if (r.CandidateProfile?.CandidateDocuments != null && r.CandidateProfile.CandidateDocuments.Any())
                {
                    var doc = r.CandidateProfile.CandidateDocuments.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.Doc));
                    if (doc != null && !string.IsNullOrWhiteSpace(doc.Doc))
                    {
                        fileName = doc.Doc;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(r.CandidateProfile?.Resume))
                {
                    fileName = r.CandidateProfile.Resume;
                }

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    try
                    {
                        var fileManager = managerFactory.Get<IFileManager>();
                        using var stream = await fileManager.Get(fileName, "resumes");
                        if (stream != null)
                        {
                            using var ms = new MemoryStream();
                            await stream.CopyToAsync(ms);
                            resumeBytes = ms.ToArray();

                            string ext = Path.GetExtension(fileName);
                            if (string.IsNullOrWhiteSpace(ext)) ext = ".pdf";
                            string sanitizedCandidate = string.Join("_", candidateName.Split(Path.GetInvalidFileNameChars()));
                            attachmentName = $"{sanitizedCandidate}_Resume{ext}";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch resume file '{FileName}' for candidate application notification.", fileName);
                    }
                }
            }

            string totalExp = r.CandidateProfile?.TotalExp != null ? $"{r.CandidateProfile.TotalExp} years" : "Not specified";
            string candidateTitle = !string.IsNullOrWhiteSpace(r.CandidateProfile?.Title) ? r.CandidateProfile.Title : "Applicant";

            // Build skills list
            string skillsHtml = "";
            if (r.CandidateProfile?.CandidateProfileSkills != null && r.CandidateProfile.CandidateProfileSkills.Any())
            {
                var skillBadges = r.CandidateProfile.CandidateProfileSkills
                    .Where(s => s.Skill != null && !string.IsNullOrWhiteSpace(s.Skill.Name))
                    .Select(s => $"<span style='display:inline-block; background:#EEF2FF; color:#4F46E5; padding:4px 10px; border-radius:12px; font-size:12px; font-weight:600; margin:2px;'>{s.Skill.Name}</span>");

                if (skillBadges.Any())
                {
                    skillsHtml = string.Join(" ", skillBadges);
                }
            }

            string subject = $"New Candidate Application: {candidateName} for {jobTitle}";

            string statusBannerHtml;
            if (includeAttachment && resumeBytes != null && resumeBytes.Length > 0)
            {
                statusBannerHtml = $@"
                    <div style='background-color: #ecfdf5; border: 1px solid #a7f3d0; padding: 12px 16px; border-radius: 6px; margin-bottom: 20px; color: #065f46; font-size: 14px;'>
                        <strong>📎 Resume Attached:</strong> The candidate's resume (<code>{attachmentName}</code>) is attached to this email for your review.
                    </div>";
            }
            else
            {
                statusBannerHtml = $@"
                    <div style='background-color: #eff6ff; border: 1px solid #bfdbfe; padding: 12px 16px; border-radius: 6px; margin-bottom: 20px; color: #1e40af; font-size: 14px;'>
                        <strong>🔔 Application Update:</strong> A new candidate has applied to your job posting. Log in to your ChatHire portal to view their full profile and download their resume.
                    </div>";
            }

            string htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #e5e7eb; border-radius: 8px; color: #1f2937;'>
                    <div style='border-bottom: 2px solid #4F46E5; padding-bottom: 12px; margin-bottom: 20px;'>
                        <h2 style='color: #4F46E5; margin: 0;'>New Job Application Received</h2>
                        <p style='color: #6b7280; font-size: 14px; margin: 4px 0 0 0;'>Application for <strong>{jobTitle}</strong> (Job ID: #{r.JobOpeningId})</p>
                    </div>

                    <div style='background-color: #f9fafb; padding: 16px; border-radius: 6px; margin-bottom: 20px;'>
                        <h3 style='margin-top: 0; color: #111827; font-size: 16px;'>Candidate Summary</h3>
                        <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                            <tr>
                                <td style='padding: 6px 0; color: #6b7280; width: 140px;'><strong>Name:</strong></td>
                                <td style='padding: 6px 0; color: #111827;'>{candidateName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #6b7280;'><strong>Current Title:</strong></td>
                                <td style='padding: 6px 0; color: #111827;'>{candidateTitle}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #6b7280;'><strong>Experience:</strong></td>
                                <td style='padding: 6px 0; color: #111827;'>{totalExp}</td>
                            </tr>
                            {(string.IsNullOrEmpty(skillsHtml) ? "" : $@"
                            <tr>
                                <td style='padding: 6px 0; color: #6b7280; vertical-align: top;'><strong>Skills:</strong></td>
                                <td style='padding: 6px 0;'>{skillsHtml}</td>
                            </tr>")}
                        </table>
                    </div>

                    {statusBannerHtml}

                    <div style='color: #6b7280; font-size: 13px; line-height: 1.5;'>
                        <p>You can review this candidate and manage your job postings directly on your <a href='https://chathire.com' style='color: #4F46E5; text-decoration: underline;'>ChatHire Dashboard</a>.</p>
                    </div>

                    <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 24px 0 16px 0;' />
                    <p style='color: #9ca3af; font-size: 12px; text-align: center; margin: 0;'>&copy; {DateTime.UtcNow.Year} ChatHire. All rights reserved.</p>
                </div>";

            await emailService.SendEmailWithAttachmentAsync(recruiterEmail, subject, htmlContent, attachmentName ?? "", resumeBytes);
        }
    }
}