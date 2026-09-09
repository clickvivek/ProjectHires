using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public  class StateDto
    {
        public int Id { get; set; }
        public string StateCode { get; set; } = null!;
        public string StateName { get; set; } = null!;
        //public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
    }
}
