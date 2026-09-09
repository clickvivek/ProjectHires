using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidateProfileDomainDto
    {
        public long? Id { get; set; }

        public long? CandidateProfileId { get; set; }

        public short? DomainId { get; set; }
        public string? DomainName { get; set; }

        public bool? Active { get; set; }

    }

    public class CandidateProfileDomainForInsertDto
    {
        public short? DomainId { get; set; }

        public bool? Active { get; set; }

    }

    public class CandidateProfileDomainDtoForUpdate
    {
        public long? Id { get; set; }

        public long? CandidateProfileId { get; set; }

        public short? DomainId { get; set; }

    }
}
