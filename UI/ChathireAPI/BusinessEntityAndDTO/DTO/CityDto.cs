using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CityDto
    {
        public int Id { get; set; }

        public int IdState { get; set; }

        public string City1 { get; set; } = null!;

        public string? Zip { get; set; }

        public string StateCode { get; set; } = null!;

        public string StateName { get; set; } = null!;

        //public string? CountryCode { get; set; }

        public string? CountryName { get; set; }
        public StateDto? IdStateNavigation { get; set; }
    }
}
