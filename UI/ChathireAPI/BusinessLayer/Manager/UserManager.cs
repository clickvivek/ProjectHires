using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utility.Configuration;
using BusinessLayer.Services;

namespace BusinessLayer.Manager
{
    public interface IUserManager
    {

        Task<List<UserDto>> GetUserByUserName(string email, UserContext userContext);
        Task<List<UserDtoForReturn>> GetUserByPublicProfileId(string publicProfileId, UserContext userContext);
        Task<UserDto> AddUser(UserDtoForInsert user, UserContext userContext);

        Task<UserDto> AddUserWithConsultacy(UserDtoForInsert? user, long? ConsultacyId, UserContext userContext);
        Task<UserDto> UpdateUser(UserDetailsDtoForUpdate user, UserContext userContext);
        Task<bool> UpdatePassword(string email, string? newpassword, string EmailTemplateId, IConfigurationValueProvider? authValueProvider, UserContext userContext);
        Task<bool> UpdateProfilePic(string email, string? filename, UserContext userContext);
        Task<bool> VerifyOtp(string email, string otp, UserContext userContext);
        Task<bool> ResendOtp(string email, UserContext userContext);
        Task<bool> SendForgotPasswordOtp(string email, UserContext userContext);
        Task<bool> ResetPasswordWithOtp(string email, string otp, string newPassword, UserContext userContext);
    }
    public class UserManager : BaseManager<UserManager>, IUserManager
    {
        public UserManager(IServiceProvider provider, ILogger<UserManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        private static string FormatTitleCase(string? str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;
            str = str.Trim();
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public async Task<List<UserDto>> GetUserByUserName(string email, UserContext userContext)
        {
            return await ExecuteAsync<List<UserDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                var list = mapper.Map<List<UserDto>>(await repo.GetUserByUserName(email));
                if (list != null)
                {
                    foreach (var u in list)
                    {
                        u.Fname = FormatTitleCase(u.Fname);
                        u.Lname = FormatTitleCase(u.Lname);
                    }
                }
                return list;
            }, "GetUserByUserName", userContext);

        }

        public async Task<List<UserDtoForReturn>> GetUserByPublicProfileId(string publicProfileId, UserContext userContext)
        {
            return await ExecuteAsync<List<UserDtoForReturn>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                var list = mapper.Map<List<UserDtoForReturn>>(await repo.GetUserByPublicProfileId(publicProfileId));
                if (list != null)
                {
                    foreach (var u in list)
                    {
                        u.Fname = FormatTitleCase(u.Fname);
                        u.Lname = FormatTitleCase(u.Lname);
                    }
                }
                return list;
            }, "GetUserByUserName", userContext);

        }

        public async Task<UserDto> AddUser(UserDtoForInsert user, UserContext userContext)
        {
            var result = await ExecuteAsync<User>(async () =>
            {
                var date = DateTime.UtcNow;
                var repo = repositoryFactory.Get<IUserRepository>();
                var existingUsers = await repo.GetUserByUserName(user.Email);
                var existingUser = existingUsers?.FirstOrDefault();

                var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

                User targetUser;
                if (existingUser != null)
                {
                    if (existingUser.EmailVerified == true)
                    {
                        throw new ArgumentException("User Already exists");
                    }
                    existingUser.Password = user.Password;
                    existingUser.Otpemail = otpCode;
                    existingUser.OtpemailDate = date.AddMinutes(10);
                    existingUser.Updated = date;
                    existingUser.UpdatedBy = userContext.UserId;
                    await repo.Put(existingUser.Id, existingUser, true);
                    targetUser = existingUser;
                }
                else
                {
                    var _user = mapper.Map<User>(user);
                    _user.Updated = date;
                    _user.UpdatedBy = userContext.UserId;
                    _user.UserName = user.Email;
                    _user.Active = false;
                    _user.EmailVerified = false;
                    _user.Otpemail = otpCode;
                    _user.OtpemailDate = date.AddMinutes(10);
                    _user.Fname = FormatTitleCase(user.Fname);
                    _user.Lname = FormatTitleCase(user.Lname);

                    targetUser = await repo.Post(_user, true);
                }

                var emailService = serviceProvider?.GetService<IResendEmailService>();
                if (emailService != null)
                {
                    await emailService.SendOtpEmailAsync(targetUser.Email, otpCode);
                }

                return targetUser;
            }, "AddUser", userContext);

            return mapper.Map<UserDto>(result);
        }

        public async Task<bool> VerifyOtp(string email, string otp, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                var users = await repo.GetUserByUserName(email);
                var user = users?.FirstOrDefault();

                if (user == null)
                {
                    throw new ArgumentException("User account not found");
                }

                if (user.EmailVerified == true)
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(user.Otpemail) || user.Otpemail.Trim() != otp.Trim())
                {
                    throw new ArgumentException("Invalid verification code. Please check and try again.");
                }

                if (user.OtpemailDate.HasValue && user.OtpemailDate.Value < DateTime.UtcNow)
                {
                    throw new ArgumentException("Verification code has expired. Please click 'Resend' to get a new code.");
                }

                user.EmailVerified = true;
                user.Active = true;
                user.Otpemail = null;
                user.OtpemailDate = null;
                user.Updated = DateTime.UtcNow;

                await repo.Put(user.Id, user, true);
                return true;
            }, "VerifyOtp", userContext);
        }

        public async Task<bool> ResendOtp(string email, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                var users = await repo.GetUserByUserName(email);
                var user = users?.FirstOrDefault();

                if (user == null)
                {
                    throw new ArgumentException("User account not found");
                }

                if (user.EmailVerified == true)
                {
                    throw new ArgumentException("Email is already verified");
                }

                var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                user.Otpemail = otpCode;
                user.OtpemailDate = DateTime.UtcNow.AddMinutes(10);
                user.Updated = DateTime.UtcNow;

                await repo.Put(user.Id, user, true);

                var emailService = serviceProvider?.GetService<IResendEmailService>();
                if (emailService != null)
                {
                    await emailService.SendOtpEmailAsync(user.Email, otpCode);
                }

                return true;
            }, "ResendOtp", userContext);
        }

        public async Task<bool> SendForgotPasswordOtp(string email, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Please enter a valid email address.");
                }

                var repo = repositoryFactory.Get<IUserRepository>();
                var users = await repo.GetUserByUserName(email.Trim());
                var user = users?.FirstOrDefault();

                if (user == null)
                {
                    throw new ArgumentException("No account found with this email address.");
                }

                var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                user.OtppwdReset = otpCode;
                user.OtppwdDateTime = DateTime.UtcNow.AddMinutes(10);
                user.Updated = DateTime.UtcNow;

                await repo.Put(user.Id, user, true);

                var emailService = serviceProvider?.GetService<IResendEmailService>();
                if (emailService != null)
                {
                    await emailService.SendPasswordResetOtpEmailAsync(user.Email, otpCode);
                }

                return true;
            }, "SendForgotPasswordOtp", userContext);
        }

        public async Task<bool> ResetPasswordWithOtp(string email, string otp, string newPassword, UserContext userContext)
        {
            return await ExecuteAsync<bool>(async () =>
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email is required.");
                }

                if (string.IsNullOrWhiteSpace(otp))
                {
                    throw new ArgumentException("Please enter the 6-digit verification code.");
                }

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    throw new ArgumentException("New password must be at least 6 characters long.");
                }

                var repo = repositoryFactory.Get<IUserRepository>();
                var users = await repo.GetUserByUserName(email.Trim());
                var user = users?.FirstOrDefault();

                if (user == null)
                {
                    throw new ArgumentException("No account found with this email address.");
                }

                if (string.IsNullOrWhiteSpace(user.OtppwdReset) || user.OtppwdReset.Trim() != otp.Trim())
                {
                    throw new ArgumentException("Invalid verification code. Please check your code and try again.");
                }

                if (user.OtppwdDateTime.HasValue && user.OtppwdDateTime.Value < DateTime.UtcNow)
                {
                    throw new ArgumentException("Verification code has expired. Please request a new code.");
                }

                user.Password = newPassword;
                user.OtppwdReset = null;
                user.OtppwdDateTime = null;
                user.ResetPassword = false;
                user.EmailVerified = true;
                user.Active = true;
                user.Updated = DateTime.UtcNow;

                await repo.Put(user.Id, user, true);

                return true;
            }, "ResetPasswordWithOtp", userContext);
        }

        public async Task<UserDto> AddUserWithConsultacy(UserDtoForInsert? user, long? consultacyId, UserContext userContext)
        {
            var result = await ExecuteAsync<User>(async () =>
            {
                var date = DateTime.UtcNow;
                var _user = mapper.Map<User>(user);
                var repo = repositoryFactory.Get<IUserRepository>();
                _user.Updated = date;
                _user.UpdatedBy = userContext.UserId;
                _user.Fname = FormatTitleCase(user?.Fname);
                _user.Lname = FormatTitleCase(user?.Lname);
                return await repo.Post(_user, true);
            }, "AddUserWithConsultacy", userContext);

            UserDto userDto = mapper.Map<UserDto>(result);

            var resultq = await ExecuteAsync<ConsultancyUser>(async () =>
            {
                var date = DateTime.UtcNow;
                var _conUser = mapper.Map<ConsultancyUser>(user);

                var repo = repositoryFactory.Get<IConsultancyUserRepository>();

                _conUser.Updated = date;
                _conUser.UpdatedBy = userContext.UserId;
                _conUser.UserId = userDto.Id;
                _conUser.ConsultancyId = consultacyId;
                return await repo.Post(_conUser, true);
            }, "AddUserWithConsultacy", userContext);

            return userDto;
        }

        public async Task<bool> UpdateProfilePic(string email, string? filename, UserContext userContext)
        {
            var userDto = await GetUserByUserName(email, userContext);

            if (userDto != null)
            {
                
                userDto[0].ProfilePic = filename;
                
                UserDetailsDtoForUpdate userDtlDto = new UserDetailsDtoForUpdate();
                userDtlDto.ConsultancyID = 0;
                userDtlDto.UserDtoForUpdate = userDto[0];
                var newUserDto = await UpdateUser(userDtlDto, userContext);

                if (newUserDto != null)
                {
                    return true;
                }
            }
            return false;

        }

            public async Task<bool> UpdatePassword(string email, string? newpassword, string EmailTemplateId, IConfigurationValueProvider? authValueProvider, UserContext userContext)
        {
            var userDto = await GetUserByUserName(email, userContext);
            bool isReset = false;

            if (newpassword == null)
            {
                newpassword = Password.Generate(8, 2);
                isReset = true;
                userDto[0].ResetPassword = true;
            }

            if (userDto != null)
            {
                var dynamicEmailData = new
                {
                    pass = newpassword,
                };
                userDto[0].Password= newpassword;
                if(isReset == false && userDto[0].ResetPassword == true)
                    userDto[0].ResetPassword = false;

                UserDetailsDtoForUpdate userDtlDto = new UserDetailsDtoForUpdate();
                userDtlDto.ConsultancyID = 0;
                userDtlDto.UserDtoForUpdate = userDto[0];
                var newUserDto = await UpdateUser(userDtlDto, userContext);

                if(newUserDto != null)
                {
                    string emailMessage;
                    
                    if(isReset)
                        emailMessage = "New password: " + newpassword;
                    else
                        emailMessage = "Account password has been changed successfully!";

                    var mgr = managerFactory.Get<ICommonManager>();
                    mgr.SendEmail(newUserDto.Email, dynamicEmailData, EmailTemplateId, authValueProvider, userContext);
                    return true;
                }
            }
            return false;
        }
        public async Task<UserDto> UpdateUser(UserDetailsDtoForUpdate user, UserContext userContext)
        {

            var result = await ExecuteAsync<User>(async () =>
            {
                var date = DateTime.UtcNow;
                //var _user = mapper.Map<User>(user.UserDtoForUpdate);

                var repo = repositoryFactory.Get<IUserRepository>();

                var _user = mapper.Map<User>(repo.GetUserById(user.UserDtoForUpdate.Id)[0]);
                if (user.UserDtoForUpdate.UserName != null)
                    _user.UserName = user.UserDtoForUpdate.UserName;

                if (user.UserDtoForUpdate.Lname != null)
                    _user.Lname = FormatTitleCase(user.UserDtoForUpdate.Lname);

                if (user.UserDtoForUpdate.Fname != null)
                    _user.Fname = FormatTitleCase(user.UserDtoForUpdate.Fname);

                if (user.UserDtoForUpdate.Linkedin != null)
                    _user.Linkedin = user.UserDtoForUpdate.Linkedin;

                if (user.UserDtoForUpdate.Phone != null)
                    _user.Phone = user.UserDtoForUpdate.Phone;

                if (user.UserDtoForUpdate.CityId > 0)
                    _user.CityId = user.UserDtoForUpdate.CityId;

                if (user.UserDtoForUpdate.AlternateEmail != null)
                    _user.AlternateEmail = user.UserDtoForUpdate.AlternateEmail;

                if (user.UserDtoForUpdate.Email != null)
                    _user.Email = user.UserDtoForUpdate.Email;

                if (user.UserDtoForUpdate.UserTypeId >0)
                    _user.UserTypeId = user.UserDtoForUpdate.UserTypeId;

                if (user.UserDtoForUpdate.UserName != null)
                    _user.UserName = user.UserDtoForUpdate.UserName;

                if (user.UserDtoForUpdate.Password != null)
                    _user.Password = user.UserDtoForUpdate.Password;

                if (user.UserDtoForUpdate.ResetPassword != null)
                    _user.ResetPassword = user.UserDtoForUpdate.ResetPassword;

                if (user.UserDtoForUpdate.Address != null)
                    _user.Address = user.UserDtoForUpdate.Address;

                if (user.UserDtoForUpdate.Hiringforcountry != null)
                    _user.Hiringforcountry = user.UserDtoForUpdate.Hiringforcountry;

                if (user.UserDtoForUpdate.Location != null)
                    _user.Location = user.UserDtoForUpdate.Location;

                if (user.UserDtoForUpdate.Gender != null)
                    _user.Gender = user.UserDtoForUpdate.Gender;

                if (user.UserDtoForUpdate.NoOfViews != null)
                    _user.NoOfViews = user.UserDtoForUpdate.NoOfViews;

                if (user.UserDtoForUpdate.NoOfPosting != null)
                    _user.NoOfPosting = user.UserDtoForUpdate.NoOfPosting;

                if (user.UserDtoForUpdate.ProfilePic != null)
                    _user.ProfilePic = user.UserDtoForUpdate.ProfilePic;

                if (user.UserDtoForUpdate.RoleRecruiter != null)
                    _user.RoleRecruiter = user.UserDtoForUpdate.RoleRecruiter;

                if (user.UserDtoForUpdate.RoleBenchSales != null)
                    _user.RoleBenchSales = user.UserDtoForUpdate.RoleBenchSales;

                _user.Updated = date;
                _user.UpdatedBy = userContext.UserId;

                await repo.Put(_user.Id, _user, true);
                return _user;
            }, "UpdateUser", userContext);

            var repo1 = repositoryFactory.Get<IConsultancyUserRepository>();

            if (user.ConsultancyID > 0)
            {
                var consultancyUser = repo1.CheckConsultancyUser(user.ConsultancyID, result.Id);
                if (consultancyUser == null)
                {
                    var resultq = await ExecuteAsync<ConsultancyUser>(async () =>
                    {
                        var date = DateTime.UtcNow;

                        ConsultancyUserInsertDto conUser = new ConsultancyUserInsertDto();
                        conUser.ConsultancyId = user.ConsultancyID;
                        conUser.UserId = result.Id;
                        conUser.PublicProfileUserName = user.PublicProfileUserName;

                        var _conUser = mapper.Map<ConsultancyUser>(conUser);

                        _conUser.Updated = date;
                        _conUser.UpdatedBy = userContext.UserId;
                        _conUser.Active = true;
                        return await repo1.Post(_conUser, true);
                    }, "UpdateUserWithConsultacy", userContext);
                }
                else //if (user.ConsultancyID != consultancyUser[0].ConsultancyId)
                {
                    var resultq = await ExecuteAsync<ConsultancyUser>(async () =>
                    {
                        var date = DateTime.UtcNow;

                        ConsultancyUserDto conUser = new ConsultancyUserDto();
                        conUser.ConsultancyId = user.ConsultancyID;
                        conUser.UserId = result.Id;
                        conUser.Id = consultancyUser[0].Id;
                        conUser.PublicProfileUserName = user.PublicProfileUserName;
                        var _conUser = mapper.Map<ConsultancyUser>(conUser);

                        _conUser.Updated = date;
                        _conUser.UpdatedBy = userContext.UserId;
                        _conUser.Active = true;
                        await repo1.Put(_conUser.Id, _conUser, true);
                        return _conUser;
                    }, "UpdateUserWithConsultacy", userContext);
                }
            }
            var rtn = mapper.Map<UserDto>(result);
            var consulUser = repo1.CheckConsultancyUser(user.ConsultancyID, result.Id);
            if (consulUser != null)
            {
                rtn.ConsultancyUserId = consulUser[0].Id;
            }
            return rtn;


        }

        //public async Task<bool> UploadImage(FileModel model, string email, int documentId, UserContext userContext)
        //{
        //    var userDto = await GetUserByUserName(email, userContext);

        //}

        public static class Password
        {
            private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

            public static string Generate(int length, int numberOfNonAlphanumericCharacters)
            {
                if (length < 1 || length > 128)
                {
                    throw new ArgumentException(nameof(length));
                }

                if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
                {
                    throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
                }

                using (var rng = RandomNumberGenerator.Create())
                {
                    var byteBuffer = new byte[length];

                    rng.GetBytes(byteBuffer);

                    var count = 0;
                    var characterBuffer = new char[length];

                    for (var iter = 0; iter < length; iter++)
                    {
                        var i = byteBuffer[iter] % 87;

                        if (i < 10)
                        {
                            characterBuffer[iter] = (char)('0' + i);
                        }
                        else if (i < 36)
                        {
                            characterBuffer[iter] = (char)('A' + i - 10);
                        }
                        else if (i < 62)
                        {
                            characterBuffer[iter] = (char)('a' + i - 36);
                        }
                        else
                        {
                            characterBuffer[iter] = Punctuations[i - 62];
                            count++;
                        }
                    }

                    if (count >= numberOfNonAlphanumericCharacters)
                    {
                        return new string(characterBuffer);
                    }

                    int j;
                    var rand = new Random();

                    for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                    {
                        int k;
                        do
                        {
                            k = rand.Next(0, length);
                        }
                        while (!char.IsLetterOrDigit(characterBuffer[k]));

                        characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
                    }

                    return new string(characterBuffer);
                }
            }
        }




    }
}

