using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class JobOpeningSkill
{
    public long Id { get; set; }

    public long? JobId { get; set; }

    public int? SkillId { get; set; }

    public bool? IsMandate { get; set; }

    public bool? Active { get; set; }

    public DateTime? Updated { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual JobOpening? Job { get; set; }

    public virtual Skill? Skill { get; set; }
}
