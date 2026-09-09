using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningProfileConsultancyCommentDto
    {
        public long Id { get; set; }

        public long JobopeningCandidateProfileMapId { get; set; }

        public long ConsultancyUserId { get; set; }

        public string? Comment { get; set; }

        public bool? Active { get; set; }
        public DateTime? Updated { get; set; }

    }

    public partial class JobOpeningProfileConsultancyCommentDtoForInsert
    {
        public long JobopeningCandidateProfileMapId { get; set; }

        public long ConsultancyUserId { get; set; }

        public string? Comment { get; set; }

        public bool? Active { get; set; }
    }

}
