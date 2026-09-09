using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLayer.Manager
{
    public interface ICandidateProfileManager
    {
        Task<CandidateProfileDto> GetCandidateProfile(long Id, UserContext userContext);
        Task<List<CandidateProfileDto>> GetCandidateProfileByUser(long? userId, long? Id, UserContext userContext);
        Task<List<CandidateProfileDto>> GetByConsultancyUserID(long? Id, string? publicprofileID, UserContext userContext);
        Task<List<CandidateProfileSimplelistDto>> GetByConsultancyUserSimplelist(long? Id, bool? isActive, short? statusId,UserContext userContext);
        Task<CandidateProfileDto> AddCandidateProfile(CandidateProfileForInsertDto profile, UserContext userContext);
        Task<CandidateProfileDto> UpdateCandidateProfile(CandidateProfileDtoForUpdate profile, UserContext userContext);
        Task DeleteCandidateProfileSkill(long id, UserContext userContext);
        Task DeleteCandidateProfileEmploymentType(long id, UserContext userContext);
        Task DeleteCandidateProfileDomain(long id, UserContext userContext);
        Task DeleteCandidatePrefJobType(long id, UserContext userContext);
        Task DeleteCandidatePrefLocation(long id, UserContext userContext);
        Task DeleteCandidateDocumentByProfileId(long id, UserContext userContext);
        Task<bool> ActivateOrDeActivateCandidateProfile(long CandidateProfileId, bool? Active, short? StatusId, UserContext userContext);
        List<CandidateProfileForSearchResultsDto> SearchCandidateProfile(CandidateProfileForSearchDto job, UserContext userContext);
        Task DeleteCandidateProfile(long id, UserContext userContext);
        //Task AddCandidateProfileSkills(List<CandidateProfileSkill> candidateProfileSkills, UserContext userContext);
        Task<bool> UploadImage(string fileName, long candidateProfileId, short documentId, UserContext userContext);
        Task<int> GetCountResumesReceivedTodayByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
        Task<int> GetCountResumesReceivedByDateByConsultancyUserID(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext);
        Task<int> GetCountUnreadResumesByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
        Task<int> GetActiveCountHotListByConsultancyUserID(long ConsultancyUserId, UserContext userContext);
    }
    public class CandidateProfileManager : BaseManager<CandidateProfileManager>, ICandidateProfileManager
    {
        public CandidateProfileManager(IServiceProvider provider, ILogger<CandidateProfileManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task<List<CandidateProfileDto>> GetCandidateProfileByUser(long? userId, long? Id, UserContext userContext)
        {
            var result = await ExecuteAsync<List<CandidateProfile>>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCandidateProfileByUser(userId, Id, userContext);
            }, "GetCandidateProfileByUser", userContext);

            return mapper.Map<List<CandidateProfileDto>>(result);
        }

        public async Task<int> GetCountResumesReceivedTodayByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var result = await ExecuteAsync<int>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCountResumesReceivedTodayByConsultancyUserID(ConsultancyUserId, userContext);
            }, "GetCountResumesReceivedTodayByConsultancyUserID", userContext);
            return result;
        }

        public async Task<int> GetCountResumesReceivedByDateByConsultancyUserID(long ConsultancyUserId, DateTime FromDate, DateTime ToDate, UserContext userContext)
        {
            var result = await ExecuteAsync<int>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCountResumesReceivedByDateByConsultancyUserID(ConsultancyUserId, FromDate, ToDate, userContext);
            }, "GetCountResumesReceivedByDateByConsultancyUserID", userContext);
            return result;
        }

        public async Task<int> GetCountUnreadResumesByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var result = await ExecuteAsync<int>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCountUnreadResumesByConsultancyUserID(ConsultancyUserId, userContext);
            }, "GetcountunreadresumesByConsultancyUserID", userContext);
            return result;
        }
        public async Task<int> GetActiveCountHotListByConsultancyUserID(long ConsultancyUserId, UserContext userContext)
        {
            var result = await ExecuteAsync<int>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetActiveCountHotListByConsultancyUserID(ConsultancyUserId, userContext);
            }, "GetActiveCountHotListByConsultancyUserID", userContext);
            return result;
        }

        public async Task<List<CandidateProfileDto>> GetByConsultancyUserID(long? Id, string? publicprofileID, UserContext userContext)
        {
            var result = await ExecuteAsync<List<CandidateProfile>>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetByConsultancyUserID(Id, publicprofileID, userContext);
            }, "GetByConsultancyUserID", userContext);

            return mapper.Map<List<CandidateProfileDto>>(result);
        }

        public async Task<List<CandidateProfileSimplelistDto>> GetByConsultancyUserSimplelist(long? Id, bool? isActive, short? statusId,UserContext userContext)
        {
            var result = await ExecuteAsync<List<CandidateProfile>>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetByConsultancyUserSimplelist(Id, isActive, statusId,userContext);
            }, "GetByConsultancyUserSimplelist", userContext);

            return mapper.Map<List<CandidateProfileSimplelistDto>>(result);
        }
        public async Task<CandidateProfileDto> GetCandidateProfile(long Id, UserContext userContext)
        {
            var result = await ExecuteAsync<CandidateProfile>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCandidateProfile(Id, userContext);
            }, "GetCandidateProfile", userContext);

            return mapper.Map<CandidateProfileDto>(result);
        }

        public async Task<CandidateProfileDto> GetCandidateProfileSkillsId(long Id, UserContext userContext)
        {
            var result = await ExecuteAsync<CandidateProfile>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return await repo.GetCandidateProfile(Id, userContext);
            }, "GetCandidateProfile", userContext);

            return mapper.Map<CandidateProfileDto>(result);
        }

        public async Task<CandidateProfileDto> AddCandidateProfile(CandidateProfileForInsertDto profile, UserContext userContext)
        {
            var result = await ExecuteAsync<CandidateProfile>(async () =>
            {
                var date = DateTime.UtcNow;
                var _profile = mapper.Map<CandidateProfile>(profile);

                _profile.CandidateProfileSkills.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _profile.CandidateProfileEmploymentTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _profile.CandidateProfileDomains.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _profile.CandidatePrefJobTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                _profile.CandidatePrefLocations.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });

                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                _profile.Updated = date;
                _profile.UpdatedBy = userContext.UserId;
                return await repo.Post(_profile, true);
            }, "AddCandidateProfile", userContext);

            return mapper.Map<CandidateProfileDto>(result);
        }

        public async Task<CandidateProfileDto> UpdateCandidateProfile(CandidateProfileDtoForUpdate profile, UserContext userContext)
        {
            var result = await ExecuteAsync<CandidateProfile>(async () =>
            {
                var date = DateTime.UtcNow;
                var _profileUpdate = mapper.Map<CandidateProfile>(profile);
                long profileId = 0;
                if(profile.Id !=null || profile.Id > 0)
                {
                    profileId = (long)profile.Id;
                }
                else
                {
                    throw new KeyNotFoundException("Profile Id invalid !");
                }


                _profileUpdate.CandidateProfileSkills.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.CandidateProfileid = profile.Id; });
                await DeleteCandidateProfileSkillByProfileId(profileId, userContext);
                var addCandidateProfileSkills = await AddCandidateProfileSkills(_profileUpdate.CandidateProfileSkills.ToList(), userContext);
                var _candidateProfileSkill = mapper.Map<List<CandidateProfileSkillForUpdateDto>>(addCandidateProfileSkills);

                _profileUpdate.CandidateProfileEmploymentTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                await DeleteCandidateProfileEmploymentTypesByProfileId(profileId,userContext);
                var _candidateProfileEmploymentTypes = mapper.Map<List<CandidateProfileEmploymentTypeDtoForUpdate>>(await AddCandidateProfileEmploymentTypes(_profileUpdate.CandidateProfileEmploymentTypes.ToList(), userContext));

                _profileUpdate.CandidateProfileDomains.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.CandidateProfileId = profile.Id; });
                await DeleteCandidateProfileDomainsByProfileId(profileId, userContext);
                var _candidateProfileDomains = mapper.Map<List<CandidateProfileDomainDtoForUpdate>>(await AddCandidateProfileDomains(_profileUpdate.CandidateProfileDomains.ToList(), userContext));

                _profileUpdate.CandidatePrefLocations.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; });
                await DeleteCandidatePrefLocationsByProfileId(profileId, userContext);
                var _candidatePrefLocations = mapper.Map<List<CandidatePrefLocationDtoForUpdate>>(await AddCandidatePrefLocations(_profileUpdate.CandidatePrefLocations.ToList(), userContext));

                _profileUpdate.CandidatePrefJobTypes.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.CandidateProfileId = profileId; });
                await DeleteCandidatePrefJobTypesByProfileId(profileId, userContext);
                var _candidatePrefJobTypes = mapper.Map<List<CandidatePrefJobTypeDtoForUpdate>>(await AddCandidatePrefJobTypes(_profileUpdate.CandidatePrefJobTypes.ToList(), userContext));

                _profileUpdate.CandidateDocuments.ToList().ForEach(o => { o.Updated = date; o.UpdatedBy = userContext.UserId; o.CandidateProfileId = profileId; });
                await DeleteCandidatePrefJobTypesByProfileId(profileId, userContext);
                var _candidateDocuments = mapper.Map<List<CandidateDocumentDtoForUpdate>>(await AddCandidateDocument(_profileUpdate.CandidateDocuments.ToList(), userContext));

                profile.CandidateProfileSkills = _candidateProfileSkill; 
                profile.CandidateProfileEmploymentTypes = _candidateProfileEmploymentTypes;
                profile.CandidateProfileDomains = _candidateProfileDomains;
                profile.CandidatePrefLocations = _candidatePrefLocations;
                profile.CandidatePrefJobTypes = _candidatePrefJobTypes;
                profile.CandidateDocuments = _candidateDocuments;

                var _profile = mapper.Map<CandidateProfile>(profile);

                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                _profile.Updated = date;
                _profile.UpdatedBy = userContext.UserId;
                await repo.Put(_profile.Id, _profile, true);
                return _profile;
            }, "UpdateCandidateProfile", userContext);

            return mapper.Map<CandidateProfileDto>(result);
        }

        public async Task DeleteCandidateProfileSkill(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileSkillsRepository>();
                await skillRepo.Delete(id, true);
            }, "DeleteCandidateProfileSkill", userContext);
        }

        public async Task DeleteCandidateProfileSkillByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileSkillsRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidateProfileSkillByProfileId", userContext);
        }

        public async Task DeleteCandidateProfileEmploymentTypesByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileEmploymentTypesRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidateProfileEmploymentTypesByProfileId", userContext);
        }

        public async Task DeleteCandidateProfileDomainsByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileDomainsRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidateProfileDomainsByProfileId", userContext);
        }

        public async Task DeleteCandidatePrefLocationsByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefLocationRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidatePrefLocationsByProfileId", userContext);
        }

        public async Task DeleteCandidatePrefJobTypesByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefJobTypesRepository>();
                await skillRepo.RemoveRange(id, userContext);
            }, "DeleteCandidatePrefJobTypesByProfileId", userContext);
        }

        public async Task DeleteCandidateDocumentByProfileId(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateDocumentRepository>();
                await skillRepo.RemoveDocument(id, 1, userContext);
            }, "DeleteCandidateDocumentByProfileId", userContext);
        }

        public async Task<List<CandidateProfileSkill>> AddCandidateProfileSkills(List<CandidateProfileSkill> candidateProfileSkills, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileSkillsRepository>();
                return await skillRepo.AddRange(candidateProfileSkills, userContext);
            }, "AddCandidateProfileSkills", userContext);
        }

        public async Task<List<CandidateProfileEmploymentType>> AddCandidateProfileEmploymentTypes(List<CandidateProfileEmploymentType> candidateProfileEmploymentType, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileEmploymentTypesRepository>();
                return await skillRepo.AddRange(candidateProfileEmploymentType, userContext);
            }, "AddcandidateProfileEmploymentTypes", userContext);
        }

        public async Task<List<CandidatePrefLocation>> AddCandidatePrefLocations(List<CandidatePrefLocation> candidatePrefLocations, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefLocationRepository>();
                return await skillRepo.AddRange(candidatePrefLocations, userContext);
            }, "AddCandidatePrefLocations", userContext);
        }

        public async Task<List<CandidateProfileDomain>> AddCandidateProfileDomains(List<CandidateProfileDomain> candidateProfileDomains, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileDomainsRepository>();
                return await skillRepo.AddRange(candidateProfileDomains, userContext);
            }, "AddCandidateProfileDomains", userContext);
        }

        public async Task<List<CandidatePrefJobType>> AddCandidatePrefJobTypes(List<CandidatePrefJobType> candidatePrefJobTypes, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefJobTypesRepository>();
                return await skillRepo.AddRange(candidatePrefJobTypes, userContext);
            }, "AddCandidatePrefJobTypes", userContext);
        }

        public async Task<List<CandidateDocument>> AddCandidateDocument(List<CandidateDocument> candidateDocument, UserContext userContext)
        {
            return await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateDocumentRepository>();
                return await skillRepo.AddRange(candidateDocument, userContext);
            }, "AddCandidateDocument", userContext);
        }

        public async Task DeleteCandidateProfileEmploymentType(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileEmploymentTypesRepository>();
                await skillRepo.Delete(id, true);
            }, "DeleteCandidateProfileEmploymentType", userContext);
        }

        public async Task DeleteCandidateProfileDomain(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileDomainsRepository>();
                await skillRepo.Delete(id, true);
            }, "DeleteCandidateProfileDomain", userContext);
        }

        public async Task DeleteCandidatePrefJobType(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefJobTypesRepository>();
                await skillRepo.Delete(id, true);
            }, "DeleteCandidatePrefJobType", userContext);
        }

        public async Task DeleteCandidatePrefLocation(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidatePrefLocationRepository>();
                await skillRepo.Delete(id, true);
            }, "DeleteCandidatePrefLocation", userContext);
        }

        public List<CandidateProfileForSearchResultsDto> SearchCandidateProfile(CandidateProfileForSearchDto job, UserContext userContext)
        {
            var jobRepo = repositoryFactory.Get<ICandidateProfileRepository>();

            //if (!job.SearchString.IsNullOrEmpty())
            //{
            //    var skillRepo = repositoryFactory.Get<ISkillsRepository>();
            //    var skills = skillRepo.GetSkills(job.SearchString).Result;
            //    if (skills != null && skills.Count > 0)
            //    {
            //        if (job.skills == null)
            //        {
            //            job.skills = new List<int>();
            //        }
            //        job.skills.AddRange(skills.Select(o => o.Id));
            //    }
            //}
            return jobRepo.SearchCandidateProfile(job);
        }

        public async Task<bool> ActivateOrDeActivateCandidateProfile(long CandidateProfileId, bool? Active, short? StatusId, UserContext userContext)
        {
            bool rtn = false;

            await ExecuteAsync(async () =>
            {
                var skillRepo = repositoryFactory.Get<ICandidateProfileRepository>();
                rtn = await skillRepo.ActivateOrDeActivateCandidateProfile(CandidateProfileId,Active, StatusId, userContext);
            }, "ActivateOrDeActivateCandidateProfile", userContext);

            return rtn;
        }

       
        public async Task DeleteCandidateProfile(long id, UserContext userContext)
        {
            await ExecuteAsync(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                await repo.Delete(id, true);
            }, "DeleteCandidateProfile", userContext);
        }

        public async Task<bool> UploadImage(string model, long candidateProfileId, short documentId, UserContext userContext)
        {
            var repo = repositoryFactory.Get<ICandidateDocumentRepository>();

            await ExecuteAsync(async () =>
            {
                string fileName = await repo.RemoveDocument(candidateProfileId, documentId, userContext);
                if(fileName != null && fileName != "")
                {
                    var fileManager = managerFactory.Get<IFileManager>();
                    await fileManager.Delete(fileName, "resumes");
                }
                
            }, "RemoveDocument", userContext);

            var result = await ExecuteAsync<CandidateDocument>(async () =>
            {
                var date = DateTime.UtcNow;
                var _profile =new CandidateDocument();

                var repo = repositoryFactory.Get<ICandidateDocumentRepository>();
                _profile.Updated = date;
                _profile.UpdatedBy = userContext.UserId;
                _profile.DocumentId = documentId;
                _profile.Doc = model;
                _profile.CandidateProfileId = candidateProfileId;

                return await repo.Post(_profile, true);
            }, "AddCandidateProfile", userContext);

            return true;
        }
    }
}
