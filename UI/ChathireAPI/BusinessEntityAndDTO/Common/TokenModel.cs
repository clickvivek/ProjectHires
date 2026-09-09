using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.Common
{
    public class TokenModel
    {
        public UserContext context { get; set; }
        public String TokenType { get; set; }
        public DateTime Expiry { get; set; }
    }
}
