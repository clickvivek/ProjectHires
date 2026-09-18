//using DataAccessLayer.Audit;
//using DataAccessLayer.Common;
//using EntityAndDTO.AuditEntity;
//using EntityAndDTO.Common;
//using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DataAccessLayer.Common;
using BusinessEntityAndDTO.Common;

namespace BusinessLayer.Common
{
    public class BaseManager<M> : IManager
    {
        protected IServiceProvider serviceProvider;
        protected IRepositoryFactory repositoryFactory;
        protected IManagerFactory managerFactory;
        protected ILogger<M> _logger;
        protected IMapper mapper;

        public BaseManager(IServiceProvider serviceProvider, ILogger<M> logger,IMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            this.managerFactory = serviceProvider.GetService<IManagerFactory>();
            _logger = logger;
            this.mapper = mapper;
        }

        private String GetLogMessage(UserContext userContext, String Message)
        {
            if (userContext != null)
                return string.Format(" UserId : {0} | SessionId : {1}  | Message : {2}", userContext.UserId, userContext.SessionGuid == null ? "" : userContext.SessionGuid, Message);
            else
                return Message;
        }
        protected async Task ExecuteAsync(Func<Task> function, String MethodName, UserContext userContext)
        {
            try
            {
                await function();
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation("Invalid or Missing {ParamName} at {ClassName} {MethodName}", ex.ParamName, typeof(M), MethodName);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error Occured while executing {ClassName} {Methodname]}", typeof(M), MethodName);
                throw;
            }
        }

        protected async Task<E> ExecuteAsync<E>(Func<Task<E>> function, String MethodName, UserContext userContext)
        {
            try
            {
                var result = await function();

                return result;
            }
            catch (ArgumentException ex)
            {
                if (userContext != null)
                    _logger.LogInformation(GetLogMessage(userContext, String.Format("Invalid or Missing {0} at {1} {2}", ex.ParamName, typeof(M), MethodName)));
                else
                    _logger.LogInformation(String.Format("Invalid or Missing {0} at {1} {2}", ex.ParamName, typeof(M), MethodName));
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                if (userContext != null)
                    _logger.LogError(ex, GetLogMessage(userContext, String.Format("Error Occured while executing {0} {1}", typeof(M), MethodName)));
                else
                    _logger.LogError(ex, "Error Occured while executing {ClassName} {Methodname}", typeof(M), MethodName);
                throw;
            }
        }
    }
}