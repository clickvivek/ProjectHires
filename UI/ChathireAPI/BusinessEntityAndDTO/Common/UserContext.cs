using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.Common
{
    public class UserContext
    {
        public long UserId { get; set; }
        public long? UserTypeId { get; set; }
        public long? ConsultancyId { get; set; }
        public bool? ResetPassword { get; set; }
        public String SessionGuid { get; set; }
        public long? ConsultancyUserId { get; set; }

    }
}
