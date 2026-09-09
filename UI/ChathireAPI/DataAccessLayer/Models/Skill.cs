using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Skill
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public bool? IsUserDefined { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual ICollection<CandidateProfileSkill> CandidateProfileSkills { get; } = new List<CandidateProfileSkill>();

    public virtual ICollection<JobOpeningSkill> JobOpeningSkills { get; } = new List<JobOpeningSkill>();
}
