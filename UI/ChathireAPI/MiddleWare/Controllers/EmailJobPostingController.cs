using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Security;
using Middleware.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailJobPostingController : BaseCtrler<EmailJobPostingController>
    {
        public EmailJobPostingController(IServiceProvider serviceProvider, ILogger<EmailJobPostingController> logger, IMapper mapper)
            : base(serviceProvider, logger, mapper)
        {
        }

        [HttpPost]
        [Route("InboundWebhook")]
        public Task<Result<EmailJobPostingQueueDto>> InboundWebhook([FromBody] InboundEmailWebhookDto dto)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.ReceiveInboundEmail(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Simulate")]
        public Task<Result<EmailJobPostingQueueDto>> Simulate([FromBody] SimulateInboundEmailDto dto)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.SimulateInboundEmail(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("SimulateHotlist")]
        public Task<Result<EmailJobPostingQueueDto>> SimulateHotlist([FromBody] SimulateInboundHotlistEmailDto dto)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.SimulateInboundHotlistEmail(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("GetAll")]
        public Task<Result<List<EmailJobPostingQueueDto>>> GetAll([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ExecuteAsync<List<EmailJobPostingQueueDto>>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                var filter = new EmailJobPostingFilterDto
                {
                    Status = status,
                    Search = search,
                    Page = page > 0 ? page : 1,
                    PageSize = pageSize > 0 ? pageSize : 20
                };
                return await mgr.GetAllQueueItems(filter, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Count")]
        public Task<Result<int>> GetCount([FromQuery] string? status, [FromQuery] string? search)
        {
            return ExecuteAsync<int>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                var filter = new EmailJobPostingFilterDto { Status = status, Search = search };
                return await mgr.GetTotalQueueCount(filter, context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Stats")]
        public Task<Result<EmailJobPostingStatsDto>> GetStats()
        {
            return ExecuteAsync<EmailJobPostingStatsDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.GetStats(context ?? GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("{id}")]
        public Task<Result<EmailJobPostingQueueDto>> GetById(long id)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                var item = await mgr.GetQueueItemById(id, context ?? GetDummyUserContext());
                if (item == null) throw new KeyNotFoundException($"Queue item with ID {id} not found.");
                return item;
            });
        }

        [HttpPost]
        [Route("Process/{id}")]
        public Task<Result<EmailJobPostingQueueDto>> Process(long id)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.ProcessQueueItem(id, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Approve")]
        public Task<Result<EmailJobPostingQueueDto>> Approve([FromBody] ApproveEmailJobPostingDto dto)
        {
            return ExecuteAsync<EmailJobPostingQueueDto>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.ApproveAndPublish(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("Reject")]
        public Task<Result<bool>> Reject([FromBody] RejectEmailJobPostingDto dto)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.RejectQueueItem(dto, context ?? GetDummyUserContext());
            });
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<Result<bool>> Delete(long id)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IEmailJobPostingManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.DeleteQueueItem(id, context ?? GetDummyUserContext());
            });
        }
    }
}
