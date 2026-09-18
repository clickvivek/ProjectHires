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
    }
    public class ConsultancyManager : BaseManager<ConsultancyManager>, IConsultancyManager
    {

        public ConsultancyManager(IServiceProvider provider, ILogger<ConsultancyManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task<ConsultancyDto> AddConsultancy(ConsultancyForInsertDto consultancy, UserContext userContext)
        {
            var result = await ExecuteAsync<Consultancy>(async () =>
            {
                var date = DateTime.UtcNow;
                var _consultancy = mapper.Map<Consultancy>(consultancy);

                var repo = repositoryFactory.Get<IConsultancyRepository>();
                _consultancy.Updated = date;
                _consultancy.UpdatedBy = userContext.UserId;
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

            foreach (var item in consultancies)
            {
                var inserted = await ExecuteAsync<Consultancy>(async () =>
                {
                    var entity = mapper.Map<Consultancy>(item);
                    entity.Updated = date;
                    entity.UpdatedBy = userContext.UserId;
                    if (!entity.Active.HasValue) entity.Active = true;
                    if (string.IsNullOrWhiteSpace(entity.Domainname))
                    {
                        if (!string.IsNullOrWhiteSpace(entity.Website))
                        {
                            entity.Domainname = entity.Website.Replace("http://", "").Replace("https://", "").Replace("www.", "").Trim('/', ' ', '\\');
                        }
                        else if (!string.IsNullOrWhiteSpace(entity.Email) && entity.Email.Contains('@'))
                        {
                            entity.Domainname = entity.Email.Split('@')[1];
                        }
                        else
                        {
                            entity.Domainname = Guid.NewGuid().ToString("N").Substring(0, 8);
                        }
                    }
                    return await repo.Post(entity, true);
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
                    existing.Linkedin = consultancy.Linkedin;
                    existing.Logo = consultancy.Logo;
                    existing.StatusId = consultancy.StatusId;
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
