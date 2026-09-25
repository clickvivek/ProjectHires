using AutoMapper;
using BusinessEntityAndDTO.DTO;
using DataAccessLayer.Models;
//using BusinessEntityAndDTO.Entity;
//using BusinessEntityAndDTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Task = DataAccessLayer.Models.Task;

namespace Middleware.Shared
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserType, UserTypeDto>();
            CreateMap<Category, CategoryDto>();

            CreateMap<Domain, DomainDto>();
            CreateMap<EmploymentType, EmploymentTypeDto>();
            CreateMap<JobType, JobTypeDto>();
            CreateMap<Status, StatusDto>();
            CreateMap<Visa, VisaDto>();
            CreateMap<CandidateAvailability, CandidateAvailabilityDto>();
            CreateMap<ConsultancyUser, ConsultancyUserDto>().ReverseMap();
            CreateMap<ConsultancyUser, ConsultancyUserDtoForReturn>().ReverseMap();
            CreateMap<CandidateProfileSkill, CandidateProfileSkillForUpdateDto>().ReverseMap();
            CreateMap< CandidateProfileEmploymentType, CandidateProfileEmploymentTypeDtoForUpdate>().ReverseMap();
            CreateMap<CandidateProfileDomain, CandidateProfileDomainDtoForUpdate>().ReverseMap();
            CreateMap<CandidatePrefLocation, CandidatePrefLocationDtoForUpdate>().ReverseMap();
            CreateMap<CandidatePrefJobType, CandidatePrefJobTypeDtoForUpdate>().ReverseMap();

            CreateMap<JobOpeningSkill, JobOpeningSkillForUpdateDto>().ReverseMap();
            CreateMap<JobOpeningEmploymentType, JobOpeningEmploymentTypeForUpdateDto>().ReverseMap();
            CreateMap<JobOpeningVisaMap, JobOpeningVisaMapForUpdateDto>().ReverseMap();
            CreateMap<JobOpeningLocation, JobOpeningLocationForUpdateDto>().ReverseMap();
            CreateMap<JobOpeningJobType, JobOpeningJobTypeForUpdateDto>().ReverseMap();

            CreateMap<JobOpeningProfileConsultancyComment, JobOpeningProfileConsultancyCommentDtoForInsert>().ReverseMap();
            CreateMap<JobOpeningProfileConsultancyComment, JobOpeningProfileConsultancyCommentDto>().ReverseMap();
            
            CreateMap<CandidateDocument, CandidateDocumentDto>().ReverseMap();

            CreateMap<City, CityDto>()
                .ReverseMap();
            CreateMap<State, StateDto>()
                .ReverseMap();
            CreateMap<CandidateProfile, CandidateProfileDto>()
                .ReverseMap();
            CreateMap<CandidateProfile, CandidateProfileSimplelistDto>()
                .ReverseMap();
            CreateMap<CandidateProfileSkill, CandidateProfileSkillDto>()
                .ForMember(c => c.Name, c => c.MapFrom(d => d.Skill != null ? d.Skill.Name : string.Empty))
                .ReverseMap();

            CreateMap<CandidateProfileEmploymentType, CandidateProfileEmploymentTypeDto>()
                .ForMember(c => c.EmploymentTypeName, c => c.MapFrom(d => d.EmploymentType != null ? d.EmploymentType.Name : string.Empty))
                .ReverseMap();
            
            CreateMap<CandidateProfileDomain, CandidateProfileDomainDto>()
                .ForMember(c => c.DomainName, c => c.MapFrom(d => d.Domain != null ? d.Domain.Name : string.Empty))
                .ReverseMap();
            CreateMap<CandidatePrefLocation, CandidatePrefLocationDto>()
                .ForMember(c => c.CityName, c => c.MapFrom(d => d.City != null ? d.City.City1 : string.Empty))
                .ForMember(c => c.StateCode, c => c.MapFrom(d => d.City != null ? d.City.IdStateNavigation.StateCode : string.Empty))
                .ForMember(c => c.StateName, c => c.MapFrom(d => d.City != null ? d.City.IdStateNavigation.StateName: string.Empty))
                .ReverseMap();
            CreateMap<CandidatePrefJobType, CandidatePrefJobTypeDto>()
                .ForMember(c => c.JobTypeName, c => c.MapFrom(d => d.JobType != null ? d.JobType.Description : string.Empty))
                .ReverseMap();

            CreateMap<Consultancy, ConsultancyDto>().ReverseMap();
            //CandidateProfileMappingStatusDto
            CreateMap<CandidateProfileMappingStatus, CandidateProfileMappingStatusDto>();
            CreateMap<JobOpeningCandidateProfileMap, JobOpeningCandidateProfileDetailMapDto>()
                .ForMember(c=> c.CandidateProfileMappingStatusName,c=>c.MapFrom(d=>d.CandidateProfileMappingStatus != null ? d.CandidateProfileMappingStatus.Name : string.Empty));

            CreateMap<ConsultancyForInsertDto, Consultancy>().ForMember(c => c.Id, c => c.Ignore());

            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserDtoForReturn>().ReverseMap();
            CreateMap<UserDtoForInsert, User>().ForMember(c => c.Id, c => c.Ignore());


            CreateMap<CandidateProfileForInsertDto,CandidateProfile>().ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidateProfileSkillForInsertDto,CandidateProfileSkill>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidateProfileEmploymentTypeForInsertDto,CandidateProfileEmploymentType>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidateProfileDomainForInsertDto,CandidateProfileDomain>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidatePrefLocationForInsertDto,CandidatePrefLocation>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidatePrefJobTypeForInsertDto,CandidatePrefJobType>()
                .ForMember(c => c.Id, c => c.Ignore());

            CreateMap<CandidateProfileDtoForUpdate, CandidateProfile>().ReverseMap();
            CreateMap<CandidateProfileSkillForUpdateDto, CandidateProfileSkill>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidateProfileEmploymentTypeDtoForUpdate, CandidateProfileEmploymentType>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidateProfileDomainDtoForUpdate, CandidateProfileDomain>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidatePrefLocationDtoForUpdate, CandidatePrefLocation>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<CandidatePrefJobTypeDtoForUpdate, CandidatePrefJobType>()
                .ForMember(c => c.Id, c => c.Ignore());

            CreateMap<SubscriptionPlan, SubscriptionPlanDto>().ReverseMap();
            CreateMap<SubscriptionPlanFeature, SubscriptionPlanFeatureDto>().ReverseMap();
            CreateMap<UserSubscriptionPlan, UserSubscriptionPlanDto>().ReverseMap();

            CreateMap<JobOpening, JobOpeningDto>().ReverseMap();
            CreateMap<JobOpeningSkill, JobOpeningSkillDto>().ReverseMap();
            CreateMap<JobOpeningEmploymentType, JobOpeningEmploymentTypeDto>().ReverseMap();
            CreateMap<JobOpeningVisaMap, JobOpeningVisaMapDto>().ReverseMap();
            CreateMap<JobOpeningLocation, JobOpeningLocationDto>().ReverseMap();
            CreateMap<JobOpeningJobType, JobOpeningJobTypeDto>().ReverseMap();
            CreateMap<JobOpeningCandidateProfileMap, JobOpeningCandidateProfileMapDto>().ReverseMap();

            CreateMap<JobOpeningCandidateProfileMapDtoForInsert, JobOpeningCandidateProfileMap>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningForInsertDto, JobOpening>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningSkillForInsertDto, JobOpeningSkill>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningEmploymentTypeForInsertDto, JobOpeningEmploymentType>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningVisaMapForInsertDto, JobOpeningVisaMap>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningLocationForInsertDto, JobOpeningLocation>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningJobTypeForInsertDto, JobOpeningJobType>()
                .ForMember(c => c.Id, c => c.Ignore());

            CreateMap<JobOpeningDtoForUpdate, JobOpening>().ReverseMap();
            CreateMap<JobOpeningSkillForUpdateDto, JobOpeningSkill>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningEmploymentTypeForUpdateDto, JobOpeningEmploymentType>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningVisaMapForUpdateDto, JobOpeningVisaMap>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningLocationForUpdateDto, JobOpeningLocation>()
                .ForMember(c => c.Id, c => c.Ignore());
            CreateMap<JobOpeningJobTypeForUpdateDto, JobOpeningJobType>()
                .ForMember(c => c.Id, c => c.Ignore());

            CreateMap<ConsultancyUserInsertDto, ConsultancyUser>()
               .ForMember(c => c.Id, c => c.Ignore());
            //CreateMap<City, CityDto>()
            //    .ReverseMap();
            CreateMap<Skill, SkillDto>();
            _ = CreateMap<City, CityDto>()
                .ForMember(c => c.StateName, c => c.MapFrom(d => d.IdStateNavigation != null ? d.IdStateNavigation.StateName : String.Empty))
                .ForMember(c => c.StateCode, c => c.MapFrom(d => d.IdStateNavigation != null ? d.IdStateNavigation.StateCode : String.Empty))
                //.ForMember(c => c.CountryCode, c => c.MapFrom(d => d.IdStateNavigation != null && d.IdStateNavigation.CountryCodeNavigation != null ? d.IdStateNavigation.CountryCodeNavigation.CountryCode : String.Empty))
                .ForMember(c => c.CountryName, c => c.MapFrom(d => d.IdStateNavigation != null && d.IdStateNavigation.CountryCodeNavigation != null ? d.IdStateNavigation.CountryCodeNavigation.Name : String.Empty));

            _ = CreateMap<State, StateDto>()
                //.ForMember(c => c.CountryCode, c => c.MapFrom(d => d.IdStateNavigation != null && d.IdStateNavigation.CountryCodeNavigation != null ? d.IdStateNavigation.CountryCodeNavigation.CountryCode : String.Empty))
                .ForMember(c => c.CountryName, c => c.MapFrom(d => d.CountryCodeNavigation != null ? d.CountryCodeNavigation.Name : String.Empty));

            // Direct Candidate mappings
            CreateMap<DirectCandidateDetail, DirectCandidateDetailDto>()
                .ForMember(dest => dest.VisaName, opt => opt.MapFrom(src => src.Visa != null ? src.Visa.Name : null));
            CreateMap<DirectCandidateDetailDtoForUpdate, DirectCandidateDetail>();

            CreateMap<DirectCandidateResume, DirectCandidateResumeDto>().ReverseMap();

            CreateMap<DirectCandidateExperience, DirectCandidateExperienceDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.City1 : null));
            CreateMap<DirectCandidateExperienceForInsertDto, DirectCandidateExperience>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<DirectCandidateEducation, DirectCandidateEducationDto>().ReverseMap();
            CreateMap<DirectCandidateEducationForInsertDto, DirectCandidateEducation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            //CreateMap<Employee, EmployeeDto>()
            //    .ForMember(d => d.Country, o => o.MapFrom(s => s.Country != null ? s.Country.CountryName : String.Empty)); ;

        }
    }
}
