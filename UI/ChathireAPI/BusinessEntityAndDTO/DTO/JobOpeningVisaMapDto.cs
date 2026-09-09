using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningVisaMapDto
    {
        public long Id { get; set; }

        public long JobOpeningId { get; set; }

        public short VisaId { get; set; }
        public VisaDto? Visa { get; set; }
    }

    public partial class JobOpeningVisaMapForInsertDto
    {
        public short VisaId { get; set; }
    }

    public partial class JobOpeningVisaMapForUpdateDto
    {
        public long? Id { get; set; }

        public long? JobOpeningId { get; set; }

        public short? VisaId { get; set; }
    }
}
