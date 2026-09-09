using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class DomainDto
    {
        public short Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? Updated { get; set; }
    }
}
