using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessEntityAndDTO.DTO
{
    public class CandidateProfileSkillDto
    {
        public long? Id { get; set; }

        public long? CandidateProfileid { get; set; }

        public int? SkillId { get; set; }

        public string? Name { get; set; }

        public bool? Active { get; set; }

        public short? ProficiencyLevel { get; set; }

        public short? YearsOfExp { get; set; }

    }

    public class CandidateProfileSkillForInsertDto
    {
        public int? SkillId { get; set; }

        public bool? Active { get; set; }

        public short? ProficiencyLevel { get; set; }

        public short? YearsOfExp { get; set; }

    }

    public class CandidateProfileSkillForUpdateDto
    {
        public long? Id { get; set; }

        public long? CandidateProfileid { get; set; }

        public int? SkillId { get; set; }

        public bool? Active { get; set; }

        public short? ProficiencyLevel { get; set; }

        public short? YearsOfExp { get; set; }

    }
}
