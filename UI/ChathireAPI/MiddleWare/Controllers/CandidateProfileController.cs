using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Middleware.Security;
using Middleware.Shared;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CandidateProfileController : BaseCtrler<UserController>
    {
        public CandidateProfileController(IServiceProvider serviceProvider, ILogger<UserController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
        }


        [HttpGet]
        [Route("Get")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<CandidateProfileDto>> GetCandidateProfile(long Id)
        {
            return ExecuteAsync<CandidateProfileDto>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.GetCandidateProfile(Id, GetUserContext());
            });
        }

        [HttpGet]
        [Route("GetByUser")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<List<CandidateProfileDto>>> GetCandidateProfileByUser(long? userId, long? id)
        {
            return ExecuteAsync<List<CandidateProfileDto>>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.GetCandidateProfileByUser(userId, id, GetUserContext());
            });
        }

        [HttpGet]
        [Route("GetByConsultancyUser")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<List<CandidateProfileDto>>> GetByConsultancyUserID(long? consultancyUserId, string? publicprofileID)
        {
            return ExecuteAsync<List<CandidateProfileDto>>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.GetByConsultancyUserID(consultancyUserId, publicprofileID, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("GetByConsultancyUserSimplelist")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<List<CandidateProfileSimplelistDto>>> GetByConsultancyUserSimplelist(long? consultancyUserId, bool? isActive, short? statusId)
        {
            return ExecuteAsync<List<CandidateProfileSimplelistDto>>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.GetByConsultancyUserSimplelist(consultancyUserId, isActive, statusId,GetDummyUserContext());
            });
        }
        
        [HttpGet]
        [Route("GetCountResumesReceivedTodayByConsultancyUserID")]
        public async Task<int> GetCountResumesReceivedTodayByConsultancyUserID(long ConsultancyUserId)
        {
            var mgr = managerFactory.Get<ICandidateProfileManager>();

            return await mgr.GetCountResumesReceivedTodayByConsultancyUserID(ConsultancyUserId, GetDummyUserContext());
        }

        [HttpGet]
        [Route("GetCountResumesReceivedByDateByConsultancyUserID")]
        public async Task<int> GetCountResumesReceivedByDateByConsultancyUserID(long ConsultancyUserId, DateTime FromDate, DateTime ToDate)
        {
            var mgr = managerFactory.Get<ICandidateProfileManager>();

            return await mgr.GetCountResumesReceivedByDateByConsultancyUserID(ConsultancyUserId, FromDate, ToDate,  GetDummyUserContext());
        }
        
        [HttpGet]
        [Route("GetountunreadresumesByConsultancyUserID")]
        public async Task<int> GetCountUnreadResumesByConsultancyUserID(long ConsultancyUserId)
        {
            var mgr = managerFactory.Get<ICandidateProfileManager>();
            return await mgr.GetCountUnreadResumesByConsultancyUserID(ConsultancyUserId, GetDummyUserContext());
        }


        [HttpGet]
        [Route("GetBenchSalesStats")]
        public Task<Result<BenchSalesStatsDto>> GetBenchSalesStats(long userId, long? consultancyUserId)
        {
            return ExecuteAsync<BenchSalesStatsDto>(async () =>
            {
                var mgr = managerFactory.Get<ICandidateProfileManager>();
                return await mgr.GetBenchSalesStats(userId, consultancyUserId, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Add")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<CandidateProfileDto>> AddCandidateProfile(CandidateProfileForInsertDto profile) 
        {
            return ExecuteAsync<CandidateProfileDto>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.AddCandidateProfile(profile, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("AddWithResume")]
        //[ApiAuthorize("AddCandidateProfile")]
        public Task<Result<CandidateProfileDto>> AddCandidateProfileWithResume([FromForm] CandidateProfileForInsertDtoWithResume profile)
        {
            return ExecuteAsync<CandidateProfileDto>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                CandidateProfileDto candidateProfileDto = await candidateProfileManager.AddCandidateProfile(profile.candidateProfileForInsertDto, GetDummyUserContext());

                string fileNameResume = "";
                bool rtnResumeVal;

                if (profile.resume.ImageFile != null && candidateProfileDto.Id > 0)
                {
                    var fileManager = managerFactory.Get<IFileManager>();
                    fileNameResume = await fileManager.Upload(profile.resume, "resumes");
                    rtnResumeVal = await candidateProfileManager.UploadImage(fileNameResume, (long)candidateProfileDto.Id, 1, GetDummyUserContext());

                }

                return candidateProfileDto;
            });
        }

        [HttpPut]
        [Route("Update")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<CandidateProfileDto>> UpdateCandidateProfile(CandidateProfileDtoForUpdate profile)
        {
            return ExecuteAsync<CandidateProfileDto>(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                return await candidateProfileManager.UpdateCandidateProfile(profile, GetDummyUserContext());
            });
        }

        [HttpDelete]
        [Route("CandidateProfileSkills")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidateProfileSkill(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidateProfileSkill(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("CandidateProfileEmploymentType")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidateProfileEmploymentType(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidateProfileEmploymentType(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("CandidateProfileDomain")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidateProfileDomain(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidateProfileDomain(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("CandidatePrefJobType")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidatePrefJobType(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidatePrefJobType(Id, GetUserContext());

                return true;
            });
        }

        [HttpDelete]
        [Route("CandidatePrefLocation")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidatePrefLocation(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidatePrefLocation(Id, GetUserContext());

                return true;
            }); 
        }

        [HttpDelete]
        [Route("DeleteCandidateProfile")]
        //[ApiAuthorize("DeleteCandidateProfile")]
        public Task<Result<Boolean>> DeleteCandidateProfile(long Id)
        {
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                await candidateProfileManager.DeleteCandidateProfile(Id, GetUserContext());

                return true;
            });
        }

        [HttpPut]
        [Route("ActivateOrDeActivate")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<Boolean>> ActivateOrDeActivateCandidateProfile(long CandidateProfileId,bool? Active, short? StatusId)
        {
            bool rtn = false;

            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();

                rtn = await candidateProfileManager.ActivateOrDeActivateCandidateProfile(CandidateProfileId, Active, StatusId, GetUserContext());
                return rtn;
            });
        }

        [HttpGet]
        [Route("SearchCandidateProfiles")]
        public List<CandidateProfileForSearchResultsDto> SearchCandidateProfile([FromQuery] CandidateProfileForSearchDto candidateProfile)
        {
            var mgr = managerFactory.Get<ICandidateProfileManager>();

            return mgr.SearchCandidateProfile(candidateProfile, GetDummyUserContext());

        }

        [HttpPost]
        [Route("UpdateDocument")]
        //[ApiAuthorize("UpdateCandidateProfile")]
        public Task<Result<string>> UpdateDocument([FromForm] FileModel model, long CandidateProfileId, short DocumentId)
        {
            bool rtn = false;
            return ExecuteAsync(async () =>
            {
                var candidateProfileManager = managerFactory.Get<ICandidateProfileManager>();
                string fileName = "";
                if (model.ImageFile != null)
                {
                    var fileManager = managerFactory.Get<IFileManager>();
                    fileName = await fileManager.Upload(model, "resumes");
                }
                rtn = await candidateProfileManager.UploadImage(fileName, CandidateProfileId, DocumentId, GetDummyUserContext());
                return fileName;
            });
        }

    }
}
