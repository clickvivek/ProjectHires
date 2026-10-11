using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Mail;
using Utility.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BusinessLayer.Manager
{
    public interface ICommonManager
    {
        Task<List<UserTypeDto>> GetUserType(UserContext userContext);
        Task<List<CategoryDto>> GetCategory(UserContext userContext);
        Task<List<DomainDto>> GetDomain(UserContext userContext);
        Task<List<EmploymentTypeDto>> GetEmploymentType(UserContext userContext);
        Task<List<JobTypeDto>> GetJobType(UserContext userContext, string? country = null);
        Task<List<StatusDto>> GetStatus(UserContext userContext);
        Task<List<CandidateProfileMappingStatusDto>> GetCandidateProfileMappingStatus(UserContext userContext);
        Task<List<VisaDto>> GetVisa(UserContext userContext, string? country = null);
        Task<List<SkillDto>> GetSkills(string? skill, UserContext userContext);
        Task<SkillsAdminSummaryDto> GetSkillsAdminSummary(bool? isUserDefined, bool? active, string? searchTerm, UserContext userContext);
        Task<BulkAddSkillsResultDto> BulkAddSkills(BulkAddSkillsRequestDto request, UserContext userContext);
        Task<SkillDto> AddSkill(CreateSkillRequestDto request, UserContext userContext);
        Task<SkillDto?> UpdateSkill(UpdateSkillRequestDto request, UserContext userContext);
        Task<bool> ToggleSkillStatus(int id, bool active, UserContext userContext);
        Task<bool> ConvertSkillUserDefined(int id, bool isUserDefined, UserContext userContext);
        Task<DeleteSkillResultDto> DeleteSkill(int id, UserContext userContext);
        Task<List<CityDto>> GetCity(string? searchString, int? state, bool isState, string? country, UserContext userContext);

        Task<List<StateDto>> GetState(string? searchString, UserContext userContext);

        Task<List<CandidateAvailabilityDto>> GetCandidateAvailability(UserContext userContext);

        void SendEmail(String? emailAddress, object? dynamicEmailData, string EmailTemplateId, IConfigurationValueProvider? authValueProvider, UserContext userContext);
        void SendEmailWithAttachment(String? emailAddress, string? fileName, string containerName, IConfigurationValueProvider? authValueProvider, string? subject, string? plainTextContent, string? htmlContent, UserContext userContext);


    }
    public class CommonManager : BaseManager<CommonManager>, ICommonManager
    {
        public CommonManager(IServiceProvider provider, ILogger<CommonManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task<List<UserTypeDto>> GetUserType(UserContext userContext)
        {
            return await ExecuteAsync<List<UserTypeDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserTypeRepository>();
                return mapper.Map<List<UserTypeDto>>(await repo.GetAll<UserType>());
            }, "GetUserType", userContext);
        }

        public async Task<List<CategoryDto>> GetCategory(UserContext userContext)
        {
            return await ExecuteAsync<List<CategoryDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ICategoryRepository>();
                return mapper.Map<List<CategoryDto>>(await repo.GetAll<Category>());
            }, "GetCategory", userContext);
        }

        public async Task<List<DomainDto>> GetDomain(UserContext userContext)
        {
            return await ExecuteAsync<List<DomainDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IDomainRepository>();
                return mapper.Map<List<DomainDto>>(await repo.GetAll<Domain>());
            }, "GetDomain", userContext);
        }

        public async Task<List<EmploymentTypeDto>> GetEmploymentType(UserContext userContext)
        {
            return await ExecuteAsync<List<EmploymentTypeDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IEmploymentTypeRepository>();
                return mapper.Map<List<EmploymentTypeDto>>(await repo.GetAll<EmploymentType>());
            }, "GetEmploymentType", userContext);
        }

        public async Task<List<JobTypeDto>> GetJobType(UserContext userContext, string? country = null)
        {
            return await ExecuteAsync<List<JobTypeDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IJobTypeRepository>();
                var all = await repo.GetAll<JobType>();
                if (!string.IsNullOrWhiteSpace(country))
                {
                    var cNorm = country.Trim().ToUpper();
                    if (cNorm == "USA" || cNorm == "US")
                        all = all.Where(j => string.IsNullOrEmpty(j.Country) || j.Country.Equals("USA", StringComparison.OrdinalIgnoreCase) || j.Country.Equals("US", StringComparison.OrdinalIgnoreCase)).ToList();
                    else if (cNorm == "CANADA" || cNorm == "CA")
                        all = all.Where(j => !string.IsNullOrEmpty(j.Country) && (j.Country.Equals("Canada", StringComparison.OrdinalIgnoreCase) || j.Country.Equals("CA", StringComparison.OrdinalIgnoreCase))).ToList();
                    else
                        all = all.Where(j => !string.IsNullOrEmpty(j.Country) && j.Country.Equals(cNorm, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return mapper.Map<List<JobTypeDto>>(all);
            }, "GetJobType", userContext);
        }

        public async Task<List<StatusDto>> GetStatus(UserContext userContext)
        {
            return await ExecuteAsync<List<StatusDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IStatusRepository>();
                return mapper.Map<List<StatusDto>>(await repo.GetAll<Status>());
            }, "GetStatus", userContext);
        }

        public async Task<List<CandidateProfileMappingStatusDto>> GetCandidateProfileMappingStatus(UserContext userContext)
        {
            return await ExecuteAsync<List<CandidateProfileMappingStatusDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateProfileRepository>();
                return mapper.Map<List<CandidateProfileMappingStatusDto>>(await repo.GetAll<CandidateProfileMappingStatus>());
            }, "GetStatus", userContext);
        }

        public async Task<List<VisaDto>> GetVisa(UserContext userContext, string? country = null)
        {
            return await ExecuteAsync<List<VisaDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IVisaRepository>();
                var all = await repo.GetAll<Visa>();
                if (!string.IsNullOrWhiteSpace(country))
                {
                    var cNorm = country.Trim().ToUpper();
                    if (cNorm == "USA" || cNorm == "US")
                        all = all.Where(v => string.IsNullOrEmpty(v.Country) || v.Country.Equals("USA", StringComparison.OrdinalIgnoreCase) || v.Country.Equals("US", StringComparison.OrdinalIgnoreCase)).ToList();
                    else if (cNorm == "CANADA" || cNorm == "CA")
                        all = all.Where(v => !string.IsNullOrEmpty(v.Country) && (v.Country.Equals("Canada", StringComparison.OrdinalIgnoreCase) || v.Country.Equals("CA", StringComparison.OrdinalIgnoreCase))).ToList();
                    else
                        all = all.Where(v => !string.IsNullOrEmpty(v.Country) && v.Country.Equals(cNorm, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return mapper.Map<List<VisaDto>>(all);
            }, "GetVisa", userContext);
        }

        public async Task<List<SkillDto>> GetSkills(string? skill, UserContext userContext)
        {
            return await ExecuteAsync<List<SkillDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return mapper.Map<List<SkillDto>>(await repo.GetSkills(skill));
            }, "GetSkills", userContext);
        }

        public async Task<SkillsAdminSummaryDto> GetSkillsAdminSummary(bool? isUserDefined, bool? active, string? searchTerm, UserContext userContext)
        {
            return await ExecuteAsync<SkillsAdminSummaryDto>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return await repo.GetSkillsAdminSummaryAsync(isUserDefined, active, searchTerm);
            }, "GetSkillsAdminSummary", userContext);
        }

        public async Task<BulkAddSkillsResultDto> BulkAddSkills(BulkAddSkillsRequestDto request, UserContext userContext)
        {
            return await ExecuteAsync<BulkAddSkillsResultDto>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return await repo.BulkAddSkillsAsync(request, userContext.UserId);
            }, "BulkAddSkills", userContext);
        }

        public async Task<SkillDto> AddSkill(CreateSkillRequestDto request, UserContext userContext)
        {
            return await ExecuteAsync<SkillDto>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                var skill = await repo.AddSkillAsync(request, userContext.UserId);
                return mapper.Map<SkillDto>(skill);
            }, "AddSkill", userContext);
        }

        public async Task<SkillDto?> UpdateSkill(UpdateSkillRequestDto request, UserContext userContext)
        {
            return await ExecuteAsync<SkillDto?>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                var skill = await repo.UpdateSkillAsync(request, userContext.UserId);
                return skill != null ? mapper.Map<SkillDto>(skill) : null;
            }, "UpdateSkill", userContext);
        }

        public async Task<bool> ToggleSkillStatus(int id, bool active, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return await repo.ToggleSkillStatusAsync(id, active, userContext.UserId);
            }, "ToggleSkillStatus", userContext);
        }

        public async Task<bool> ConvertSkillUserDefined(int id, bool isUserDefined, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return await repo.ConvertSkillUserDefinedAsync(id, isUserDefined, userContext.UserId);
            }, "ConvertSkillUserDefined", userContext);
        }

        public async Task<DeleteSkillResultDto> DeleteSkill(int id, UserContext userContext)
        {
            return await ExecuteAsync<DeleteSkillResultDto>(async () =>
            {
                var repo = repositoryFactory.Get<ISkillsRepository>();
                return await repo.DeleteSkillAsync(id);
            }, "DeleteSkill", userContext);
        }

        public async Task<List<CityDto>> GetCity(string? searchString, int? state, bool isState, string? country, UserContext userContext)
        {
            return await ExecuteAsync<List<CityDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ICityRepository>();
                var result = await repo.GetLocation(searchString, state, isState, country);
                var uniqueResult = new List<City>();
                foreach (var city in result)
                {
                    if (isState)
                    {
                        if (uniqueResult.Where(u => u.IdState == city.IdState).FirstOrDefault() == null
                            && city.IsState == true)
                            uniqueResult.Add(city);

                    }
                    else if (uniqueResult.Where(u => u.Id == city.Id || (u.City1 == city.City1 && u.IdState == city.IdState && u.Zip == city.Zip)).FirstOrDefault() == null)
                    {
                        uniqueResult.Add(city);
                    }
                }
                return mapper.Map<List<CityDto>>(uniqueResult);
            }, "GetCity", userContext);
        }

        public async Task<List<StateDto>> GetState(string? searchString, UserContext userContext)
        {
            return await ExecuteAsync<List<StateDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IStateRepository>();
                return mapper.Map<List<StateDto>>(await repo.GetState(searchString));
            }, "GetState", userContext);
        }
        public async Task<List<CandidateAvailabilityDto>> GetCandidateAvailability(UserContext userContext)
        {
            return await ExecuteAsync<List<CandidateAvailabilityDto>>(async () =>
            {
                var repo = repositoryFactory.Get<ICandidateAvailabilityRepository>();
                return mapper.Map<List<CandidateAvailabilityDto>>(await repo.GetAll());
            }, "GetCandidateAvailability", userContext);
        }

        public async void SendEmail(String? emailAddress, object? dynamicEmailData, string EmailTemplateId, IConfigurationValueProvider? authValueProvider, UserContext userContext)
        {
            //var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
            var client = new SendGridClient(authValueProvider.GetValue("APIKey"));
            var from = new EmailAddress(authValueProvider.GetValue("SenderEmail"), "Hires Co");
            var to = new EmailAddress(emailAddress, ""); ;
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, EmailTemplateId, dynamicEmailData);

            var response = await client.SendEmailAsync(msg);
        }

        public async void SendEmailWithAttachment(String? emailAddress, string? fileName, string containerName, IConfigurationValueProvider? authValueProvider, string? subject, string? plainTextContent, string? htmlContent, UserContext userContext)
        {
            var client = new SendGridClient(authValueProvider.GetValue("APIKey"));
            var from = new EmailAddress(authValueProvider.GetValue("SenderEmail"), "Hires Co");
            var to = new EmailAddress(emailAddress, ""); ;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var fileManager = managerFactory.Get<IFileManager>();

            var blob = fileManager.GetBlobClient(fileName, containerName);
            
            await msg.AddAttachmentAsync("Resume."+ fileName.Split(".")[1]  , blob.OpenRead());

            var response = await client.SendEmailAsync(msg);
        }

        //myMessage.AddAttachment(Server.MapPath(@"~\img\logo.png"));

    }
}