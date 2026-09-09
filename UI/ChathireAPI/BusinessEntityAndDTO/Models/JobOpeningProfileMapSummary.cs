using BusinessEntityAndDTO.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.Models
{
    public class JobOpeningProfileMapSummary
    {
        public long JobOpeningId { get; set; }
        public short? CandidateProfileMappingStatusId { get; set; }
        public string? CandidateProfileMappingStatusName { get; set; }
        public int? Count { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? PostDate { get; set; }
        public string? JobLocation { get; set; }
        public bool? Active { get; set; }
    
    }

    public class JobOpeningProfileMapSummary1
    {
        public long JobOpeningId { get; set; }
        public List<ProfileCountSummary>? profileCountSummary { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? PostDate { get; set; }
        public string? JobLocation { get; set; }
        public bool? Active { get; set; }
        public List<JobOpeningLocationDto>? JobOpeningLocation { get; set; }

    }

    public class JobOpeningSummary
    {
        public long ConsultancyUserId { get; set; }
        public int? Count { get; set; }

    }

    public class ProfileCountSummary
    {
        public short? CandidateProfileMappingStatusId { get; set; }
        public string? CandidateProfileMappingStatusName { get; set; }
        public int? Count { get; set; }
    }


}
