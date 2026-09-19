using BusinessLayer.Common;
//using BusinessEntityAndDTO.AuditEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Utility.Configuration;
//using Utility.Security.Encryption;
using Microsoft.Extensions.DependencyInjection;
using BusinessEntityAndDTO.Common;
using AutoMapper;
using BusinessEntityAndDTO.Common;

namespace Middleware.Shared
{
    public class BaseCtrler<C> : Controller
    {
        protected IManagerFactory managerFactory;
        protected IMemoryCache memoryCache;
        protected ILogger<C> logger;
        protected IMapper mapper;
        private long? _userId;
        private int? _userTypeId;
        private string _sessionGuId;
        private int? _consultancyId;

        protected IConfiguration configuration;
        public BaseCtrler(IServiceProvider serviceProvider, ILogger<C> logger, IMapper mapper)
        {
            this.managerFactory = serviceProvider.GetService<IManagerFactory>();
            this.memoryCache = serviceProvider.GetService<IMemoryCache>();
            this.logger = logger;
            this.configuration = serviceProvider.GetService<IConfiguration>();
            this.mapper = mapper;
        }
        private Tuple<String, String> GetLogMessage(String Message)
        {
            String guid = Guid.NewGuid().ToString();
            return Tuple.Create(string.Format(" UserId : {0} | SessionId : {1} | TraceGuid : {2} | Message : {3}", TryGetUserId, TryGetSessionGuId, guid, Message), guid);
        }
        protected async Task<Result<E>> ExecuteAsync<E>(Func<Task<E>> function)
        {
            Result<E> value = new Result<E>();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errLst = new List<Error>();
                    foreach (var key in ModelState.Keys)
                    {
                        errLst.Add(new Error() { Message = String.Format("Invalid or Missing {0}", key), Id = "422" });
                    }
                    value.errors = errLst;
                    return value;
                }
                var result = await function();
                value.value = result;
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return value;
            }
            catch (ArgumentException ex)
            {
                var error = new Error() { Message = ex.Message, Id = "422" };
                if (!String.IsNullOrWhiteSpace(ex.ParamName))
                    error.Message = error + " : " + ex.ParamName;
                value.errors = new List<Error>() { error };
                Response.StatusCode = (int)System.Net.HttpStatusCode.UnprocessableEntity;
                return value;
            }
            catch (UnauthorizedAccessException ex)
            {
                var logMsg = GetLogMessage("UnauthorizedAccess");
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };;
                Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                return value;
            }
            catch (AccessViolationException ex)
            {
                var logMsg = GetLogMessage("Access Violation :" + ex.Message);
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return value;
            }
            catch (System.Exception ex)
            {
                String guid = Guid.NewGuid().ToString();
                var logMsg = GetLogMessage("Error Occured : " + ex.Message);
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return value;

            }
        }
        protected async Task<Result> ExecuteAsync(Func<Task> function)
        {
            Result value = new Result();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errLst = new List<Error>();
                    foreach (var key in ModelState.Keys)
                    {
                        errLst.Add(new Error() { Message = String.Format("Invalid or Missing {0}", key), Id = "422" });
                    }
                    value.errors = errLst;
                    return value;
                }
                await function();

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return value;
            }
            catch (ArgumentException ex)
            {
                value.errors = new List<Error>() { new Error() { Message = ex.Message, Id = "422" } };
                Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status422UnprocessableEntity;
                return value;
            }
            catch (UnauthorizedAccessException ex)
            {
                var logMsg = GetLogMessage("UnauthorizedAccess");
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };
                Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
                return value;
            }
            catch (AccessViolationException ex)
            {
                var logMsg = GetLogMessage("Access Violation :" + ex.Message);
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };

                Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest;
                return value;
            }
            catch (System.Exception ex)
            {
                String guid = Guid.NewGuid().ToString();
                var logMsg = GetLogMessage("Error Occured : " + ex.Message);
                logger.LogError(ex, logMsg.Item1);
                value.errors = new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support.", Id = logMsg.Item2 } };
                Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest;
                return value;

            }
        }
        //private IActionResult BadRequest(String TraceId)
        //{
        //    return BadRequest(new List<Error>() { new Error() { Message = "Error Occured , Please contact customer support", Id = TraceId } });
        //}
        protected long UserId
        {
            get
            {
                if (_userId == null)
                {

                    if (TryGetUserId == null)
                        throw new UnauthorizedAccessException("User Not found in the token,Unauthorized access");
                    else
                        return _userId.Value;
                }
                else
                {
                    return _userId.Value;
                }
            }
        }
        protected long? TryGetUserId
        {
            get
            {
                if (_userId == null)
                {
                    try
                    {
                        var claim = User.Claims.AsEnumerable().Single(c => c.Type == "UserId");
                        long userId;
                        if (long.TryParse(claim.Value, out userId))
                        {
                            _userId = userId;
                            return _userId.Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception) { }
                }
                return _userId;
            }
        }

        private int? TryGetConsultancyId
        {
            get
            {
                if (_consultancyId == null)
                {
                    try
                    {
                        var claim = User.Claims.AsEnumerable().Single(c => c.Type == "ConsultancyId");
                        int consultancyId;
                        if (int.TryParse(claim.Value, out consultancyId))
                        {
                            if (consultancyId == -1)
                            {
                                _consultancyId = null;
                            }
                            else
                            {
                                _consultancyId = consultancyId;
                            }
                        }
                        else
                        {
                            _consultancyId = null;
                        }
                    }
                    catch (Exception) { }
                }
                return _consultancyId;
            }
        }
        private int? TryGetUserTypeId
        {
            get
            {
                if (_userTypeId == null)
                {
                    try
                    {
                        var claim = User.Claims.AsEnumerable().Single(c => c.Type == "UserTypeId");
                        int userTypeId;
                        if (int.TryParse(claim.Value, out userTypeId))
                        {
                            _userTypeId = userTypeId;
                            return _userTypeId.Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception) { }
                }
                return _userTypeId;
            }
        }
        protected string TryGetSessionGuId
        {
            get
            {
                if (_sessionGuId == null)
                {
                    try
                    {

                        var claim = User.Claims.AsEnumerable().Single(c => c.Type == "SessionGuId");

                        if (!String.IsNullOrWhiteSpace(claim.Value))
                        {
                            _sessionGuId = claim.Value;
                            return _sessionGuId;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception) { }
                }
                return _sessionGuId;
            }
        }

       
        protected UserContext GetUserContext()
        {
            if(TryGetUserId == null)
            {
                throw new UnauthorizedAccessException();
            }
            return new UserContext() { UserId = TryGetUserId.Value
                                        ,UserTypeId = TryGetUserTypeId == null ? 0: TryGetUserTypeId.Value
                                        ,SessionGuid = TryGetSessionGuId
                                        ,ConsultancyId = TryGetConsultancyId
            };
        }

        protected bool CheckPermission(string permission)
        {
            if (!string.IsNullOrWhiteSpace(permission))
            {
                //Permissions
                var claims = User.Claims.AsEnumerable().Where(c => c.Type == "Permissions");
                var permissionMatch  = claims.FirstOrDefault(c=> c.Value == permission);
                if (permissionMatch != null) return true;
            }
            return false;
        }

        protected UserContext GetDummyUserContext()
        {
            return new UserContext()
            {
                UserId = -1
                                        ,
                UserTypeId = -1
                                        ,
                SessionGuid = Guid.NewGuid().ToString(),
            };
        }
    }
}
