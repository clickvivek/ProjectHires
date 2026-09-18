using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningSkillDto
    {
        public long Id { get; set; }

        public long? JobId { get; set; }

        public int? SkillId { get; set; }

        public bool? IsMandate { get; set; }

        public bool? Active { get; set; }
        public SkillDto? Skill { get; set; }

    }

    public partial class JobOpeningSkillForInsertDto
    {
        public int? SkillId { get; set; }

        public string? Name { get; set; }

        public bool? IsMandate { get; set; }

        public bool? Active { get; set; }

    }

    public partial class JobOpeningSkillForUpdateDto
    {
        public long? Id { get; set; }
        public long? JobId { get; set; }
        public int? SkillId { get; set; }
        public string? Name { get; set; }
        public bool? IsMandate { get; set; }
        public bool? Active { get; set; }
    }


}
