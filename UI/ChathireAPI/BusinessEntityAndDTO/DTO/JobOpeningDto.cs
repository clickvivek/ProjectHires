namespace BusinessEntityAndDTO.DTO
{
    public partial class JobOpeningDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Country { get; set; }

        public int? Joiningdays { get; set; }

        public string? Description { get; set; }

        public int? TotalExp { get; set; }

        public DateTime? PostedDate { get; set; }

        public DateTime? LastDate { get; set; }

        public short? NumberOfOpening { get; set; }

        public bool? Active { get; set; }

        public long? ConsultancyUserId { get; set; }

        public short? PriorityId { get; set; }

        public int? CategoryId { get; set; }

        public string? JobLocation { get; set; }

        public string? Postalcode { get; set; }

        public short? ProjectStartId { get; set; }

        public bool? DirectClient { get; set; }

        public bool? IsReviewed { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }

        public bool? NotifyOnCandidateProfileMap { get; set; }

        public bool? NotifyWithResume { get; set; }

        public bool? LocalCandidatePref { get; set; }

        public bool? LocalCandidateOnly { get; set; }

        public bool? IsExpired { get; set; }

        public List<JobOpeningSkillDto> JobOpeningSkills { get; set; } = new List<JobOpeningSkillDto>();
        public List<JobOpeningVisaMapDto> JobOpeningVisaMaps { get; set; } = new List<JobOpeningVisaMapDto>();
        public List<JobOpeningLocationDto> JobOpeningLocations { get; set; } = new List<JobOpeningLocationDto>();
        public List<JobOpeningEmploymentTypeDto> JobOpeningEmploymentTypes { get; set; } = new List<JobOpeningEmploymentTypeDto>();
        public List<JobOpeningJobTypeDto> JobOpeningJobTypes { get; set; } = new List<JobOpeningJobTypeDto>();


        /*
        public JobSkillDto[]? skills { get; set; }
        public JobVisaDto[]? visa { get; set; }
        public JobOpeninglocation[]? JobOpeningLocations { get; set; }
        public JobOpeningEmploymentType[]? EmploymentTypes { get; set; }
        public JobOpeningJobType[]? JobTypes { get; set; }


        //public virtual CategoryDto? Category { get; set; }

        //public virtual ConsultancyUserDto? ConsultancyUser { get; set; }

        //public virtual ICollection<JobOpeningCandidateProfileMapDto> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMapDto>();

        //public virtual ICollection<JobOpeningEmploymentTypeDto> JobOpeningEmploymentTypes { get; } = new List<JobOpeningEmploymentTypeDto>();

        //public virtual ICollection<JobOpeningJobTypeDto> JobOpeningJobTypes { get; } = new List<JobOpeningJobTypeDto>();

        //public virtual ICollection<JobOpeningLocationDto> JobOpeningLocations { get; } = new List<JobOpeningLocationDto>();

        //public virtual ICollection<JobOpeningSkillDto> JobOpeningSkills { get; } = new List<JobOpeningSkillDto>();

        //public virtual ICollection<JobOpeningVisaMapDto> JobOpeningVisaMaps { get; } = new List<JobOpeningVisaMapDto>();

        //public virtual ProjectStartInWeekDto? ProjectStart { get; set; }

        public virtual List<Binding> GetBinding()
        {
            var binding = new List<Binding>();
            Binding.GetBindings("Name", Name, binding);
            Binding.GetBindings("Country", Country, binding);
            Binding.GetBindings("Joiningdays", Joiningdays, binding);
            Binding.GetBindings("Description", Description, binding);
            Binding.GetBindings("TotalExp", TotalExp, binding);
            Binding.GetBindings("PostedDate", PostedDate, binding);
            Binding.GetBindings("LastDate", LastDate, binding);
            Binding.GetBindings("NumberOfOpening", NumberOfOpening, binding);
            Binding.GetBindings("Active", Active, binding);
            Binding.GetBindings("Updated", Updated, binding);
            Binding.GetBindings("ConsultancyUserId", ConsultancyUserId, binding);
            Binding.GetBindings("PriorityID", PriorityId, binding);
            Binding.GetBindings("CategoryId", CategoryId, binding);
            Binding.GetBindings("JobLocation", JobLocation, binding);
            Binding.GetBindings("Postalcode", Postalcode, binding);
            Binding.GetBindings("ProjectStartId", ProjectStartId, binding);
            Binding.GetBindings("DirectClient", DirectClient, binding);
            Binding.GetBindings("IsReviewed", IsReviewed, binding);
            Binding.GetBindings("FromAmt", FromAmt, binding);
            Binding.GetBindings("ToAmt", ToAmt, binding);
            Binding.GetBindings("NotifyOnCandidateProfileMap", NotifyOnCandidateProfileMap, binding);
            Binding.GetBindings("NotifyWithResume", NotifyWithResume, binding);
            Binding.GetBindings("UpdatedBy", UpdatedBy, binding);
            Binding.GetBindings("LocalCandidatePref", LocalCandidatePref, binding);
            Binding.GetBindings("LocalCandidateOnly", LocalCandidateOnly, binding);
            Binding.GetBindings("ReviewedBy", ReviewedBy, binding);
            Binding.GetBindings("IsExpired", IsExpired, binding);

            var complexObj = new Binding("skills", "UT_KeySkill", sqlMetaDataSkill, skills.Select(o => new object[] { o.KeySkillId, o.isMandate, o.Active }).ToList());
            binding.Add(complexObj);
            var complexObjVisa = new Binding("UT_Visa", "UT_Visa", sqlMetaDataVisa, visa.Select(o => new object[] { o.VisaId }).ToList());
            binding.Add(complexObjVisa);
            var complexObjLocation = new Binding("UT_Location", "UT_JobOpeningLocation", sqlMetaDataLocation, JobOpeningLocations.Select(o => new object[] { o.LocationId, o.NumberOfOpenings }).ToList());
            binding.Add(complexObjLocation);
            var complexObjET = new Binding("UT_EmploymentType", "UT_EmploymentType", sqlMetaDataET, EmploymentTypes.Select(o => new object[] { o.EmploymentTypeId,o.Active }).ToList());
            binding.Add(complexObjLocation);
            var complexObjJT = new Binding("UT_JobTypes", "UT_JobTypes", sqlMetaDataJT, JobTypes.Select(o => new object[] { o.JobTypeId, o.Active }).ToList());
            binding.Add(complexObjLocation);
            return binding;
        }

        public static readonly SqlMetaData[] sqlMetaDataSkill = new[]
            {
                  new SqlMetaData("SkillId", SqlDbType.Int),
                   new SqlMetaData("isMandate", SqlDbType.Bit),
                    new SqlMetaData("Active", SqlDbType.Bit)
               };
        public static readonly SqlMetaData[] sqlMetaDataVisa = new[]
           {
                  new SqlMetaData("VisaId", SqlDbType.SmallInt),
            };
        public static readonly SqlMetaData[] sqlMetaDataLocation = new[]
           {
                  new SqlMetaData("LocationId", SqlDbType.BigInt),
                  new SqlMetaData("NumberOfOpenings", SqlDbType.Int)
            };

        public static readonly SqlMetaData[] sqlMetaDataET = new[]
            {
                  new SqlMetaData("EmploymentTypeId", SqlDbType.SmallInt),
                    new SqlMetaData("Active", SqlDbType.Bit)
               };
        public static readonly SqlMetaData[] sqlMetaDataJT = new[]
            {
                  new SqlMetaData("JobTypeId", SqlDbType.SmallInt),
                    new SqlMetaData("Active", SqlDbType.Bit)
               };
        */
    }


    public partial class JobOpeningForInsertDto
    {
        
        public string? Name { get; set; }

        public string? Country { get; set; }

        public int? Joiningdays { get; set; }

        public string? Description { get; set; }

        public int? TotalExp { get; set; }

        public DateTime? PostedDate { get; set; }

        public DateTime? LastDate { get; set; }

        public short? NumberOfOpening { get; set; }

        public bool? Active { get; set; }

        public long? ConsultancyUserId { get; set; }

        public short? PriorityId { get; set; }

        public int? CategoryId { get; set; }

        public string? JobLocation { get; set; }

        public string? Postalcode { get; set; }

        public short? ProjectStartId { get; set; }

        public bool? DirectClient { get; set; }

        public bool? IsReviewed { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }

        public bool? NotifyOnCandidateProfileMap { get; set; }

        public bool? NotifyWithResume { get; set; }

        public bool? LocalCandidatePref { get; set; }

        public bool? LocalCandidateOnly { get; set; }

        public bool? IsExpired { get; set; }

        public List<JobOpeningSkillForInsertDto> JobOpeningSkills { get; set; } = new List<JobOpeningSkillForInsertDto>();
        public List<JobOpeningVisaMapForInsertDto> JobOpeningVisaMaps { get; set; } = new List<JobOpeningVisaMapForInsertDto>();
        public List<JobOpeningLocationForInsertDto> JobOpeningLocations { get; set; } = new List<JobOpeningLocationForInsertDto>();
        public List<JobOpeningEmploymentTypeForInsertDto> JobOpeningEmploymentTypes { get; set; } = new List<JobOpeningEmploymentTypeForInsertDto>();
        public List<JobOpeningJobTypeForInsertDto> JobOpeningJobTypes { get; set; } = new List<JobOpeningJobTypeForInsertDto>();

    }


    public partial class JobOpeningForSearchDto
    {
        
        public List<string> searchStrings { get; set; } = new List<string>();
        public List<int>? cityIds { get; set; } = new List<int>();
        public List<int>? skills { get; set; } = new List<int>();
        public List<int>? visas { get; set; } = new List<int>();
        public List<int>? employmentTypes { get; set; } = new List<int>();
        public List<int>? jobTypes { get; set; } = new List<int>();

        public int? startYearsOfExp { get; set; }
        public int? endYearsOfExp { get; set; }
        public int? RowsOfPage { get; set; }
        public int? PageNumber { get; set; }
        public DateTime? PostedStartDate { get; set; }
        public DateTime? PostedEndDate { get; set; }
        public long? ConsultancyUserId { get; set; }
        public string? PublicProfileUserName { get; set; }

    }

    public partial class JobOpeningForSearchResultsDto
    {
        public string JobOpeningId { get; set; }
        public string PostedDate { get; set; }
        public string LastDate { get; set; }
        public string JobOpeningName { get; set; }
        public string CompanyName { get; set; }
        public string Joiningdays { get; set; }
        public string JobDescription { get; set; }
        public string TotalExp { get; set; }
        public string NumberOfOpening { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFName { get; set; }
        public string UserLName { get; set; }
        public string ProfilePic { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Locations { get; set; }
        public List<string> Visas { get; set; }
        public List<string> EmploymentTypes { get; set; }
        public List<string> JobTypes { get; set; }

    }

    public partial class JobOpeningDtoForUpdate
    {
        public long? Id { get; set; }

        public string? Name { get; set; }

        public string? Country { get; set; }

        public int? Joiningdays { get; set; }

        public string? Description { get; set; }

        public int? TotalExp { get; set; }

        public DateTime? PostedDate { get; set; }

        public DateTime? LastDate { get; set; }

        public short? NumberOfOpening { get; set; }

        public bool? Active { get; set; }

        public long? ConsultancyUserId { get; set; }

        public short? PriorityId { get; set; }

        public int? CategoryId { get; set; }

        public string? JobLocation { get; set; }

        public string? Postalcode { get; set; }

        public short? ProjectStartId { get; set; }

        public bool? DirectClient { get; set; }

        public bool? IsReviewed { get; set; }

        public short? FromAmt { get; set; }

        public short? ToAmt { get; set; }

        public bool? NotifyOnCandidateProfileMap { get; set; }

        public bool? NotifyWithResume { get; set; }

        public bool? LocalCandidatePref { get; set; }

        public bool? LocalCandidateOnly { get; set; }

        public bool? IsExpired { get; set; }

        public List<JobOpeningSkillForUpdateDto>? JobOpeningSkills { get; set; } = new List<JobOpeningSkillForUpdateDto>();
        public List<JobOpeningVisaMapForUpdateDto>? JobOpeningVisaMaps { get; set; } = new List<JobOpeningVisaMapForUpdateDto>();
        public List<JobOpeningLocationForUpdateDto>? JobOpeningLocations { get; set; } = new List<JobOpeningLocationForUpdateDto>();
        public List<JobOpeningEmploymentTypeForUpdateDto>? JobOpeningEmploymentTypes { get; set; } = new List<JobOpeningEmploymentTypeForUpdateDto>();
        public List<JobOpeningJobTypeForUpdateDto>? JobOpeningJobTypes { get; set; } = new List<JobOpeningJobTypeForUpdateDto>();
    }

    public class RecruiterStatsDto
    {
        public int JobRequirementPosted { get; set; }
        public int ExpiredPostings { get; set; }
        public int ResumesReceived { get; set; }
    }
}
