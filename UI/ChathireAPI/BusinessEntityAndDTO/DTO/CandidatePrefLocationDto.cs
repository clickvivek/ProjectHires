using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidatePrefLocationDto
    {
        public long? Id { get; set; }

        public long? CandidateId { get; set; }

        public int? CityId { get; set; }

        public string? CityName { get; set; }

        public string? StateName { get; set; }

        public string? StateCode { get; set; }
    }

    public class CandidatePrefLocationForInsertDto
    {
        public int CityId { get; set; }

    }

    public class CandidatePrefLocationDtoForUpdate
    {
        public long? Id { get; set; }

        public long? CandidateId { get; set; }

        public int? CityId { get; set; }
    }
}
