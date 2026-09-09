using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidateProfileMappingStatusDto
    {
        public short Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        //public DateTime? Updated { get; set; }

        //public long? UpdatedBy { get; set; }
    }
}
