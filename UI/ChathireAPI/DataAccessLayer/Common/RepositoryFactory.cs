//using DataAccessLayer.Audit;
using DataAccessLayer.Models;
//using DataAccessLayer.EFContext;
//using DataAccessLayer.EFContext;
using DataAccessLayer.Repository;
using System;
using System.Collections.Generic;

namespace DataAccessLayer.Common
{
    public interface IRepositoryFactory
    {
        T Get<T>() where T : class, IRepository;

        IRepository GetType(string Table);
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        IServiceProvider servicesProvider;
        EFContexts _context;
        public RepositoryFactory(IServiceProvider services, EFContexts context)
        {
            servicesProvider = services;
            _context = context;
        }

        public T Get<T>() where T : class, IRepository
        {
            if (typeof(T) == typeof(IJobOpeningRepository))
            {
                return new JobOpeningRepository(_context) as T;
            }
            if (typeof(T) == typeof(IUserRepository))
            {
                return new UserRepository(_context) as T;
            }
            if (typeof(T) == typeof(IUserTypeRepository))
            {
                return new UserTypeRepository(_context) as T;
            }
            if (typeof(T) == typeof(ICategoryRepository))
            {
                return new CategoryRepository(_context) as T;
            }
            if (typeof(T) == typeof(IDomainRepository))
            {
                return new DomainRepository(_context) as T;
            }
            if (typeof(T) == typeof(IEmploymentTypeRepository))
            {
                return new EmploymentTypeRepository(_context) as T;
            }
            if (typeof(T) == typeof(IJobTypeRepository))
            {
                return new JobTypeRepository(_context) as T;
            }
            if (typeof(T) == typeof(IStatusRepository))
            {
                return new StatusRepository(_context) as T;
            }
            if (typeof(T) == typeof(IVisaRepository))
            {
                return new VisaRepository(_context) as T;
            }
            if (typeof(T) == typeof(ISkillsRepository))
            {
                return new SkillsRepository(_context) as T;
            }
            if (typeof(T) == typeof(ICityRepository))
            {
                return new CityRepository(_context) as T;
            }
            if (typeof(T) == typeof(IStateRepository))
            {
                return new StateRepository(_context) as T;
            }
            //CandidateAvailability
            if (typeof(T) == typeof(ICandidateAvailabilityRepository))
            {
                return new CandidateAvailabilityRepository(_context) as T;
            }

            //CandidateAvailability
            if (typeof(T) == typeof(ICandidateProfileRepository))
            {
                return new CandidateProfileRepository(_context) as T;
            }

            if (typeof(T) == typeof(IConsultancyUserRepository))
            {
                return new ConsultancyUserRepository(_context) as T;
            }



            if (typeof(T) == typeof(ICandidateDocumentRepository))
            {
                return new CandidateDocumentRepository(_context) as T;
            }

            if (typeof(T) == typeof(ICandidateProfileEmploymentTypesRepository))
            {
                return new CandidateProfileEmploymentTypesRepository(_context) as T;
            }
            if (typeof(T) == typeof(ICandidateProfileDomainsRepository))
            {
                return new CandidateProfileDomainsRepository(_context) as T;
            }
            if (typeof(T) == typeof(ICandidatePrefJobTypesRepository))
            {
                return new CandidatePrefJobTypesRepository(_context) as T;
            }
            if (typeof(T) == typeof(ICandidatePrefLocationRepository))
            {
                return new CandidatePrefLocationRepository(_context) as T;
            }
            
            if (typeof(T) == typeof(ICandidateProfileSkillsRepository))
            {
                return new CandidateProfileSkillsRepository(_context) as T;
            }

            if (typeof(T) == typeof(IJobOpeningSkillsRepository))
            {
                return new JobOpeningSkillsRepository(_context) as T;
            }
            //IJobOpeningCandidateProfileMapRepository
            if (typeof(T) == typeof(IJobOpeningCandidateProfileMapRepository))
            {
                return new JobOpeningCandidateProfileMapRepository(_context) as T;
            }
            //ConsultancyRepository
            if (typeof(T) == typeof(IConsultancyRepository))
            {
                return new ConsultancyRepository(_context) as T;
            }
            if (typeof(T) == typeof(IJobOpeningVisaMapsRepository))
            {
                return new JobOpeningVisaMapsRepository(_context) as T;
            }

            if (typeof(T) == typeof(IJobOpeningLocationsRepository))
            {
                return new JobOpeningLocationsRepository(_context) as T;
            }
            if (typeof(T) == typeof(IJobOpeningEmploymentTypesRepository))
            {
                return new JobOpeningEmploymentTypesRepository(_context) as T;
            }
            if (typeof(T) == typeof(IJobOpeningJobTypesRepository))
            {
                return new JobOpeningJobTypesRepository(_context) as T;
            }
            if (typeof(T) == typeof(IJobOpeningProfileConsultancyCommentRepository))
            {
                return new JobOpeningProfileConsultancyCommentRepository(_context) as T;
            }

            if (typeof(T) == typeof(ISubscriptionRepository))
            {
                return new SubscriptionRepository(_context) as T;
            }

            if (typeof(T) == typeof(IUserSubscriptionPlanRepository))
            {
                return new UserSubscriptionPlanRepository(_context) as T;
            }

            //if (typeof(T) == typeof(IJobOpeningSkillsRepository))
            //{
            //    return new JobOpeningSkillsRepository(_context) as T;
            //}
            return null;
        }

        public IRepository GetType(string T)
        {
            //if (T == "Employee")
            //{
            //    return new EmployeeRepository(_context);
            //}

            
            return null;
        }
    }
}
