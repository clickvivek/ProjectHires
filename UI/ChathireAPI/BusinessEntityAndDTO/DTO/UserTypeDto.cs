using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class UserTypeDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public bool? Active { get; set; }

        //public DateTime? Inserted { get; set; }

        public DateTime? Updated { get; set; }

        public bool? IsInternal { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
