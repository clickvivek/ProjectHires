using Microsoft.AspNetCore.Http;
using System;

namespace BusinessEntityAndDTO.DTO
{
    public class DirectCandidateResumeDto
    {
        public long Id { get; set; }
        public long CandidateUserId { get; set; }
        public string FileName { get; set; } = null!;
        public string BlobUrl { get; set; } = null!;
        public int? FileSizeInKb { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedDate { get; set; }
    }

    public class DirectCandidateResumeUploadDto
    {
        public IFormFile? File { get; set; }
        public bool IsPrimary { get; set; }
    }
}
