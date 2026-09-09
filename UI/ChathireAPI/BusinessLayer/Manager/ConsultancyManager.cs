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

        public async Task<ConsultancyDto> UpdateConsultancy(ConsultancyDto consultancy, UserContext userContext)
        {
            var result = await ExecuteAsync<Consultancy>(async () =>
            {
                var date = DateTime.UtcNow;
                var _consultancy = mapper.Map<Consultancy>(consultancy);

                var repo = repositoryFactory.Get<IConsultancyRepository>();
                _consultancy.Updated = date;
                _consultancy.UpdatedBy = userContext.UserId;
                await repo.Put(_consultancy.Id, _consultancy, true);
                return _consultancy;
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
