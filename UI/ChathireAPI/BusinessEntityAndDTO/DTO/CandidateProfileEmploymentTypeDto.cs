using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidateProfileEmploymentTypeDto
    {
        public long? Id { get; set; }

        public long? CandidateProfileId { get; set; }

        public short? EmploymentTypeId { get; set; }
        public string? EmploymentTypeName { get; set; }

        public bool? Active { get; set; }


    }

    public class CandidateProfileEmploymentTypeForInsertDto
    {

        public short? EmploymentTypeId { get; set; }

        public bool? Active { get; set; }
    }

    public class CandidateProfileEmploymentTypeDtoForUpdate
    {
        public long? Id { get; set; }
        public long? CandidateProfileId { get; set; }
        public short? EmploymentTypeId { get; set; }
    }
}
