using System;
using System.Collections.Generic;

namespace BusinessEntityAndDTO.DTO
{
    public class SkillsAdminSummaryDto
    {
        public int TotalCount { get; set; }
        public int SystemDefinedCount { get; set; }
        public int UserDefinedCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }

    public class BulkAddSkillsRequestDto
    {
        public string SkillsText { get; set; } = string.Empty;
        public bool IsUserDefined { get; set; } = false;
        public bool Active { get; set; } = true;
    }

    public class BulkAddSkillsResultDto
    {
        public int TotalProcessed { get; set; }
        public int AddedCount { get; set; }
        public int SkippedDuplicateCount { get; set; }
        public List<string> AddedSkills { get; set; } = new List<string>();
        public List<string> SkippedSkills { get; set; } = new List<string>();
    }

    public class CreateSkillRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsUserDefined { get; set; } = false;
        public bool Active { get; set; } = true;
    }

    public class UpdateSkillRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool? Active { get; set; }
        public bool? IsUserDefined { get; set; }
    }

    public class ToggleSkillStatusDto
    {
        public int Id { get; set; }
        public bool Active { get; set; }
    }

    public class ConvertSkillUserDefinedDto
    {
        public int Id { get; set; }
        public bool IsUserDefined { get; set; }
    }

    public class DeleteSkillResultDto
    {
        public bool Success { get; set; }
        public bool WasDeactivated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
