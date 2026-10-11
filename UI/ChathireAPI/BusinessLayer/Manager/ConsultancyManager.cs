using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface IConsultancyManager
    {
        Task<ConsultancyDto> AddConsultancy(ConsultancyForInsertDto consultancy, UserContext userContext);
        Task<List<ConsultancyDto>> BulkAddConsultancies(List<ConsultancyForInsertDto> consultancies, UserContext userContext);
        Task<ConsultancyDto> UpdateConsultancy(ConsultancyDto consultancy, UserContext userContext);
        Task<List<ConsultancyDto>> GetAllConsultancy(UserContext userContext);
        Task<ConsultancyDto> GetConsultancyById(long Id, UserContext userContext);

        Task<List<ConsultancyDto>> SearchConsultancies(string job, UserContext userContext);
        Task<ConsultancyDto> CheckAndUpdateLogo(long consultancyId, string? linkedinUrl, UserContext userContext);
    }
    public class ConsultancyManager : BaseManager<ConsultancyManager>, IConsultancyManager
    {

        public ConsultancyManager(IServiceProvider provider, ILogger<ConsultancyManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task<ConsultancyDto> CheckAndUpdateLogo(long consultancyId, string? linkedinUrl, UserContext userContext)
        {
            var result = await ExecuteAsync<Consultancy>(async () =>
            {
                var repo = repositoryFactory.Get<IConsultancyRepository>();
                var existing = await repo.Get(consultancyId);
                if (existing == null)
                {
                    throw new Exception($"Consultancy with ID {consultancyId} not found.");
                }

                string targetLinkedin = !string.IsNullOrWhiteSpace(linkedinUrl) ? linkedinUrl.Trim() : (existing.Linkedin?.Trim() ?? string.Empty);

                var linkedInScraper = serviceProvider.GetService(typeof(BusinessLayer.Services.ILinkedInScraperService)) as BusinessLayer.Services.ILinkedInScraperService;
                var companyUrlScraper = serviceProvider.GetService(typeof(BusinessLayer.Services.ICompanyUrlScraperService)) as BusinessLayer.Services.ICompanyUrlScraperService;

                string updatedLogo = string.Empty;

                // 1. Try scraping from LinkedIn if LinkedIn URL is present
                if (!string.IsNullOrWhiteSpace(targetLinkedin) && linkedInScraper != null)
                {
                    try
                    {
                        var linkedInResult = await linkedInScraper.ScrapeCompanyAsync(targetLinkedin);
                        if (linkedInResult != null && !string.IsNullOrWhiteSpace(linkedInResult.AzureLogoFileName))
                        {
                            updatedLogo = linkedInResult.AzureLogoFileName;
                            if (string.IsNullOrWhiteSpace(existing.Linkedin) && !string.IsNullOrWhiteSpace(linkedInResult.NormalizedLinkedinUrl))
                            {
                                existing.Linkedin = linkedInResult.NormalizedLinkedinUrl;
                            }
                            if (string.IsNullOrWhiteSpace(existing.Website) && !string.IsNullOrWhiteSpace(linkedInResult.Website))
                            {
                                existing.Website = linkedInResult.Website;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "LinkedIn logo check failed for company {CompanyId} / {Url}", consultancyId, targetLinkedin);
                    }
                }

                // 2. If no logo found from LinkedIn, try scraping company website/domain
                if (string.IsNullOrWhiteSpace(updatedLogo) && companyUrlScraper != null)
                {
                    string targetWebsite = existing.Website?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(targetWebsite) && !string.IsNullOrWhiteSpace(existing.Website2))
                    {
                        targetWebsite = existing.Website2.Trim();
                    }
                    if (string.IsNullOrWhiteSpace(targetWebsite) && !string.IsNullOrWhiteSpace(existing.Domainname) && !existing.Domainname.Contains("IT Services") && !existing.Domainname.Contains(" "))
                    {
                        targetWebsite = existing.Domainname.Trim();
                    }
                    if (string.IsNullOrWhiteSpace(targetWebsite) && !string.IsNullOrWhiteSpace(existing.Name))
                    {
                        string cleanSlug = existing.Name.Trim().ToLowerInvariant()
                            .Replace(" llc", "")
                            .Replace(" inc", "")
                            .Replace(" corp", "")
                            .Replace(" ltd", "")
                            .Replace(" ", "")
                            .Replace("-", "")
                            .Replace("_", "")
                            .Replace(",", "")
                            .Replace(".", "");
                        targetWebsite = $"https://www.{cleanSlug}.com";
                    }

                    if (!string.IsNullOrWhiteSpace(targetWebsite))
                    {
                        try
                        {
                            var urlResult = await companyUrlScraper.ScrapeCompanyUrlAsync(targetWebsite);
                            if (urlResult != null)
                            {
                                if (!string.IsNullOrWhiteSpace(urlResult.AzureLogoFileName))
                                {
                                    updatedLogo = urlResult.AzureLogoFileName;
                                }
                                if (string.IsNullOrWhiteSpace(existing.Linkedin) && !string.IsNullOrWhiteSpace(urlResult.LinkedinUrl))
                                {
                                    existing.Linkedin = urlResult.LinkedinUrl;
                                }
                                if (string.IsNullOrWhiteSpace(existing.Website) && !string.IsNullOrWhiteSpace(urlResult.NormalizedWebsiteUrl))
                                {
                                    existing.Website = urlResult.NormalizedWebsiteUrl;
                                }
                                else if (!string.IsNullOrWhiteSpace(urlResult.NormalizedWebsiteUrl) && !string.Equals(existing.Website, urlResult.NormalizedWebsiteUrl, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(existing.Website2))
                                {
                                    existing.Website2 = urlResult.NormalizedWebsiteUrl;
                                }
                                if ((string.IsNullOrWhiteSpace(existing.Domainname) || existing.Domainname.Contains("IT Services") || existing.Domainname.Contains(" ")) && !string.IsNullOrWhiteSpace(urlResult.DomainName))
                                {
                                    existing.Domainname = urlResult.DomainName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Website logo check failed for company {CompanyId} / {Url}", consultancyId, targetWebsite);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(updatedLogo))
                {
                    throw new Exception("Could not find or extract a valid logo from the company LinkedIn page or website.");
                }

                existing.Logo = updatedLogo;
                existing.Updated = DateTime.UtcNow;
                if (userContext != null && userContext.UserId > 0)
                {
                    existing.UpdatedBy = userContext.UserId;
                }

                await repo.Put(existing.Id, existing, true);
                return existing;
            }, "CheckAndUpdateLogo", userContext);

            return mapper.Map<ConsultancyDto>(result);
        }

        public async Task<ConsultancyDto> AddConsultancy(ConsultancyForInsertDto consultancy, UserContext userContext)
        {
            var result = await ExecuteAsync<Consultancy>(async () =>
            {
                var date = DateTime.UtcNow;
                var _consultancy = mapper.Map<Consultancy>(consultancy);
                var repo = repositoryFactory.Get<IConsultancyRepository>();

                string domain = DataAccessLayer.Repository.ConsultancyRepository.ExtractNormalizedDomain(_consultancy.Domainname ?? _consultancy.Website);
                if (string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(_consultancy.Email) && _consultancy.Email.Contains('@'))
                {
                    domain = _consultancy.Email.Split('@')[1].ToLowerInvariant().Trim();
                }

                // Check duplicate by domain, website, linkedin, or name
                var existing = await repo.FindExistingDuplicateAsync(domain, _consultancy.Website, _consultancy.Linkedin, _consultancy.Name);
                if (existing != null)
                {
                    // Update existing record if new info is available
                    if (!string.IsNullOrWhiteSpace(_consultancy.Website) && string.IsNullOrWhiteSpace(existing.Website)) existing.Website = _consultancy.Website;
                    if (!string.IsNullOrWhiteSpace(_consultancy.Website2) && string.IsNullOrWhiteSpace(existing.Website2)) existing.Website2 = _consultancy.Website2;
                    if (!string.IsNullOrWhiteSpace(domain) && (string.IsNullOrWhiteSpace(existing.Domainname) || existing.Domainname.Contains("IT Services") || existing.Domainname.Contains(" "))) existing.Domainname = domain;
                    if (!string.IsNullOrWhiteSpace(_consultancy.Linkedin) && string.IsNullOrWhiteSpace(existing.Linkedin)) existing.Linkedin = _consultancy.Linkedin;
                    if (!string.IsNullOrWhiteSpace(_consultancy.Logo) && string.IsNullOrWhiteSpace(existing.Logo)) existing.Logo = _consultancy.Logo;
                    if (!string.IsNullOrWhiteSpace(_consultancy.Phone) && string.IsNullOrWhiteSpace(existing.Phone)) existing.Phone = _consultancy.Phone;
                    if (!string.IsNullOrWhiteSpace(_consultancy.Address) && string.IsNullOrWhiteSpace(existing.Address)) existing.Address = _consultancy.Address;
                    existing.Active = true;
                    existing.Updated = date;
                    if (userContext != null && userContext.UserId > 0) existing.UpdatedBy = userContext.UserId;

                    await repo.Put(existing.Id, existing, true);
                    return existing;
                }

                _consultancy.Domainname = !string.IsNullOrWhiteSpace(domain) ? domain : "IT Services ,Consulting & Staffing";
                _consultancy.Updated = date;
                _consultancy.UpdatedBy = userContext != null && userContext.UserId > 0 ? userContext.UserId : -1;
                _consultancy.StatusId = 1;
                _consultancy.Active = true;
                _consultancy.IsDirectCompany = _consultancy.IsDirectCompany ?? false;

                return await repo.Post(_consultancy, true);
            }, "AddConsultancy", userContext);

            return mapper.Map<ConsultancyDto>(result);
        }

        public async Task<List<ConsultancyDto>> BulkAddConsultancies(List<ConsultancyForInsertDto> consultancies, UserContext userContext)
        {
            var resultList = new List<ConsultancyDto>();
            if (consultancies == null || consultancies.Count == 0) return resultList;

            var repo = repositoryFactory.Get<IConsultancyRepository>();
            var date = DateTime.UtcNow;
            var processedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processedLinkedins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in consultancies)
            {
                var inserted = await ExecuteAsync<Consultancy>(async () =>
                {
                    var entity = mapper.Map<Consultancy>(item);
                    string domain = DataAccessLayer.Repository.ConsultancyRepository.ExtractNormalizedDomain(entity.Domainname ?? entity.Website);
                    if (string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(entity.Email) && entity.Email.Contains('@'))
                    {
                        domain = entity.Email.Split('@')[1].ToLowerInvariant().Trim();
                    }

                    string linkedinSlug = DataAccessLayer.Repository.ConsultancyRepository.ExtractLinkedInSlug(entity.Linkedin);

                    // Check duplicate within batch
                    if (!string.IsNullOrEmpty(domain) && processedDomains.Contains(domain))
                    {
                        var dup = await repo.FindExistingDuplicateAsync(domain, entity.Website, entity.Linkedin, entity.Name);
                        return dup;
                    }
                    if (!string.IsNullOrEmpty(linkedinSlug) && processedLinkedins.Contains(linkedinSlug))
                    {
                        var dup = await repo.FindExistingDuplicateAsync(domain, entity.Website, entity.Linkedin, entity.Name);
                        return dup;
                    }

                    // Check duplicate in DB
                    var existing = await repo.FindExistingDuplicateAsync(domain, entity.Website, entity.Linkedin, entity.Name);
                    if (existing != null)
                    {
                        if (!string.IsNullOrWhiteSpace(entity.Website) && string.IsNullOrWhiteSpace(existing.Website)) existing.Website = entity.Website;
                        if (!string.IsNullOrWhiteSpace(entity.Website2) && string.IsNullOrWhiteSpace(existing.Website2)) existing.Website2 = entity.Website2;
                        if (!string.IsNullOrWhiteSpace(domain) && (string.IsNullOrWhiteSpace(existing.Domainname) || existing.Domainname.Contains("IT Services") || existing.Domainname.Contains(" "))) existing.Domainname = domain;
                        if (!string.IsNullOrWhiteSpace(entity.Linkedin) && string.IsNullOrWhiteSpace(existing.Linkedin)) existing.Linkedin = entity.Linkedin;
                        if (!string.IsNullOrWhiteSpace(entity.Logo) && string.IsNullOrWhiteSpace(existing.Logo)) existing.Logo = entity.Logo;
                        if (!string.IsNullOrWhiteSpace(entity.Phone) && string.IsNullOrWhiteSpace(existing.Phone)) existing.Phone = entity.Phone;
                        if (!string.IsNullOrWhiteSpace(entity.Address) && string.IsNullOrWhiteSpace(existing.Address)) existing.Address = entity.Address;
                        existing.Active = true;
                        existing.Updated = date;
                        if (userContext != null && userContext.UserId > 0) existing.UpdatedBy = userContext.UserId;

                        await repo.Put(existing.Id, existing, true);
                        if (!string.IsNullOrEmpty(domain)) processedDomains.Add(domain);
                        if (!string.IsNullOrEmpty(linkedinSlug)) processedLinkedins.Add(linkedinSlug);
                        return existing;
                    }

                    entity.Domainname = !string.IsNullOrWhiteSpace(domain) ? domain : Guid.NewGuid().ToString("N").Substring(0, 8);
                    entity.Updated = date;
                    entity.UpdatedBy = userContext.UserId;
                    if (!entity.Active.HasValue) entity.Active = true;
                    entity.StatusId = 1;
                    entity.IsDirectCompany = entity.IsDirectCompany ?? false;

                    var created = await repo.Post(entity, true);
                    if (!string.IsNullOrEmpty(domain)) processedDomains.Add(domain);
                    if (!string.IsNullOrEmpty(linkedinSlug)) processedLinkedins.Add(linkedinSlug);
                    return created;
                }, "BulkAddConsultancyItem", userContext);

                if (inserted != null)
                {
                    resultList.Add(mapper.Map<ConsultancyDto>(inserted));
                }
            }

            return resultList;
        }

        public async Task<ConsultancyDto> UpdateConsultancy(ConsultancyDto consultancy, UserContext userContext)
        {
            var result = await ExecuteAsync<Consultancy>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo = repositoryFactory.Get<IConsultancyRepository>();
                var existing = await repo.Get(consultancy.Id);
                if (existing != null)
                {
                    existing.Name = consultancy.Name;
                    existing.Email = consultancy.Email;
                    existing.Address = consultancy.Address;
                    existing.Phone = consultancy.Phone;
                    existing.Active = consultancy.Active;
                    existing.Website = consultancy.Website;
                    existing.Website2 = consultancy.Website2;
                    existing.Linkedin = consultancy.Linkedin;
                    existing.Logo = consultancy.Logo;
                    existing.StatusId = consultancy.StatusId;
                    if (consultancy.IsDirectCompany.HasValue) existing.IsDirectCompany = consultancy.IsDirectCompany.Value;
                    if (consultancy.CityId.HasValue) existing.CityId = consultancy.CityId;
                    if (!string.IsNullOrWhiteSpace(consultancy.Domainname))
                    {
                        existing.Domainname = consultancy.Domainname;
                    }
                    else if (string.IsNullOrWhiteSpace(existing.Domainname))
                    {
                        if (!string.IsNullOrWhiteSpace(existing.Website))
                        {
                            existing.Domainname = existing.Website.Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim('/', ' ', '\\');
                        }
                        else
                        {
                            existing.Domainname = existing.Id.ToString();
                        }
                    }
                    existing.Updated = date;
                    existing.UpdatedBy = userContext.UserId;
                    await repo.Put(existing.Id, existing, true);
                    return existing;
                }
                else
                {
                    var _consultancy = mapper.Map<Consultancy>(consultancy);
                    _consultancy.Updated = date;
                    _consultancy.UpdatedBy = userContext.UserId;
                    if (string.IsNullOrWhiteSpace(_consultancy.Domainname))
                    {
                        _consultancy.Domainname = !string.IsNullOrWhiteSpace(_consultancy.Website)
                            ? _consultancy.Website.Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim('/', ' ', '\\')
                            : _consultancy.Id.ToString();
                    }
                    await repo.Put(_consultancy.Id, _consultancy, true);
                    return _consultancy;
                }
            }, "UpdateConsultancy", userContext);

            return mapper.Map<ConsultancyDto>(result);
        }

        public async Task<List<ConsultancyDto>> GetAllConsultancy(UserContext userContext)
        {
            return await ExecuteAsync<List<ConsultancyDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IConsultancyRepository>();
                return mapper.Map<List<ConsultancyDto>>(await repo.GetAll<Consultancy>());
            }, "GetAllConsultancy", userContext);
        }

        public async Task<ConsultancyDto> GetConsultancyById(long Id, UserContext userContext)
        {
            return await ExecuteAsync<ConsultancyDto>(async () =>
            {
                var repo = repositoryFactory.Get<IConsultancyRepository>();
                return mapper.Map<ConsultancyDto>(await repo.Get(Id));
            }, "GetAllConsultancy", userContext);
        }
        //SearchConsultancies

        public async Task<List<ConsultancyDto>> SearchConsultancies(string conName, UserContext userContext)
        {
            return await ExecuteAsync<List<ConsultancyDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IConsultancyRepository>();
                return mapper.Map<List<ConsultancyDto>>(await repo.SearchConsultancies(conName));
            }, "SearchConsultancies", userContext);
        }

    }
}
