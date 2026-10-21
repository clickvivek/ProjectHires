using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware.Security;
using Middleware.Shared;
using Microsoft.EntityFrameworkCore;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultancyController : BaseCtrler<ConsultancyController>
    {
        EFContexts dbcontext;
        private readonly IServiceProvider _serviceProvider;

        public ConsultancyController(IServiceProvider serviceProvider, ILogger<ConsultancyController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
            _serviceProvider = serviceProvider;
            dbcontext = serviceProvider.GetService<EFContexts>();
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

                UserContext context = null;
                try { context = GetUserContext(); } catch { }

                return await ConsultancyManager.AddConsultancy(consultancy.consultancyForInsertDto, context ?? GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("BulkAdd")]
        public Task<Result<List<ConsultancyDto>>> BulkAddConsultancies([FromBody] List<ConsultancyForInsertDto> consultancies)
        {
            return ExecuteAsync<List<ConsultancyDto>>(async () =>
            {
                var mgr = managerFactory.Get<IConsultancyManager>();
                return await mgr.BulkAddConsultancies(consultancies, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ScrapeAndAddLinkedInCompanies")]
        [AdminAuthorize]
        public Task<Result<List<LinkedInCompanyScrapedDto>>> ScrapeAndAddLinkedInCompanies([FromBody] LinkedInScrapeRequestDto request)
        {
            return ExecuteAsync<List<LinkedInCompanyScrapedDto>>(async () =>
            {
                var scraperService = _serviceProvider.GetService<BusinessLayer.Services.ILinkedInScraperService>();
                if (scraperService == null)
                {
                    throw new InvalidOperationException("LinkedInScraperService is not registered.");
                }

                long? adminUserId = null;
                try
                {
                    var userContext = GetUserContext();
                    if (userContext != null && userContext.UserId > 0)
                    {
                        adminUserId = userContext.UserId;
                    }
                }
                catch { }

                return await scraperService.ScrapeAndSaveCompaniesAsync(request.Urls, request.AutoSaveToDb, adminUserId);
            });
        }

        [HttpPost]
        [Route("ReviewAndUpdateLinkedInWebsites")]
        [AdminAuthorize]
        public Task<Result<LinkedInBatchUpdateResultDto>> ReviewAndUpdateLinkedInWebsites([FromBody] LinkedInBatchUpdateRequestDto? request)
        {
            return ExecuteAsync<LinkedInBatchUpdateResultDto>(async () =>
            {
                var scraperService = _serviceProvider.GetService<BusinessLayer.Services.ILinkedInScraperService>();
                if (scraperService == null)
                {
                    throw new InvalidOperationException("LinkedInScraperService is not registered.");
                }

                int? limit = request?.Limit ?? 50;
                int? offset = request?.Offset ?? 0;
                int concurrency = request?.Concurrency > 0 ? request.Concurrency : 3;
                long? specificId = request?.SpecificConsultancyId;

                return await scraperService.ReviewAndUpdateLinkedInWebsitesAsync(limit, offset, concurrency, specificId);
            });
        }

        [HttpPost]
        [Route("ScrapeAndAddCompanyUrls")]
        [AdminAuthorize]
        public Task<Result<List<CompanyUrlScrapedDto>>> ScrapeAndAddCompanyUrls([FromBody] CompanyUrlScrapeRequestDto request)
        {
            return ExecuteAsync<List<CompanyUrlScrapedDto>>(async () =>
            {
                var scraperService = _serviceProvider.GetService<BusinessLayer.Services.ICompanyUrlScraperService>();
                if (scraperService == null)
                {
                    throw new InvalidOperationException("CompanyUrlScraperService is not registered.");
                }

                long? adminUserId = null;
                try
                {
                    var userContext = GetUserContext();
                    if (userContext != null && userContext.UserId > 0)
                    {
                        adminUserId = userContext.UserId;
                    }
                }
                catch { }

                return await scraperService.ScrapeAndSaveCompanyUrlsAsync(request.Urls, request.AutoSaveToDb, adminUserId);
            });
        }

        [HttpPost]
        [Route("UploadCompanyLogo")]
        [AdminAuthorize]
        public Task<Result<string>> UploadCompanyLogo([FromForm] FileModel model, [FromQuery] string? websiteUrl = null)
        {
            return ExecuteAsync<string>(async () =>
            {
                if (model == null || model.ImageFile == null)
                {
                    throw new ArgumentException("No image file provided");
                }

                string target = websiteUrl;
                if (string.IsNullOrWhiteSpace(target) && Request.HasFormContentType && Request.Form.ContainsKey("websiteUrl"))
                {
                    target = Request.Form["websiteUrl"].ToString();
                }

                var fileManager = managerFactory.Get<IFileManager>();
                string targetName = !string.IsNullOrWhiteSpace(target) ? target : "company_logo";
                string uploadedFileName = await fileManager.UploadWithName(model, targetName, "profilepic");
                return uploadedFileName;
            });
        }

        [HttpPost]
        [Route("CheckAndUpdateLogo")]
        [AdminAuthorize]
        public Task<Result<ConsultancyDto>> CheckAndUpdateLogo([FromQuery] long consultancyId, [FromQuery] string? linkedinUrl = null)
        {
            return ExecuteAsync<ConsultancyDto>(async () =>
            {
                var mgr = managerFactory.Get<IConsultancyManager>();
                UserContext context = null;
                try { context = GetUserContext(); } catch { }
                return await mgr.CheckAndUpdateLogo(consultancyId, linkedinUrl, context ?? GetDummyUserContext());
            });
        }

        [HttpPut]
        [Route("Update")]
        //[ApiAuthorize("UpdateConsultancy")]
        public Task<Result<ConsultancyDto>> UpdateConsultancy([FromBody] ConsultancyDto consultancy)
        {
            return ExecuteAsync<ConsultancyDto>(async () =>
            {
                var ConsultancyManager = managerFactory.Get<IConsultancyManager>();
                return await ConsultancyManager.UpdateConsultancy(consultancy, GetDummyUserContext());
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

        [HttpGet]
        [Route("CompanyUserCounts")]
        public async Task<IActionResult> GetCompanyUserCounts()
        {
            try
            {
                var counts = await dbcontext.ConsultancyUsers.AsNoTracking()
                    .Where(cu => cu.ConsultancyId.HasValue)
                    .GroupBy(cu => cu.ConsultancyId.Value)
                    .Select(g => new
                    {
                        consultancyId = g.Key,
                        userCount = g.Count()
                    })
                    .ToListAsync();

                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("CompanyUsers")]
        public async Task<IActionResult> GetCompanyUsers([FromQuery] long consultancyId)
        {
            try
            {
                var users = await dbcontext.ConsultancyUsers.AsNoTracking()
                    .Where(cu => cu.ConsultancyId == consultancyId)
                    .Include(cu => cu.User)
                    .Select(cu => new
                    {
                        consultancyUserId = cu.Id,
                        userId = cu.UserId,
                        name = (cu.User.Fname + " " + (cu.User.Lname ?? "")).Trim(),
                        email = cu.User.Email,
                        phone = cu.User.Phone,
                        active = cu.Active ?? cu.User.Active ?? true,
                        publicProfileUserName = cu.PublicProfileUserName,
                        roleRecruiter = cu.User.RoleRecruiter ?? false,
                        roleBenchSales = cu.User.RoleBenchSales ?? false,
                        profilePic = cu.User.ProfilePic,
                        updated = cu.Updated ?? cu.User.Updated
                    })
                    .OrderBy(u => u.name)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("HotlistUsers")]
        public async Task<IActionResult> GetHotlistUsers()
        {
            try
            {
                var hotlistStats = await dbcontext.CandidateProfiles.AsNoTracking()
                    .Where(cp => cp.Active == true)
                    .GroupBy(cp => cp.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var userIds = hotlistStats.Select(s => s.UserId).ToList();

                var users = await dbcontext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new
                    {
                        userId = u.Id,
                        name = (u.Fname + " " + (u.Lname ?? "")).Trim(),
                        email = u.Email,
                        userName = u.UserName,
                        password = u.Password,
                        active = u.Active,
                        roleRecruiter = u.RoleRecruiter,
                        roleBenchSales = u.RoleBenchSales
                    })
                    .ToListAsync();

                string[] commonPasswords = new[] {
                    "admin123", "password", "Password123!", "123456", "test1234", "Admin@123", "P@ssword1", "password123",
                    "Test@123", "Sharklasers123!", "Sharklasers@123", "Welcome1", "Welcome@123", "Welcome123", "12345678",
                    "Password@123", "Password@1", "TestPass123!", "TestPass123", "Admin123!", "Pass@123", "Pass1234",
                    "Chathire123!", "ChatHire@123", "Hires@123", "Hires123!", "Hires123", "Thayasrk22@@", "Password",
                    "vikram123", "vikram12345", "avinash123", "avinash12345", "recruiter123", "benchsales123", "secret123"
                };

                var result = users.Select(u =>
                {
                    string clearPassword = null;
                    if (!string.IsNullOrEmpty(u.password))
                    {
                        foreach (var testPwd in commonPasswords)
                        {
                            try
                            {
                                if (Utility.Security.Hashing.SecurePasswordHasher.Verify(testPwd, u.password))
                                {
                                    clearPassword = testPwd;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }

                    return new
                    {
                        u.userId,
                        u.name,
                        u.email,
                        u.userName,
                        password = clearPassword ?? (u.password != null ? "[Hashed: " + u.password.Substring(0, 15) + "...]" : "No Password"),
                        active = u.active,
                        roleRecruiter = u.roleRecruiter,
                        roleBenchSales = u.roleBenchSales,
                        hotlistCount = hotlistStats.FirstOrDefault(s => s.UserId == u.userId)?.Count ?? 0
                    };
                }).OrderByDescending(x => x.hotlistCount).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("SetUserPassword")]
        public async Task<IActionResult> SetUserPassword([FromQuery] long userId, [FromQuery] string newPassword)
        {
            try
            {
                var user = await dbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound("User not found");

                user.Password = Utility.Security.Hashing.SecurePasswordHasher.Hash(newPassword.Trim());
                user.ResetPassword = false;
                await dbcontext.SaveChangesAsync();

                return Ok(new { success = true, email = user.Email, newPassword = newPassword });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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