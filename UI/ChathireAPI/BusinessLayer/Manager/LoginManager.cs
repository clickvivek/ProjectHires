using AutoMapper;
using BusinessEntityAndDTO.Common;
using BusinessLayer.Common;
using DataAccessLayer.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Configuration;
using Utility.Security.Hashing;

namespace BusinessLayer.Manager
{
    public interface ILoginManager
    {
        Task<Tuple<String, String, UserContext, String>> GenerateToken(string EmployeeCode, String Pwd, string ipAddress = null, string location = null);

        Task<Tuple<String, String, UserContext, String>> GenerateGoogleToken(string idToken, string ipAddress = null, string location = null);

        Task<TokenModel> ValidateToken(String Token);

        String GetTokenFromContext(UserContext context);

        Task<List<String>> UserFunction(UserContext userContext);

        Task UpdateLastActive(long userId);
    }
    class LoginManager : BaseManager<LoginManager>, ILoginManager
    {
        IConfiguration configuration;
        public LoginManager(IServiceProvider provider, ILogger<LoginManager> logger, IMapper mapper) : base(provider, logger, mapper)
        {
            this.configuration = provider.GetService<IConfiguration>();
        }

        public async Task<TokenModel> ValidateToken(string Token)
        {

            return await ExecuteAsync(async () =>
            {
                string hashToken = Token.Substring(Token.LastIndexOf("."));
                string actualToken = Token.Substring(0, (Token.Length - hashToken.Length));
                var bytes = Convert.FromBase64String(actualToken);
                actualToken = Encoding.UTF8.GetString(bytes);
                string hashTokenToValdate = Convert.ToBase64String(HashingFactory.GetHash("SHA256").ComputeHash(Encoding.UTF8.GetBytes(actualToken.ToString())));
                hashToken = hashToken.Substring(1);
                if (hashTokenToValdate != hashToken)
                {
                    throw new UnauthorizedAccessException(String.Format("Token Modified - Potential Fraud Token : {0}", Token));
                }

                var tokenModel = JsonConvert.DeserializeObject<TokenModel>(actualToken);
                if (tokenModel.Expiry < DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException(String.Format("Token Expired UserId {0} SessionGuid {1} Expired at {2}", tokenModel.context.UserId, tokenModel.context.SessionGuid, tokenModel.Expiry));
                }
                return tokenModel;
            }, "ValidateToken", null);
        }
        public String GetTokenFromContext(UserContext context)
        {
            TokenModel model = new TokenModel();
            model.TokenType = "GenericToken";

            if (context != null)
            {
                context.SessionGuid = Guid.NewGuid().ToString();
                model.context = context;
                var authValueProvider = CustomConfigurationProvider.GetConfigurationSection(configuration, "Auth");
                int expiry = int.Parse(authValueProvider.GetValue("SessionTimeOut"));
                model.Expiry = DateTime.UtcNow.AddMinutes(expiry);
                StringBuilder token = new StringBuilder(JsonConvert.SerializeObject(model));
                var bytes = Encoding.UTF8.GetBytes(token.ToString());
                var encodedString = Convert.ToBase64String(bytes);
                return encodedString + "." + Convert.ToBase64String(HashingFactory.GetHash("SHA256").ComputeHash(Encoding.UTF8.GetBytes(token.ToString())));
            }
            throw new UnauthorizedAccessException();
        }

      
        public async Task<Tuple<String, String, UserContext, String>> GenerateToken(string UserName, String Pwd, string ipAddress = null, string location = null)
        {
            return await ExecuteAsync<Tuple<String, String, UserContext, String>>(async () =>
            {
                try
                {
                    var context = await ExecuteAsync<Tuple<String, UserContext, String>>(async () =>
                    {
                        var loginRepo = repositoryFactory.Get<IUserRepository>();

                        var result = await loginRepo.ValidateUser(UserName, Pwd);

                        if (result?.Item2 != null && result.Item2.UserId > 0)
                        {
                            await loginRepo.RecordUserLogin(result.Item2.UserId, ipAddress, location);
                        }

                        //await UpdateEmployeeLastLogin(result.Item2);
                        return result;
                    }, "GenerateToken", null);

                    return Tuple.Create(context.Item1, GetTokenFromContext(context.Item2), context.Item2, context.Item3);
                }
                catch (ArgumentException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedAccessException(ex.Message, ex);
                }
            }, "GenerateToken", null);
        }

        public async Task<Tuple<String, String, UserContext, String>> GenerateGoogleToken(string idToken, string ipAddress = null, string location = null)
        {
            return await ExecuteAsync<Tuple<String, String, UserContext, String>>(async () =>
            {
                var googleSection = CustomConfigurationProvider.GetConfigurationSection(configuration, "Google");
                var clientId = googleSection?.GetValue("ClientId") ?? "262467975068-u0o6qtjog1o7e1p4jp5kuag34ibhfm1l.apps.googleusercontent.com";

                var validationSettings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

                var loginRepo = repositoryFactory.Get<IUserRepository>();
                var result = await loginRepo.ValidateOrCreateGoogleUser(payload.Email, payload.GivenName, payload.FamilyName, payload.Picture);

                if (result?.Item2 != null && result.Item2.UserId > 0)
                {
                    await loginRepo.RecordUserLogin(result.Item2.UserId, ipAddress, location);
                }

                return Tuple.Create(result.Item1, GetTokenFromContext(result.Item2), result.Item2, result.Item3);
            }, "GenerateGoogleToken", null);
        }


        public async Task<List<String>> UserFunction(UserContext userContext)
        {
            return await ExecuteAsync<List<String>>(async () =>
            {
                var loginRepo = repositoryFactory.Get<IUserRepository>();

                return await loginRepo.UserFunction(userContext);
            }, "UserFunction", userContext);
        }

        public async Task UpdateLastActive(long userId)
        {
            await ExecuteAsync(async () =>
            {
                var loginRepo = repositoryFactory.Get<IUserRepository>();
                await loginRepo.UpdateUserLastActive(userId);
            }, "UpdateLastActive", null);
        }
    }
}
