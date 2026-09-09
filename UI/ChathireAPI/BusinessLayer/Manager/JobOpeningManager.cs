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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Configuration;
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

            var r = await GetJobOpeningCandidateProfileMapById(result.Id, userContext); 

            if (r.JobOpening.NotifyWithResume.HasValue && r.JobOpening.NotifyWithResume.Value
                && (!jobOpeningCandidateProfile.Doc.IsNullOrEmpty() || !r.CandidateProfile.CandidateDocuments.FirstOrDefault().Doc.IsNullOrEmpty()))
            {
                string fileName = "";
                if (!jobOpeningCandidateProfile.Doc.IsNullOrEmpty())
                    fileName = jobOpeningCandidateProfile.Doc;
                else if(r.CandidateProfile.CandidateDocuments.FirstOrDefault().Doc.IsNullOrEmpty())
                    fileName = r.CandidateProfile.CandidateDocuments.FirstOrDefault().Doc;

                if(fileName != "")
                {
                    string email = r.ConsultancyUser.User.Email;
                    string subject = "Resume For JobId:" + result.JobOpeningId.ToString() ;
                    string plainTextFormat = r.JobOpeningId.ToString() + r.JobOpening.Description;
                    var mgr = managerFactory.Get<ICommonManager>();
                    mgr.SendEmailWithAttachment(email, fileName, "resumes", authValueProvider, subject, "", plainTextFormat, userContext);
                }
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
            var details =  repo.GetJobOpeningProfileMapSummary(ConsultancyUserId);
            var rtn = new List<JobOpeningProfileMapSummary1>();

            foreach (var item in details)
            {
                item.JobOpeningLocation = mapper.Map<List<JobOpeningLocationDto>>(await locationRepo.GetJobOpeningLocationByJobId(item.JobOpeningId));
                rtn.Add(item);
            }

            return rtn;
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
    }
}