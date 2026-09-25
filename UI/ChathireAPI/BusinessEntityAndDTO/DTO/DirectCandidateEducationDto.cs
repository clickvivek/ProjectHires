using System;

namespace BusinessEntityAndDTO.DTO
{
    public class DirectCandidateEducationDto
    {
        public long Id { get; set; }
        public long CandidateUserId { get; set; }
        public string Institution { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string? Major { get; set; }
        public int? StartYear { get; set; }
        public int? GraduationYear { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DirectCandidateEducationForInsertDto
    {
        public string Institution { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string? Major { get; set; }
        public int? StartYear { get; set; }
        public int? GraduationYear { get; set; }
    }
}
