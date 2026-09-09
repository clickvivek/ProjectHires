using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.Models
{
    public class LoginModel
    {
        public string? Token { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public long UserTypeId { get; set; }
        public string? UserTypeName { get; set; }
        public long? ConsultancyId { get; set; }
        public List<string> Functions { get; set; }
        public long? ConsultancyUserId { get; set; }
    }
}
