
//using BusinessLayer.Manager;
using AutoMapper;
using BusinessLayer.Manager;
//using BusinessLayer.Manager;
//using BusinessLayer.UserAccessControl;
using DataAccessLayer.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BusinessLayer.Common
{
    public interface IManager
    {
    }

    public interface IManagerFactory
    {
        T Get<T>();
    }

    public class ManagerFactory : IManagerFactory
    {
        Dictionary<Type, Lazy<IManager>> _bslObjects = new Dictionary<Type, Lazy<IManager>>();

        IServiceProvider servicesProvider;

        public ManagerFactory(IServiceProvider services)
        {
            servicesProvider = services;
            var repositoryFactory = services.GetService<IRepositoryFactory>();

            _bslObjects.Add(typeof(ILoginManager), new Lazy<IManager>(() => new LoginManager(services, services.GetService<ILogger<LoginManager>>(), services.GetService<IMapper>())));

            _bslObjects.Add(typeof(ICommonManager), new Lazy<IManager>(() => new CommonManager(services, services.GetService<ILogger<CommonManager>>(), services.GetService<IMapper>())));

            _bslObjects.Add(typeof(IJobOpeningManager), new Lazy<IManager>(() => new JobOpeningManager(services, services.GetService<ILogger<JobOpeningManager>>(), services.GetService<IMapper>())));
            
            _bslObjects.Add(typeof(ICandidateProfileManager), new Lazy<IManager>(() => new CandidateProfileManager(services, services.GetService<ILogger<CandidateProfileManager>>(), services.GetService<IMapper>())));
            
            _bslObjects.Add(typeof(IFileManager), new Lazy<IManager>(() => new FileManager(services, services.GetService<ILogger<FileManager>>(), services.GetService<IMapper>())));

            _bslObjects.Add(typeof(IUserManager), new Lazy<IManager>(() => new UserManager(services, services.GetService<ILogger<UserManager>>(), services.GetService<IMapper>())));
            
            _bslObjects.Add(typeof(IConsultancyManager), new Lazy<IManager>(() => new ConsultancyManager(services, services.GetService<ILogger<ConsultancyManager>>(), services.GetService<IMapper>())));

            _bslObjects.Add(typeof(ISubscriptionManager), new Lazy<IManager>(() => new SubscriptionManager(services, services.GetService<ILogger<SubscriptionManager>>(), services.GetService<IMapper>())));

        }

        public T Get<T>()
        {
            Lazy<IManager> value;
            _bslObjects.TryGetValue(typeof(T), out value);
            return value == null ? throw new TypeLoadException(string.Format("Type {0} not found in Manager Factory", typeof(T).ToString())) : (T)value.Value;
        }
    }
}
