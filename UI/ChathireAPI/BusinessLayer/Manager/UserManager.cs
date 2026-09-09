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
    }
    public class UserManager : BaseManager<UserManager>, IUserManager
    {
        public UserManager(IServiceProvider provider, ILogger<UserManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
        }

        public async Task<List<UserDto>> GetUserByUserName(string email, UserContext userContext)
        {
            return await ExecuteAsync<List<UserDto>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                return mapper.Map<List<UserDto>>(await repo.GetUserByUserName(email));
            }, "GetUserByUserName", userContext);

        }

        public async Task<List<UserDtoForReturn>> GetUserByPublicProfileId(string publicProfileId, UserContext userContext)
        {
            return await ExecuteAsync<List<UserDtoForReturn>>(async () =>
            {
                var repo = repositoryFactory.Get<IUserRepository>();
                return mapper.Map<List<UserDtoForReturn>>(await repo.GetUserByPublicProfileId(publicProfileId));
            }, "GetUserByUserName", userContext);

        }

        public async Task<UserDto> AddUser(UserDtoForInsert user, UserContext userContext)
        {
            var result = await ExecuteAsync<User>(async () =>
            {
                var date = DateTime.UtcNow;
                var _user = mapper.Map<User>(user);

                var repo = repositoryFactory.Get<IUserRepository>();
                _user.Updated = date;
                _user.UpdatedBy = userContext.UserId;
                _user.UserName = user.Email;
                if (user.Fname == null || user.Fname == "")
                    _user.Fname = "";

                return await repo.Post(_user, true);
            }, "AddUser", userContext);

            return mapper.Map<UserDto>(result);
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
                    _user.Lname = user.UserDtoForUpdate.Lname;

                if (user.UserDtoForUpdate.Fname != null)
                    _user.Fname = user.UserDtoForUpdate.Fname;

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

                if (user.UserDtoForUpdate.Gender != null)
                    _user.Gender = user.UserDtoForUpdate.Gender;

                if (user.UserDtoForUpdate.NoOfViews != null)
                    _user.NoOfViews = user.UserDtoForUpdate.NoOfViews;

                if (user.UserDtoForUpdate.NoOfPosting != null)
                    _user.NoOfPosting = user.UserDtoForUpdate.NoOfPosting;

                if (user.UserDtoForUpdate.ProfilePic != null)
                    _user.ProfilePic = user.UserDtoForUpdate.ProfilePic;

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

