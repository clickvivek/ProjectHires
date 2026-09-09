using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class CandidateDocumentDtoForUpdate
    {
        public long Id { get; set; }

        public short DocumentId { get; set; }

        public long CandidateProfileId { get; set; }

        public string? Doc { get; set; }
    }
}
