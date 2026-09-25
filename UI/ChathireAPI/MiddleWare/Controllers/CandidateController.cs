using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Security;
using Middleware.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utility.Configuration;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : BaseCtrler<CandidateController>
    {
        public CandidateController(IServiceProvider serviceProvider, ILogger<CandidateController> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        [HttpGet]
        [Route("GetProfile")]
        public Task<Result<DirectCandidateProfileCompositeDto>> GetProfile([FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.GetCandidateFullProfile(targetUserId, context);
            });
        }

        [HttpPost]
        [Route("SaveDetails")]
        public Task<Result<DirectCandidateDetailDto>> SaveDetails([FromBody] DirectCandidateDetailDtoForUpdate detailsDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.SaveDetails(targetUserId, detailsDto, context);
            });
        }

        [HttpPost]
        [Route("UploadResume")]
        public Task<Result<DirectCandidateResumeDto>> UploadResume([FromForm] DirectCandidateResumeUploadDto uploadDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                if (uploadDto.File == null || uploadDto.File.Length == 0)
                {
                    throw new Exception("No resume file uploaded.");
                }

                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.UploadResume(targetUserId, uploadDto.File, uploadDto.IsPrimary, context);
            });
        }

        [HttpGet]
        [Route("GetResumes")]
        public Task<Result<List<DirectCandidateResumeDto>>> GetResumes([FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.GetResumes(targetUserId, context);
            });
        }

        [HttpPut]
        [Route("SetPrimaryResume")]
        public Task<Result<bool>> SetPrimaryResume([FromQuery] long resumeId, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.SetPrimaryResume(targetUserId, resumeId, context);
            });
        }

        [HttpDelete]
        [Route("DeleteResume/{id}")]
        public Task<Result<bool>> DeleteResume(long id, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.DeleteResume(targetUserId, id, context);
            });
        }

        [HttpPost]
        [Route("AddExperience")]
        public Task<Result<DirectCandidateExperienceDto>> AddExperience([FromBody] DirectCandidateExperienceForInsertDto expDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.AddExperience(targetUserId, expDto, context);
            });
        }

        [HttpPut]
        [Route("UpdateExperience/{id}")]
        public Task<Result<DirectCandidateExperienceDto?>> UpdateExperience(long id, [FromBody] DirectCandidateExperienceForInsertDto expDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.UpdateExperience(targetUserId, id, expDto, context);
            });
        }

        [HttpDelete]
        [Route("DeleteExperience/{id}")]
        public Task<Result<bool>> DeleteExperience(long id, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.DeleteExperience(targetUserId, id, context);
            });
        }

        [HttpGet]
        [Route("GetExperiences")]
        public Task<Result<List<DirectCandidateExperienceDto>>> GetExperiences([FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.GetExperiences(targetUserId, context);
            });
        }

        [HttpPost]
        [Route("AddEducation")]
        public Task<Result<DirectCandidateEducationDto>> AddEducation([FromBody] DirectCandidateEducationForInsertDto eduDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.AddEducation(targetUserId, eduDto, context);
            });
        }

        [HttpPut]
        [Route("UpdateEducation/{id}")]
        public Task<Result<DirectCandidateEducationDto?>> UpdateEducation(long id, [FromBody] DirectCandidateEducationForInsertDto eduDto, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.UpdateEducation(targetUserId, id, eduDto, context);
            });
        }

        [HttpDelete]
        [Route("DeleteEducation/{id}")]
        public Task<Result<bool>> DeleteEducation(long id, [FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.DeleteEducation(targetUserId, id, context);
            });
        }

        [HttpGet]
        [Route("GetEducations")]
        public Task<Result<List<DirectCandidateEducationDto>>> GetEducations([FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.GetEducations(targetUserId, context);
            });
        }

        [HttpGet]
        [Route("GetMyApplications")]
        public Task<Result<List<CandidateAppliedJobDto>>> GetMyApplications([FromQuery] long? userId = null)
        {
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.GetCandidateApplications(targetUserId, context);
            });
        }

        [HttpPost]
        [Route("ApplyDirect")]
        public Task<Result<JobOpeningCandidateProfileMapDto>> ApplyDirect([FromBody] DirectCandidateApplyJobDto applyDto, [FromQuery] long? userId = null)
        {
            var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
            return ExecuteAsync(async () =>
            {
                var context = GetUserContext();
                long targetUserId = userId ?? context.UserId;
                var manager = managerFactory.Get<IDirectCandidateManager>();
                return await manager.ApplyDirectJob(targetUserId, applyDto, authValueProvider, context);
            });
        }
    }
}
