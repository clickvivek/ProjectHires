using AutoMapper;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.Security;
using Middleware.Shared;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultancyController : BaseCtrler<ConsultancyController>
    {
        EFContexts dbcontext = new EFContexts();

        public ConsultancyController(IServiceProvider serviceProvider, ILogger<ConsultancyController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
        }

        [HttpPost]
        [Route("Add")]
        //[ApiAuthorize("AddConsultancy")]
        public Task<Result<ConsultancyDto>> AddConsultancy([FromForm] ConsultancyForInsertDtoWithImage consultancy)
        {
            return ExecuteAsync<ConsultancyDto>(async () =>
            {
                var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

                
                if (consultancy.img != null)
                {
                    string fileName = "";
                    var fileManager = managerFactory.Get<IFileManager>();
                    fileName = await fileManager.Upload(consultancy.img, "profilepic");
                    consultancy.consultancyForInsertDto.Logo = fileName;
                }

                return await ConsultancyManager.AddConsultancy(consultancy.consultancyForInsertDto, GetDummyUserContext());
            });
        }

       
        [HttpPut]
        [Route("Update")]
        //[ApiAuthorize("UpdateConsultancy")]
        public Task<Result<ConsultancyDto>> UpdateConsultancy(ConsultancyDto consultancy, [FromForm] FileModel model)
        {
            return ExecuteAsync<ConsultancyDto>(async () =>
            {
                var ConsultancyManager = managerFactory.Get<IConsultancyManager>();
                if (model.ImageFile != null)
                {
                    string fileName = "";
                    var fileManager = managerFactory.Get<IFileManager>();
                    fileName = await fileManager.Upload(model, "profilepic");
                    consultancy.Logo = fileName;
                }
                return await ConsultancyManager.UpdateConsultancy(consultancy, GetUserContext());
            });
        }

        [HttpGet]
        [Route("ConsultancyById")]
        public Task<Result<ConsultancyDto>> GetConsultancyById(long Id)
        {
            return ExecuteAsync<ConsultancyDto>(async () =>
            {
                var mgr = managerFactory.Get<IConsultancyManager>();

                return await mgr.GetConsultancyById(Id, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("AllConsultancy")]
        public Task<Result<List<ConsultancyDto>>> GetAllConsultancy()
        {
            return ExecuteAsync<List<ConsultancyDto>>(async () =>
            {
                var mgr = managerFactory.Get<IConsultancyManager>();

                return await mgr.GetAllConsultancy(GetDummyUserContext());
            });
        }


        
    [HttpGet]
    [Route("SearchConsultancies")]
    public Task<Result<List<ConsultancyDto>>> SearchConsultancies([FromQuery] string conname)
    {
       

            return ExecuteAsync<List<ConsultancyDto>>(async () =>
            {
                var mgr = managerFactory.Get<IConsultancyManager>();

                return await mgr.SearchConsultancies(conname, GetDummyUserContext());
            });

        }
        /*

   [HttpDelete]
   [Route("ConsultancySkills")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancySkill(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancySkill(Id, GetUserContext());

           return true;
       });
   }

   [HttpDelete]
   [Route("ConsultancyEmploymentType")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancyEmploymentType(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancyEmploymentType(Id, GetUserContext());

           return true;
       });
   }

   [HttpDelete]
   [Route("ConsultancyVisaMap")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancyVisaMap(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancyVisaMap(Id, GetUserContext());

           return true;
       });
   }

   [HttpDelete]
   [Route("ConsultancyJobType")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancyJobType(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancyJobType(Id, GetUserContext());

           return true;
       });
   }

   [HttpDelete]
   [Route("ConsultancyLocation")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancyLocation(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancyLocation(Id, GetUserContext());

           return true;
       });
   }

   [HttpDelete]
   [Route("Consultancy")]
   //[ApiAuthorize("UpdateConsultancy")]
   public Task<Result<Boolean>> DeleteConsultancy(long Id)
   {
       return ExecuteAsync(async () =>
       {
           var ConsultancyManager = managerFactory.Get<IConsultancyManager>();

           await ConsultancyManager.DeleteConsultancyLocation(Id, GetUserContext());

           return true;
       });
   }
   */
    }

}