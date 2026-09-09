using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class CandidateProfileSkill
{
    public long Id { get; set; }

    public long? CandidateProfileid { get; set; }

    public int? SkillId { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public short? ProficiencyLevel { get; set; }

    public short? YearsOfExp { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual CandidateProfile? CandidateProfile { get; set; }

    public virtual Skill? Skill { get; set; }
}
