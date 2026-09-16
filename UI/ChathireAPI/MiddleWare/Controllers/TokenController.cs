using AutoMapper;
using BusinessEntityAndDTO.Models;
using BusinessLayer.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Middleware.Security;
using Middleware.Shared;
using Utility.Configuration;

namespace MiddleWare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : BaseCtrler<TokenController>
    {
        public TokenController(IServiceProvider serviceProvider, ILogger<TokenController> logger, IMapper mapper) : base(serviceProvider, logger, mapper)
        {

        }

        [HttpPost]
        [AllowAnonymous]
        public Task<Result<LoginModel>> Token([FromBody] AuthRequestModel model)
        {
            return ExecuteAsync<LoginModel>(async () =>
            {
                var loginMgr = managerFactory.Get<ILoginManager>();

                var result = await loginMgr.GenerateToken(model.EMail, model.Pwd);

                var userFunctions = await loginMgr.UserFunction(result.Item3);

                String _key = String.Format("Functions_{0}", result.Item3.UserId);
                List<String> functions;
                memoryCache.Remove(_key);
                if (!memoryCache.TryGetValue(_key, out functions))
                {
                    var funs = userFunctions;

                    if (funs != null && funs.Count > 0)
                    {
                        if (functions == null) functions = new List<string>();
                        functions.AddRange(funs);
                    }
                    long? _timeOut = null;
                    MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                    if (_timeOut == null)
                    {
                        long timeOut;
                        if (long.TryParse(CustomConfigurationProvider.GetConfigurationSection(configuration, "Auth").GetValue("SessionTimeOut"), out timeOut))
                        {
                            _timeOut = timeOut;
                        }
                        else
                        {
                            _timeOut = Defaults.Timeout;
                        }
                    }
                    options.SetSlidingExpiration(TimeSpan.FromSeconds(_timeOut.Value));

                    memoryCache.Set(_key, functions, options);
                }

                var loginModel = new LoginModel()
                {
                    Token = result.Item2,
                    Functions = userFunctions,
                    UserTypeId = result.Item3.UserTypeId.Value,
                    UserId = result.Item3.UserId,
                    UserName = result.Item1,
                    UserTypeName = result.Item4,
                    ConsultancyId = result.Item3.ConsultancyId,
                    ConsultancyUserId = result.Item3.ConsultancyUserId
                };

                return loginModel;
            });
        }

        [HttpPost]
        [Route("Google")]
        [AllowAnonymous]
        public Task<Result<LoginModel>> GoogleToken([FromBody] GoogleAuthRequestModel model)
        {
            return ExecuteAsync<LoginModel>(async () =>
            {
                var loginMgr = managerFactory.Get<ILoginManager>();

                var result = await loginMgr.GenerateGoogleToken(model.IdToken);

                var userFunctions = await loginMgr.UserFunction(result.Item3);

                String _key = String.Format("Functions_{0}", result.Item3.UserId);
                List<String> functions;
                memoryCache.Remove(_key);
                if (!memoryCache.TryGetValue(_key, out functions))
                {
                    var funs = userFunctions;

                    if (funs != null && funs.Count > 0)
                    {
                        if (functions == null) functions = new List<string>();
                        functions.AddRange(funs);
                    }
                    long? _timeOut = null;
                    MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                    if (_timeOut == null)
                    {
                        long timeOut;
                        if (long.TryParse(CustomConfigurationProvider.GetConfigurationSection(configuration, "Auth").GetValue("SessionTimeOut"), out timeOut))
                        {
                            _timeOut = timeOut;
                        }
                        else
                        {
                            _timeOut = Defaults.Timeout;
                        }
                    }
                    options.SetSlidingExpiration(TimeSpan.FromSeconds(_timeOut.Value));

                    memoryCache.Set(_key, functions, options);
                }

                var loginModel = new LoginModel()
                {
                    Token = result.Item2,
                    Functions = userFunctions,
                    UserTypeId = result.Item3.UserTypeId.HasValue ? result.Item3.UserTypeId.Value : 1,
                    UserId = result.Item3.UserId,
                    UserName = result.Item1,
                    UserTypeName = result.Item4,
                    ConsultancyId = result.Item3.ConsultancyId,
                    ConsultancyUserId = result.Item3.ConsultancyUserId
                };

                return loginModel;
            });
        }

        [HttpGet]
        [Route("Validate")]
        [ApiAuthorize("Login")]
        public Task<Result<String>> Validate()
        {
            return ExecuteAsync<String>(async () =>
            {
                return "True";
            });
        }


        [HttpGet]
        [Route("Refresh")]
        [ApiAuthorize("Login")]
        public Task<Result<String>> RefreshToken()
        {
            return ExecuteAsync<String>(async () =>
            {
                var loginMgr = managerFactory.Get<ILoginManager>();

                return loginMgr.GetTokenFromContext(GetUserContext());
            });
        }


        [HttpGet]
        [Route("Functions")]
        [ApiAuthorize("Login")]
        public Task<Result<List<String>>> GetUserFunctions()
        {
            return ExecuteAsync<List<String>>(async () =>
            {
                var loginMgr = managerFactory.Get<ILoginManager>();

                return await loginMgr.UserFunction(GetUserContext());
            });
        }
    }
}
