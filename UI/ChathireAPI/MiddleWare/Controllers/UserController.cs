using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Mvc;
using Middleware.Shared;
using Microsoft.AspNetCore.DataProtection;
using static BusinessLayer.Manager.UserManager;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail.Model;
using SendGrid.Helpers.Mail;
using Utility.Configuration;
using Middleware.Security;
using Newtonsoft.Json.Linq;
using BusinessEntityAndDTO.Models;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseCtrler<UserController>
    {
        public UserController(IServiceProvider serviceProvider, ILogger<UserController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {
        }


        [HttpPost]
        [Route("Add")]
        //[ApiAuthorize("AddUser")]
        public Task<Result<UserDto>> AddUser(UserDtoForInsert User)
        {
            return ExecuteAsync<UserDto>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                var usrDtls = await mgr.GetUserByUserName(User.Email, GetDummyUserContext());
                var existingUser = usrDtls?.FirstOrDefault();

                if (existingUser != null && existingUser.EmailVerified == true)
                {
                    throw new ArgumentException("User Already exists");
                }

                return await mgr.AddUser(User, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("VerifyOtp")]
        public Task<Result<bool>> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.VerifyOtp(model.Email, model.Otp, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ResendOtp")]
        public Task<Result<bool>> ResendOtp([FromBody] ResendOtpModel model)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.ResendOtp(model.Email, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public Task<Result<bool>> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.SendForgotPasswordOtp(model.Email, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ResetPasswordWithOtp")]
        public Task<Result<bool>> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpModel model)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.ResetPasswordWithOtp(model.Email, model.Otp, model.NewPassword, GetDummyUserContext());
            });
        }

        [HttpPost]
        [Route("ResendForgotPasswordOtp")]
        public Task<Result<bool>> ResendForgotPasswordOtp([FromBody] ForgotPasswordModel model)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.SendForgotPasswordOtp(model.Email, GetDummyUserContext());
            });
        }

        [HttpGet]
        //[ApiAuthorize("UpdateUser")]
        [Route("GetUserByUserName")]
        public Task<Result<List<UserDto>>> GetUserByUserName(string email)
        {
            return ExecuteAsync<List<UserDto>>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                return await mgr.GetUserByUserName(email, GetDummyUserContext());
            });
        }

        [HttpGet]
        //[ApiAuthorize("UpdateUser")]
        [Route("GetUserByPublicProfileId")]
        public Task<Result<List<UserDtoForReturn>>> GetUserByPublicProfileId(string publicProfileId)
        {
            return ExecuteAsync<List<UserDtoForReturn>>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                return await mgr.GetUserByPublicProfileId(publicProfileId, GetDummyUserContext());
            });
        }

        //[HttpGet]
        //[Route("UserNameValidation")]
        private Task<Result<bool>> UserNameValidation(string email)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                var usrDtls = await mgr.GetUserByUserName(email, GetDummyUserContext());

                if (usrDtls.Count > 0)
                {
                    return true;
                }
                return false;
            });
        }

        [HttpGet]
        //[ApiAuthorize("UpdateUser")]
        [Route("PublicProfileValidation")]
        public Task<Result<bool>> PublicProfileValidation(string publicProfileId)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                var usrDtls = await mgr.GetUserByPublicProfileId(publicProfileId, GetDummyUserContext());

                if (usrDtls.Count > 0)
                {
                    return true;
                }
                return false;
            });
        }

        [HttpPut]
        [Route("Update")]
        //[ApiAuthorize("UpdateUser")]
        public Task<Result<UserDto>> UpdateUser(UserDetailsDtoForUpdate user)
        {
            return ExecuteAsync<UserDto>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                return await mgr.UpdateUser(user, GetDummyUserContext());
            });
        }

        [HttpPut]
        [Route("UpdatePassword")]
        //[ApiAuthorize("UpdateUser")]
        public Task<Result<bool>> UpdatePassword(string email, string newpassword)
        {
            var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");


            var EmailTemplateId = CustomConfigurationProvider.GetConfigurationSection(configuration, "EmailTemplates");

            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.UpdatePassword(email, newpassword, EmailTemplateId.GetValue("ResetEmailTemplateId"), authValueProvider, GetUserContext());
            });
        }


        [HttpPut]
        [Route("ResetPassword")]
        //[ApiAuthorize("UpdateUser")]
        public Task<Result<bool>> ResetPassword(string email)
        {
            var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");

            //string TemplateId = CustomConfigurationProvider.GetConfigurationSection(configuration, "ResetEmailTemplateId");

            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();
                return await mgr.UpdatePassword(email, null, "",authValueProvider, GetUserContext());
            });

        }

        [HttpPut]
        [Route("UploadProfilePic")]
        public Task<Result<bool>> UploadProfilePic([FromForm] FileModel model, string email)
        {
            return ExecuteAsync<bool>(async () =>
            {
                var mgr = managerFactory.Get<IUserManager>();

                return await mgr.UpdateProfilePic(email, await fileUpload(model), GetDummyUserContext());
            });
        }

        private async Task<string> fileUpload(FileModel model)
        {
            string fileName = "";
            if (model.ImageFile != null)
            {
                var fileManager = managerFactory.Get<IFileManager>();

                fileName = await fileManager.Upload(model, "profilepic");
            }

            return fileName;
        }

        //[HttpPut]
        //[Route("ResetPassword")]
        ////[ApiAuthorize("UpdateUser")]
        //public async Task<bool> ResetPassword(long Id, string email)
        //{
        //    string password = Password.Generate(8, 2);

        //    UserDto user = new UserDto();
        //    user.Email = email;
        //    user.Password = password;
        //    user.ResetPassword = true;
        //    user.Id= Id;
        //    user.Fname= email;

        //    var result = ExecuteAsync<UserDto>(async () =>
        //    {
        //        var mgr = managerFactory.Get<IUserManager>();

        //        return await mgr.UpdateUser(user, GetDummyUserContext());
        //    });
        //    if(result.IsCompleted)
        //    {    
        //       var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Email");    
        //        var mgr = managerFactory.Get<ICommonManager>();
        //        mgr.SendEmail(email, "Password Reset", "New Password: "+ password, null, authValueProvider, GetDummyUserContext());
        //        return true;
        //    }
        //    return false;


        //}

        //[HttpPost]
        //[Route("Add")]
        ////[ApiAuthorize("AddUser")]
        //public Task<Result<UserDto>> AddUserMini(UserDtoForInsert User)
        //{
        //    return ExecuteAsync<UserDto>(async () =>
        //    {
        //        var UserManager = managerFactory.Get<IUserManager>();

        //        return await UserManager.AddUser(User, GetUserContext());
        //    });
        //}

        [HttpPost]
        [Route("AddUserWithConsultancy")]
        //[ApiAuthorize("AddUser")]
        public Task<Result<UserDto>> AddUserWithConsultancy(UserDetailsDtoForInsert user)
        {
            return ExecuteAsync<UserDto>(async () =>
            {
                var UserManager = managerFactory.Get<IUserManager>();

                return await UserManager.AddUserWithConsultacy(user.UserDtoForInsert, user.ConsultancyID, GetUserContext());
            });
        }

        [HttpGet]
        [Route("DauStats")]
        public Task<Result<DauDashboardDto>> GetDauStats([FromQuery] string timeframe = "day", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            return ExecuteAsync<DauDashboardDto>(async () =>
            {
                var userManager = managerFactory.Get<IUserManager>();
                return await userManager.GetDauDashboard(timeframe, startDate, endDate, GetDummyUserContext());
            });
        }
    }
}
