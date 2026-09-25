using System;

namespace BusinessEntityAndDTO.DTO
{
    public class DirectCandidateExperienceDto
    {
        public long Id { get; set; }
        public long CandidateUserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DirectCandidateExperienceForInsertDto
    {
        public string CompanyName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? CityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }
}
