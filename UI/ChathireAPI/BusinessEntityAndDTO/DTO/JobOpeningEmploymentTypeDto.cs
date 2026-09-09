using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningEmploymentTypeDto
    {
        public long Id { get; set; }

        public long? JobOpeningId { get; set; }

        public short? EmploymentTypeId { get; set; }

        public bool? Active { get; set; }
        public EmploymentTypeDto? EmploymentType { get; set; }

    }

    public partial class JobOpeningEmploymentTypeForInsertDto
    {
        public short? EmploymentTypeId { get; set; }
        public bool? Active { get; set; }
    }

    public partial class JobOpeningEmploymentTypeForUpdateDto
    {
        public long? Id { get; set; }

        public long? JobOpeningId { get; set; }

        public short? EmploymentTypeId { get; set; }

        public bool? Active { get; set; }

    }
}
