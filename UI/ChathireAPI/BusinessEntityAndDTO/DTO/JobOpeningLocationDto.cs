using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningLocationDto
    {
        public long Id { get; set; }

        public long JobOpeningId { get; set; }

        public int? NumberOfOpenings { get; set; }

        public int? CityId { get; set; }
        public CityDto? City { get; set; }

    }


    public partial class JobOpeningLocationForInsertDto
    {
        public int? NumberOfOpenings { get; set; }
        public int? CityId { get; set; }

    }

    public partial class JobOpeningLocationForUpdateDto
    {
        public long? Id { get; set; }

        public long? JobOpeningId { get; set; }

        public int? NumberOfOpenings { get; set; }

        public int? CityId { get; set; }

    }
}
