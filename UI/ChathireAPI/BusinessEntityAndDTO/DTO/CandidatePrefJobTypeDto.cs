using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidatePrefJobTypeDto
    {
        public long? Id { get; set; }

        public long? CandidateProfileId { get; set; }

        public short? JobTypeId { get; set; }

        public string? JobTypeName { get; set; }
    }

    public class CandidatePrefJobTypeForInsertDto
    {
        public short JobTypeId { get; set; }
    }

    public class CandidatePrefJobTypeDtoForUpdate
    {
        public long? Id { get; set; }

        public long? CandidateProfileId { get; set; }

        public short? JobTypeId { get; set; }
    }
}
