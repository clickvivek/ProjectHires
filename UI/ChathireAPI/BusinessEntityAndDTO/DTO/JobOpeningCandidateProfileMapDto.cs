using BusinessEntityAndDTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningCandidateProfileMapDto
    {
        public long Id { get; set; }

        public long JobOpeningId { get; set; }

        public long CandidateProfileId { get; set; }

        public DateTime? AppliedDate { get; set; }

        public bool? Active { get; set; }

        public short CandidateProfileMappingStatusId { get; set; }

        public string? Comment { get; set; }

        public long? ConsultancyUserId { get; set; }

        public long? CandidateUserId { get; set; }
        public string? Doc { get; set; }
        public string? CandidateName { get; set; }

        public int? TotalYearsOfExp { get; set; }

        public string? LinkedIn { get; set; }

        public int? CurrentLocationCityid { get; set; }
        public bool IsRead { get; set; }
    }

    public partial class JobOpeningCandidateProfileMapDtoForInsert
    {
        public long JobOpeningId { get; set; }

        public long? CandidateProfileId { get; set; }

        public DateTime? AppliedDate { get; set; }

        public bool? Active { get; set; }

        public short CandidateProfileMappingStatusId { get; set; }

        public string? Comment { get; set; }

        public long? ConsultancyUserId { get; set; }

        public long? CandidateUserId { get; set; }
        public string? Doc { get; set; }
        public string? OriginalDocName { get; set; }

        public string? CandidateName { get; set; }

        public int? TotalYearsOfExp { get; set; }

        public string? LinkedIn { get; set; }

        public int? CurrentLocationCityid { get; set; }

    }

    public class JobOpeningCandidateProfileMapDtoForInsertWithResume
    {
        public FileModel? resume { get; set; }
        public JobOpeningCandidateProfileMapDtoForInsert? jobOpeningCandidateProfileMapDtoForInsert { get; set; }

    }

    public class JobOpeningCandidateProfileDetailMapDto
    {
        public long Id { get; set; }

        public long JobOpeningId { get; set; }

        public long? CandidateProfileId { get; set; }

        public DateTime? AppliedDate { get; set; }

        public bool? Active { get; set; }

        public short CandidateProfileMappingStatusId { get; set; }

        public string? CandidateProfileMappingStatusName { get; set; }

        public string? Comment { get; set; }

        public long? UpdatedBy { get; set; }

        public long? ConsultancyUserId { get; set; }

        public long? CandidateUserId { get; set; }

        public DateTime? Updated { get; set; }

        public CandidateProfileDto CandidateProfile { get; set; } = null!;
        //public List<CandidateDocumentDto> CandidateDocuments { get; } = new List<CandidateDocumentDto>();

        // public CandidateProfileMappingStatusDto CandidateProfileMappingStatus { get; set; } = null!;

        //public virtual User? CandidateUser { get; set; }

        public virtual ConsultancyUserDto? ConsultancyUser { get; set; }

        public virtual JobOpeningDto JobOpening { get; set; } = null!;

        //public virtual ICollection<JobOpeningProfileConsultancyComment> JobOpeningProfileConsultancyComments { get; } = new List<JobOpeningProfileConsultancyComment>();
        public string? Doc { get; set; }
        public string? CandidateName { get; set; }

        public int? TotalYearsOfExp { get; set; }

        public string? LinkedIn { get; set; }

        public int? CurrentLocationCityid { get; set; }
        //public virtual CityDto? city { get; set; }
        public virtual CityDto? CurrentLocationCity { get; set; }
    }
}
