using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchedController : BaseCtrler<SearchedController>
    {
        public SearchedController(IServiceProvider serviceProvider, ILogger<SearchedController> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        [HttpGet]
        [Route("GetRecentSearches")]
        public Task<Result<List<SearchedDto>>> GetRecentSearches([FromQuery] string? searchType = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            return ExecuteAsync<List<SearchedDto>>(async () =>
            {
                var mgr = managerFactory.Get<ISearchedManager>();
                return await mgr.GetRecentSearchesAsync(searchType, page, pageSize);
            });
        }

        [HttpGet]
        [Route("GetSearchesCount")]
        public Task<Result<int>> GetSearchesCount([FromQuery] string? searchType = null)
        {
            return ExecuteAsync<int>(async () =>
            {
                var mgr = managerFactory.Get<ISearchedManager>();
                return await mgr.GetSearchesCountAsync(searchType);
            });
        }

        [HttpGet]
        [Route("GetAnalyticsSummary")]
        public Task<Result<SearchedAnalyticsSummaryDto>> GetAnalyticsSummary()
        {
            return ExecuteAsync<SearchedAnalyticsSummaryDto>(async () =>
            {
                var mgr = managerFactory.Get<ISearchedManager>();
                return await mgr.GetSearchAnalyticsSummaryAsync();
            });
        }
    }
}
