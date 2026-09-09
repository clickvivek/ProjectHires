using AutoMapper;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Manager;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using Middleware.Security;
using Middleware.Shared;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Utility.Configuration;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : BaseCtrler<CommonController>
    {
        public CommonController(IServiceProvider serviceProvider, ILogger<CommonController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {

        }

        [HttpGet]
        //[ApiAuthorize("Login")]
        [Route("UserType")]
        public Task<Result<List<UserTypeDto>>> GetUserType()
        {
            return ExecuteAsync<List<UserTypeDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetUserType(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Category")]
        public Task<Result<List<CategoryDto>>> GetCategory()
        {
            return ExecuteAsync<List<CategoryDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetCategory(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("CandidateAvailability")]
        public Task<Result<List<CandidateAvailabilityDto>>> GetCandidateAvailability()
        {
            return ExecuteAsync<List<CandidateAvailabilityDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetCandidateAvailability(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Domain")]
        public Task<Result<List<DomainDto>>> GetDomain()
        {
            return ExecuteAsync<List<DomainDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetDomain(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("EmploymentType")]
        public Task<Result<List<EmploymentTypeDto>>> GetEmploymentType()
        {
            return ExecuteAsync<List<EmploymentTypeDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetEmploymentType(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("JobType")]
        public Task<Result<List<JobTypeDto>>> GetJobType()
        {
            return ExecuteAsync<List<JobTypeDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetJobType(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Status")]
        public Task<Result<List<StatusDto>>> GetStatus()
        {
            return ExecuteAsync<List<StatusDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetStatus(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("CandidateProfileMappingStatus")]
        public Task<Result<List<CandidateProfileMappingStatusDto>>> GetCandidateProfileMappingStatus()
        {
            return ExecuteAsync<List<CandidateProfileMappingStatusDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetCandidateProfileMappingStatus(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Visa")]
        public Task<Result<List<VisaDto>>> GetVisa()
        {
            return ExecuteAsync<List<VisaDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetVisa(GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("Skills")]
        public Task<Result<List<SkillDto>>> GetSkills(String? Skill)
        {
            return ExecuteAsync<List<SkillDto>>(async () =>
            {
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetSkills(Skill,GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("City")]
        public Task<Result<List<CityDto>>> GetCity(String? cityOrZip, int? state, bool isState)
        {
            return ExecuteAsync<List<CityDto>>(async () =>
            {
                //city = city.Trim();
                //if (city.Length < 3)
                //{
                //    throw new ArgumentException("searchString name should be 3 char in the search");
                //}
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetCity(cityOrZip, state, isState, GetDummyUserContext());
            });
        }

        [HttpGet]
        [Route("State")]
        public Task<Result<List<StateDto>>> GetState(String? searchString)
        {
            return ExecuteAsync<List<StateDto>>(async () =>
            {
                //searchString = searchString.Trim();
                //if (searchString.Length < 2)
                //{
                //    throw new ArgumentException("searchString name should be minimum 2 char in the search");
                //}
                var mgr = managerFactory.Get<ICommonManager>();

                return await mgr.GetState(searchString, GetDummyUserContext());
            });
        }


        [HttpGet]
        [Route("Email")]
        public void SendEmail(String? emailAddress, object? dynamicData)
        {
            var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
            //var client = new SendGridClient(authValueProvider.GetValue("APIKey"));
            //var from = new EmailAddress(authValueProvider.GetValue("SenderEmail"), "Hires Co");
            //var to = new EmailAddress(emailAddress, "");;
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //var response = await client.SendEmailAsync(msg);

            var mgr = managerFactory.Get<ICommonManager>();

            mgr.SendEmail(emailAddress,dynamicData, "",authValueProvider, GetDummyUserContext());
        }

        [HttpGet]
        [Route("EmailAttachment")]
        public Task<Result<bool>> EmailAttachment(String? emailAddress)
        {
            
            return ExecuteAsync<bool>(async () =>
            {
                var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
                var client = new SendGridClient(authValueProvider.GetValue("APIKey"));
                var from = new EmailAddress(authValueProvider.GetValue("SenderEmail"), "Hires Co");
                var to = new EmailAddress(emailAddress, ""); ;
                var msg = MailHelper.CreateSingleEmail(from, to, "Test", "Test", "");
                var fileManager = managerFactory.Get<IFileManager>();

                var blob = fileManager.GetBlobClient("20231103060358_9520a403-a501-45c3-a553-3a93ab2bbd9d.pdf", "resumes");
                if (!await blob.ExistsAsync()) return false;

                await msg.AddAttachmentAsync("1.pdf", blob.OpenRead());
                var response = await client.SendEmailAsync(msg);

                return true;

            });
        }
        //    [HttpGet]
        //[Route("Email1")]
        //public Task<Result<bool>> SendEmail1(String? emailAddress)
        //{
        //    var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");
        //    var client = new SendGridClient(authValueProvider.GetValue("APIKey"));
        //    var from = new EmailAddress(authValueProvider.GetValue("SenderEmail"), "Hires Co");
        //    var to = new EmailAddress(emailAddress, ""); ;
        //    var msg = MailHelper.CreateSingleEmail(from, to, "Test", "Test", "");
        //    //byte[] byteData = Encoding.ASCII.GetBytes(File.ReadAllText(filePath));
        ////    msg.Attachments = new List<SendGrid.Helpers.Mail.Attachment>
        ////{
        ////    new SendGrid.Helpers.Mail.Attachment
        ////    {
        ////        Content = Convert.ToBase64String(byteData),
        ////        Filename = "FILE_NAME.txt",
        ////        Type = "txt/plain",
        ////        Disposition = "attachment"
        ////    }
        ////};
        //    //msg.AddAttachment(@"C:\test\test.txt");
        //    //var A = new FileController(IServiceProvider, logger ).GetDownload;
        //    //var controller = DependencyResolver.Current.GetService<FileController>();
        //    //controller.ControllerContext = new System.Web.Mvc.ControllerContext(this.Request.RequestContext, controller);
        //    var fileManager = managerFactory.Get<IFileManager>();

        //    var blob = fileManager.GetBlobClient("20231103060358_9520a403-a501-45c3-a553-3a93ab2bbd9d.pdf", "resumes");
        //    //string filetype = "pdf";
        //    //var a = File(imgStream, $"image/{filetype}", $"blobfile.{filetype}");
        //    // Validate the file exists in Azure
        //    if (!await blob.ExistsAsync()) return false;

        //    await msg.AddAttachmentAsync("20231103060358_9520a403-a501-45c3-a553-3a93ab2bbd9d.pdf", imgStream.OpenRead());
        //    var response = await client.SendEmailAsync(msg);

        //    return Json(true);

        //    //return response;
        //    //var mgr = managerFactory.Get<ICommonManager>();

        //    //mgr.SendEmail(emailAddress, dynamicData, "", authValueProvider, GetDummyUserContext());
        //}

        //20231103060358_9520a403-a501-45c3-a553-3a93ab2bbd9d.pdf
        //pdf
        //resumes
    }
}
