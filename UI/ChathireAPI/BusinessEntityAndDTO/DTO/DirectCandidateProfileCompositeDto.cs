using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class DirectCandidateProfileCompositeDto
    {
        public long UserId { get; set; }
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Linkedin { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? ProfilePic { get; set; }

        public DirectCandidateDetailDto? Details { get; set; }
        public List<DirectCandidateResumeDto> Resumes { get; set; } = new();
        public List<DirectCandidateExperienceDto> Experiences { get; set; } = new();
        public List<DirectCandidateEducationDto> Educations { get; set; } = new();
    }

    public class CandidateAppliedJobDto
    {
        public long Id { get; set; }
        public long JobOpeningId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? JobLocation { get; set; }
        public decimal? FromAmt { get; set; }
        public decimal? ToAmt { get; set; }
        public short StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? ResumeDoc { get; set; }
        public string? Comment { get; set; }
        public DateTime? AppliedDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class DirectCandidateApplyJobDto
    {
        public long JobOpeningId { get; set; }
        public long? ResumeId { get; set; }
        public string? CoverNote { get; set; }
    }
}
