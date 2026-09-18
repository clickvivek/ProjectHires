using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.Security;
using Middleware.Shared;
using Utility.Configuration;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobOpeningController : BaseCtrler<JobOpeningController>
    {
        EFContexts dbcontext;

        public JobOpeningController(IServiceProvider serviceProvider, ILogger<JobOpeningController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
            dbcontext = serviceProvider.GetService<EFContexts>();
        }

        [HttpPost]
        [Route("ApplyWithResume")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningCandidateProfileMapDto>> ApplyJobWithResume([FromForm] JobOpeningCandidateProfileMapDtoForInsertWithResume jobOpeningCandidateProfile)
        {
            return ExecuteAsync<JobOpeningCandidateProfileMapDto>(async () =>
            {
                string fileNameResume = "";
                var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");

                if (jobOpeningCandidateProfile.resume != null && (jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert.CandidateProfileId == null))
                {
                    var fileManager = managerFactory.Get<IFileManager>();
                    fileNameResume = await fileManager.Upload(jobOpeningCandidateProfile.resume, "resumes");
                    if (jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert != null)
                    {
                        jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert.OriginalDocName = jobOpeningCandidateProfile.resume.ImageFile?.FileName;
                    }
                }
                if (jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert != null && !string.IsNullOrEmpty(fileNameResume))
                {
                    jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert.Doc = fileNameResume;
                }

                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.ApplyJob(jobOpeningCandidateProfile.jobOpeningCandidateProfileMapDtoForInsert, authValueProvider, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Apply")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningCandidateProfileMapDto>> ApplyJob(JobOpeningCandidateProfileMapDtoForInsert jobOpeningCandidateProfile)
        {
            var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
            return ExecuteAsync<JobOpeningCandidateProfileMapDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.ApplyJob(jobOpeningCandidateProfile, authValueProvider, GetDummyUserContext());
            });
        }


        [HttpPut]
        [Route("ChangeCandidateProfileMappingStatusId")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningCandidateProfileMapDto>> UpdateJobStatus(long Id, short CandidateProfileMappingStatusId)
        {
            return ExecuteAsync<JobOpeningCandidateProfileMapDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.UpdateJobStatus(Id, CandidateProfileMappingStatusId, GetUserContext());
            });
        }

        [HttpPut]
        [Route("UpdateProfileAsRead")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningCandidateProfileMapDto>> UpdateProfileAsRead(long Id)
        {
            return ExecuteAsync<JobOpeningCandidateProfileMapDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.UpdateProfileAsRead(Id, GetUserContext());
            });
        }

        [HttpPut]
        [Route("ActivateDeactivateJob")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningDto>> ActivateDeactivateJob(long Id, bool ActiveDeactive)
        {
            return ExecuteAsync<JobOpeningDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.ActivateDeactivateJob(Id, ActiveDeactive, GetUserContext());
            });
        }

        [HttpPost]
        [Route("AddJobProfileComment")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningProfileConsultancyCommentDto>> AddJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDtoForInsert jobOpeningProfileConsultancyComment)
        {
            return ExecuteAsync<JobOpeningProfileConsultancyCommentDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.AddJobOpeningProfileConsultancyComment(jobOpeningProfileConsultancyComment, GetDummyUserContext());
            });
        }


        [HttpPut]
        [Route("UpdateJobProfileComment")]
        //[ApiAuthorize("ApplyJob")]
        public Task<Result<JobOpeningProfileConsultancyCommentDto>> UpdateJobOpeningProfileConsultancyComment(JobOpeningProfileConsultancyCommentDto jobOpeningProfileConsultancyComment)
        {
            return ExecuteAsync<JobOpeningProfileConsultancyCommentDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.UpdateJobOpeningProfileConsultancyComment(jobOpeningProfileConsultancyComment, GetUserContext());
            });
        }

        [HttpDelete]
        [Route("DeleteJobProfileComment")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobProfileComment(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobProfileComment(Id, GetUserContext());

                return true;
            });
        }


        [HttpPost]
        [Route("Add")]
        //[ApiAuthorize("AddJobOpening")]
        public Task<Result<JobOpeningDto>> AddJobOpening(JobOpeningForInsertDto jobOpening)
        {
            UserContext user = GetUserContext();
            //List<UserSubscriptionPlanDto> subDetail = new List<UserSubscriptionPlanDto>();
            //subDetail = ExecuteAsync<UserSubscriptionPlanDto>(async () =>
            //{
            //    var subscriptionManager = managerFactory.Get<ISubscriptionManager>();

            //    return await (List<UserSubscriptionPlanDto>)subscriptionManager.GetUserSubscriptionPlanByUserId(user.UserId, user);
            //});
            var subscriptionManager = managerFactory.Get<ISubscriptionManager>();
            var subDetail = subscriptionManager.GetUserSubscriptionPlanByUserId(user.UserId, user);

            if (subDetail != null && subDetail.Result.Count > 0)
            {
                if (subDetail.Result[0].ActualJobPosting <= subDetail.Result[0].NoOfUsedJobPosting)
                {
                    throw new Exception("Subscription Plan Exceeded Limit: Your current subscription plan has exceeded its allocated limit. Please upgrade your plan to continue accessing this service.");
                }
            }
            return ExecuteAsync<JobOpeningDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.AddJobOpening(jobOpening, subDetail.Result, GetUserContext());
            });
        }

        [HttpPut]
        [Route("Update")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<JobOpeningDto>> UpdateJobOpening(JobOpeningDtoForUpdate profile)
        {
            return ExecuteAsync<JobOpeningDto>(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                return await jobOpeningManager.UpdateJobOpening(profile, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("JobOpeningById")]
        public Task<Result<JobOpeningDto>> GetJobOpeningById(long Id)
        {
            return ExecuteAsync<JobOpeningDto>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetJobOpeningById(Id, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("JobOpeningsByConsultancyID")]
        public Task<Result<List<JobOpeningDto>>> GetJobOpeningsByConsultancyID(long ConsultancyId)
        {
            return ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetJobOpeningsByConsultancyID(ConsultancyId, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("JobOpeningsByConsultancyUserID")]
        public Task<Result<List<JobOpeningDto>>> GetJobOpeningsByConsultancyUserID(long ConsultancyUserID, string? publicprofileID)
        {
            return ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetJobOpeningsByConsultancyUserID(ConsultancyUserID, publicprofileID, GetDummyUserContext());
            });
        }


        [HttpGet]
        [Route("GetJobOpeningProfileConsultancyComment")]
        public Task<Result<List<JobOpeningProfileConsultancyCommentDto>>> GetJobOpeningProfileConsultancyComment(long JobopeningCandidateProfileMapId)
        {
            return ExecuteAsync<List<JobOpeningProfileConsultancyCommentDto>>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetJobOpeningProfileConsultancyComment(JobopeningCandidateProfileMapId, GetDummyUserContext());
            });
        }


        [HttpGet]
        [Route("AllJobOpening")]
        public Task<Result<List<JobOpeningDto>>> GetAllJobOpening()
        {
            return ExecuteAsync<List<JobOpeningDto>>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetAllJobOpening(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("SearchJobOpenings")]
        public List<JobOpeningForSearchResultsDto> SearchJobOpenings([FromQuery] JobOpeningForSearchDto job)
        {
            var mgr = managerFactory.Get<IJobOpeningManager>();

            return mgr.SearchJobOpenings(job, GetDummyUserContext());

        }

        [HttpDelete]
        [Route("JobOpeningSkills")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpeningSkill(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpeningSkill(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("JobOpeningEmploymentType")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpeningEmploymentType(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpeningEmploymentType(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("JobOpeningVisaMap")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpeningVisaMap(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpeningVisaMap(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("JobOpeningJobType")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpeningJobType(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpeningJobType(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("JobOpeningLocation")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpeningLocation(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpeningLocation(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("DeleteJobOpening")]
        //[ApiAuthorize("UpdateJobOpening")]
        public Task<Result<Boolean>> DeleteJobOpening(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

                await jobOpeningManager.DeleteJobOpening(Id, GetUserContext());

                return true;
            });
        }


        //[HttpDelete]
        //[Route("DeleteJobOpeningLocation")]
        ////[ApiAuthorize("UpdateJobOpening")]
        //public Task<Result<Boolean>> DeleteJobOpeningLocation(long Id)
        //{
        //    return ExecuteAsync(async () =>
        //    {
        //        var jobOpeningManager = managerFactory.Get<IJobOpeningManager>();

        //        await jobOpeningManager.DeleteJobOpeningLocation(Id, GetUserContext());

        //        return true;
        //    });
        //}

        [HttpGet]
        [Route("GetJobOpeningProfileMapSummary")]
        public async Task<List<JobOpeningProfileMapSummary1>> GetJobOpeningProfileMapSummary(long ConsultancyUserId)
        {
            var mgr = managerFactory.Get<IJobOpeningManager>();

            return await mgr.GetJobOpeningProfileMapSummary(ConsultancyUserId, GetDummyUserContext());
        }

        [HttpGet]
        [Route("GetCountJobsPostedByConsultancyUserId")]
        public async Task<List<JobOpeningSummary>> GetCountJobsPostedByConsultancyUserId(long ConsultancyUserId, DateTime FromDate, DateTime ToDate)
        {
            var mgr = managerFactory.Get<IJobOpeningManager>();

            return await mgr.GetCountJobsPostedByConsultancyUserId(ConsultancyUserId, FromDate, ToDate, GetDummyUserContext());
        }

        [HttpGet]
        [Route("GetCountActiveJobsAsOfToday")]
        public async Task<List<JobOpeningSummary>> GetCountActiveJobsAsOfToday(long ConsultancyUserId)
        {
            var mgr = managerFactory.Get<IJobOpeningManager>();

            return await mgr.GetCountActiveJobsAsOfToday(ConsultancyUserId, GetDummyUserContext());
        }

        //Task<List<JobOpeningCandidateProfileDetailMapDto>> GetAllResumeReceivedByJobId(long JobOpeningId, UserContext userContext)
        [HttpGet]
        [Route("GetAllResumeReceivedByJobId")]
        public async Task<Result<List<JobOpeningCandidateProfileDetailMapDto>>> GetAllResumeReceivedByJobId(long JobOpeningId)
        {
            return await ExecuteAsync<List<JobOpeningCandidateProfileDetailMapDto>>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();

                return await mgr.GetAllResumeReceivedByJobId(JobOpeningId, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("GetRecruiterStats")]
        public Task<Result<RecruiterStatsDto>> GetRecruiterStats(long consultancyUserId)
        {
            return ExecuteAsync<RecruiterStatsDto>(async () =>
            {
                var mgr = managerFactory.Get<IJobOpeningManager>();
                return await mgr.GetRecruiterStats(consultancyUserId, GetDummyUserContext());
            });
        }

    }

}