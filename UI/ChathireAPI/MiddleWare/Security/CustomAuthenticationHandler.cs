using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Middleware.Shared;
//using BusinessLayer.UserAccessControl;
using BusinessLayer.Common;
using BusinessEntityAndDTO.Common;
using Utility.Configuration;
using BusinessLayer.Manager;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
//using Utility.Configuration;
//using BusinessLayer.UserAccessControl;

namespace Middleware.Security
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILoginManager loginManager;
        private readonly IServiceProvider _serviceProvider;
        private ILogger<CustomAuthenticationHandler> log;
        protected IMemoryCache memoryCache;
        private static long? _timeOut = null;
        IConfiguration configuration;
        public CustomAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IServiceProvider serviceProvider)
            : base(options, logger, encoder, clock)
        {
            _serviceProvider = serviceProvider;
            loginManager = serviceProvider?.GetService<IManagerFactory>().Get<ILoginManager>();
            this.memoryCache = serviceProvider?.GetService<IMemoryCache>();
            log = logger.CreateLogger<CustomAuthenticationHandler>();
            this.configuration = serviceProvider?.GetService<IConfiguration>();
            //logger.CreateLogger<CustomAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if ((Request.Path.Equals("/api/Token") && Request.Method.Equals("POST")||(Request.Path.StartsWithSegments("/api/Common")))
                )
            {
                var claims = new List<Claim>();
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new System.Security.Principal.GenericPrincipal(identity, null);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                
                return AuthenticateResult.Success(ticket);
            }
            // validation comes in here
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("UnAuthorized");
            }

            var authHeader = Request.Headers["Authorization"].ToString();

            if (String.IsNullOrWhiteSpace(authHeader))
            {
                log.LogError("Invalid Token - Missing Auth Header");
                return AuthenticateResult.Fail("UnAuthorized");
            }

            if (!authHeader.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase))
            {
                log.LogError("Invalid Token - bearer not found");
                return AuthenticateResult.Fail("UnAuthorized");
            }

            var token = authHeader.Substring("bearer".Length).Trim();

            if (String.IsNullOrWhiteSpace(token))
            {
                log.LogError("Invalid Token - Empty token found");
                return AuthenticateResult.Fail("UnAuthorized");
            }
            try
            {
                return await ValidateToken(token);
            }
            catch (Exception ex)
            {
                log.LogError(String.Format("Invalid Token : {1} , Error Occured : {1}", token, ex.Message + ex.StackTrace));
                return AuthenticateResult.Fail("UnAuthorized");
            }

        }

        private async Task<AuthenticateResult> ValidateToken(string token)
        {
            var validatedToken = await loginManager.ValidateToken(token);

            if (validatedToken == null)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, validatedToken.context.UserId.ToString()),
                    new Claim("UserId", validatedToken.context.UserId.ToString()),
                    new Claim("UserTypeId",validatedToken.context.UserTypeId.ToString()),
                    new Claim("ConsultancyId",validatedToken.context.ConsultancyId != null ? validatedToken.context.ConsultancyId.ToString() : "-1")
                };
            
            var functions = await GetUserFunctions(validatedToken);
            if (functions != null && functions.Count() > 0)
            {
                foreach (var function in functions)
                {
                    claims.Add(new Claim("Permissions", function));
                }
            }
            if (validatedToken?.context != null && validatedToken.context.UserId > 0)
            {
                TrackUserActivity(validatedToken.context.UserId);
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new System.Security.Principal.GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }


        private string GetCacheKeyForFunctions(TokenModel model)
        {
            return String.Format("Functions_{0}", model.context.UserId);
        }
        protected async Task<List<String>> GetUserFunctions(TokenModel model)
        {
            List<String> functions;
            String _key = GetCacheKeyForFunctions(model);
            if (!memoryCache.TryGetValue(_key, out functions))
            {
                var funs = await loginManager.UserFunction(model.context);
                if (funs != null && funs.Count > 0)
                {
                    if (functions == null) functions = new List<string>();
                    functions.AddRange(funs);
                }

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
            return functions;
        }

        private void TrackUserActivity(long userId)
        {
            try
            {
                string cacheKey = $"LastActive_{userId}";
                if (memoryCache != null && !memoryCache.TryGetValue(cacheKey, out _))
                {
                    // Set 15-minute throttle in cache
                    memoryCache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromMinutes(15));

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (_serviceProvider != null)
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var mgrFactory = scope.ServiceProvider.GetService<IManagerFactory>();
                                if (mgrFactory != null)
                                {
                                    var loginMgr = mgrFactory.Get<ILoginManager>();
                                    await loginMgr.UpdateLastActive(userId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogWarning("Failed to update last active timestamp for user {UserId}: {Message}", userId, ex.Message);
                        }
                    });
                }
            }
            catch
            {
                // Non-blocking
            }
        }
    }
}
