using BusinessEntityAndDTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
   
    public class CandidateProfileDto
    {
        public long? Id { get; set; }

        public long UserId { get; set; }

        public long? ConsultancyUserId { get; set; }

        public string? CandidateName { get; set; }

        public string? Title { get; set; }

        public bool? Active { get; set; }

        public DateTime? PostedDate { get; set; }

        public bool? CanRelocate { get; set; }

        public int? TotalExp { get; set; }

        public short StatusId { get; set; }

        public string? LinkedIn { get; set; }

        public string? Resume { get; set; }

        public short? Availability { get; set; }

        public short? VisaId { get; set; }

        public DateTime? VisaExpiryDate { get; set; }

        public string? Passportno { get; set; }

        public DateTime? FirstArrivalDate { get; set; }

        public bool? Backgroundchecked { get; set; }

        public int? CityId { get; set; }

        public bool? RemoteOnly { get; set; }

        public bool? AnyLocation { get; set; }

        public string? Comment { get; set; }

        public string? EmployerInfo { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }


        public string? Email { get; set; }

        public string? Phone { get; set; }

        public short? ConsultingRoleId { get; set; }

        public List<CandidateDocumentDto> CandidateDocuments { get; } = new List<CandidateDocumentDto>();

        public List<CandidatePrefJobTypeDto>? CandidatePrefJobTypes { get; set; } = new List<CandidatePrefJobTypeDto>();

        public List<CandidatePrefLocationDto>? CandidatePrefLocations { get; set; } = new List<CandidatePrefLocationDto>();

        public List<CandidateProfileDomainDto>? CandidateProfileDomains { get; set; } = new List<CandidateProfileDomainDto>();

        public List<CandidateProfileEmploymentTypeDto>? CandidateProfileEmploymentTypes { get; set; } = new List<CandidateProfileEmploymentTypeDto>();

        public List<CandidateProfileSkillDto>? CandidateProfileSkills { get; set; } = new List<CandidateProfileSkillDto>();

        public CityDto? city { get; set; } = new CityDto();

      
    }

    public class CandidateProfileForInsertDtoWithResume
    {
        public FileModel? resume { get; set; }
        public CandidateProfileForInsertDto? candidateProfileForInsertDto { get; set; }

    }
        public class CandidateProfileForInsertDto
    {

        public long? Id { get; set; }

        public long UserId { get; set; }

        public long? ConsultancyUserId { get; set; }

        public string? CandidateName { get; set; }

        public string? Title { get; set; }

        public bool? Active { get; set; }

        public DateTime? PostedDate { get; set; }

        public bool? CanRelocate { get; set; }

        public int? TotalExp { get; set; }

        public short StatusId { get; set; }

        public string? LinkedIn { get; set; }

        public string? Resume { get; set; }

        public short? Availability { get; set; }

        public short? VisaId { get; set; }

        public DateTime? VisaExpiryDate { get; set; }

        public string? Passportno { get; set; }

        public DateTime? FirstArrivalDate { get; set; }

        public bool? Backgroundchecked { get; set; }

        public int? CityId { get; set; }

        public bool? RemoteOnly { get; set; }

        public bool? AnyLocation { get; set; }

        public string? Comment { get; set; }

        public string? EmployerInfo { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }


        public string? Email { get; set; }

        public string? Phone { get; set; }

        public short? ConsultingRoleId { get; set; }

        public List<CandidatePrefJobTypeForInsertDto>? CandidatePrefJobTypes { get; set; } = new List<CandidatePrefJobTypeForInsertDto>();

        public List<CandidatePrefLocationForInsertDto>? CandidatePrefLocations { get; set; } = new List<CandidatePrefLocationForInsertDto>();

        public List<CandidateProfileDomainForInsertDto>? CandidateProfileDomains { get; set; } = new List<CandidateProfileDomainForInsertDto>();

        public List<CandidateProfileEmploymentTypeForInsertDto>? CandidateProfileEmploymentTypes { get; set; } = new List<CandidateProfileEmploymentTypeForInsertDto>();

        public List<CandidateProfileSkillForInsertDto>? CandidateProfileSkills { get; set; } = new List<CandidateProfileSkillForInsertDto>();


    }

    public class CandidateProfileDtoForUpdate
    {
        public long? Id { get; set; }

        public long UserId { get; set; }

        public long? ConsultancyUserId { get; set; }

        public string? CandidateName { get; set; }

        public string? Title { get; set; }

        public bool? Active { get; set; }

        public DateTime? PostedDate { get; set; }

        public bool? CanRelocate { get; set; }

        public int? TotalExp { get; set; }

        public short StatusId { get; set; }

        public string? LinkedIn { get; set; }

        public string? Resume { get; set; }

        public short? Availability { get; set; }

        public short? VisaId { get; set; }

        public DateTime? VisaExpiryDate { get; set; }

        public string? Passportno { get; set; }

        public DateTime? FirstArrivalDate { get; set; }

        public bool? Backgroundchecked { get; set; }

        public int? CityId { get; set; }

        public bool? RemoteOnly { get; set; }

        public bool? AnyLocation { get; set; }

        public string? Comment { get; set; }

        public string? EmployerInfo { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }


        public string? Email { get; set; }

        public string? Phone { get; set; }

        public short? ConsultingRoleId { get; set; }

        public List<CandidateDocumentDtoForUpdate>? CandidateDocuments { get; set; } = new List<CandidateDocumentDtoForUpdate>();

        public List<CandidatePrefJobTypeDtoForUpdate>? CandidatePrefJobTypes { get; set; } = new List<CandidatePrefJobTypeDtoForUpdate>();

        public List<CandidatePrefLocationDtoForUpdate>? CandidatePrefLocations { get; set; } = new List<CandidatePrefLocationDtoForUpdate>();

        public List<CandidateProfileDomainDtoForUpdate>? CandidateProfileDomains { get; set; } = new List<CandidateProfileDomainDtoForUpdate>();

        public List<CandidateProfileEmploymentTypeDtoForUpdate>? CandidateProfileEmploymentTypes { get; set; } = new List<CandidateProfileEmploymentTypeDtoForUpdate>();

        public List<CandidateProfileSkillForUpdateDto>? CandidateProfileSkills { get; set; } = new List<CandidateProfileSkillForUpdateDto>();


    }

    public partial class CandidateProfileForSearchDto
    {
        public List<string> SearchString { get; set; }
        public List<int> skills { get; set; } = new List<int>();
        public List<int> cityIds { get; set; } = new List<int>();
        public List<int> stateIds { get; set; } = new List<int>();
        public List<int> visas { get; set; } = new List<int>();
        //public List<string> searchStrings { get; set; } = new List<string>();
        public int startYearsOfExp { get; set; }
        public int endYearsOfExp { get; set; }
        public int RowsOfPage { get; set; }
        public int PageNumber { get; set; }
        //public DateTime PostedStartDate { get; set; }
        //public DateTime PostedEndDate { get; set; }
    }

    public partial class CandidateProfileForSearchResultsDto
    {
        public string CandidateProfileId { get; set; }
        public string CandidateName { get; set; }
        public string Title { get; set; }
        public string VisaId { get; set; }
        public string Visa { get; set; }
        public string CandidateAvailability { get; set; }
        public string CurrentLocationCity { get; set; }
        public string CurrentLocationState { get; set; }
        public string TotalExp { get; set; }
        public string CanRelocate { get; set; }
        public string RemoteOnly { get; set; }
        public string ConsultancyUserId { get; set; }
        public string ConsultancyUserFName { get; set; }
        public string ConsultancyUserLName { get; set; }
        public string ConsultancyName { get; set; }
        public string EmploymentTypeName { get; set; }
        public string JobTypeName { get; set; }
        public string Resume { get; set; }
        public string ProfilePic { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Locations { get; set; }
        public string? PublicProfileUserName { get; set; }

    }

    public class CandidateProfileSimplelistDto
    {
        public long Id { get; set; }

        public string? CandidateName { get; set; }

        public string? Title { get; set; }

        public bool? CanRelocate { get; set; }

        public int? TotalExp { get; set; }

        public string? LinkedIn { get; set; }

        public short? VisaId { get; set; }
        public int? CityId { get; set; }
        public bool? RemoteOnly { get; set; }
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public List<CandidateDocumentDto> CandidateDocuments { get; } = new List<CandidateDocumentDto>();

        public List<CandidatePrefLocationDto>? CandidatePrefLocations { get; set; } = new List<CandidatePrefLocationDto>();

        public List<CandidateProfileSkillDto>? CandidateProfileSkills { get; set; } = new List<CandidateProfileSkillDto>();

        public CityDto? city { get; set; } = new CityDto();
        public VisaDto? visa { get; set; } = new VisaDto();


    }
}
