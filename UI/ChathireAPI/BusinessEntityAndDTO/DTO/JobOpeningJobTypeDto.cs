using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningJobTypeDto
    {
        public long Id { get; set; }

        public long? JobOpeningId { get; set; }

        public short? JobTypeId { get; set; }

        public bool? Active { get; set; }
        public JobTypeDto? JobType { get; set; }

    }

    public partial class JobOpeningJobTypeForInsertDto
    {
        public short? JobTypeId { get; set; }
        public bool? Active { get; set; }
    }

    public partial class JobOpeningJobTypeForUpdateDto
    {
        public long? Id { get; set; }

        public long? JobOpeningId { get; set; }

        public short? JobTypeId { get; set; }

        public bool? Active { get; set; }

    }
}
