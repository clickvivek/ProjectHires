using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class User
{
    public long Id { get; set; }

    public string Fname { get; set; } = null!;

    public string? UserName { get; set; }

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public int? CityId { get; set; }

    public string? Phone { get; set; }

    public string? Linkedin { get; set; }

    public bool? Active { get; set; }

    public string? ProfilePic { get; set; }

    public DateTime? Updated { get; set; }

    public string? AlternateEmail { get; set; }

    public string? Password { get; set; }

    public string? Gender { get; set; }

    public long? UserTypeId { get; set; }

    public string? Hiringforcountry { get; set; }

    public string? Location { get; set; }

    public string? Otpemail { get; set; }

    public bool? EmailVerified { get; set; }

    public string? OtpaltEmail { get; set; }

    public bool? AltEmailVerified { get; set; }

    public DateTime? OtpemailDate { get; set; }

    public DateTime? OtpaltEmailDate { get; set; }

    public bool? ResetPassword { get; set; }

    public string? Lname { get; set; }

    public int? NoOfViews { get; set; }

    public int? NoOfPosting { get; set; }

    public long? UpdatedBy { get; set; }

    public string? OtppwdReset { get; set; }

    public DateTime? OtppwdDateTime { get; set; }

    public bool? RoleRecruiter { get; set; }

    public bool? RoleBenchSales { get; set; }

    public virtual ICollection<CandidateProfile> CandidateProfiles { get; } = new List<CandidateProfile>();

    public virtual City? City { get; set; }

    public virtual ICollection<ConsultancyUser> ConsultancyUsers { get; } = new List<ConsultancyUser>();

    public virtual ICollection<JobOpeningCandidateProfileMap> JobOpeningCandidateProfileMaps { get; } = new List<JobOpeningCandidateProfileMap>();

    public virtual ICollection<UserFavorite> UserFavorites { get; } = new List<UserFavorite>();

    public virtual ICollection<UserPlanTree> UserPlanTreeAssignedUsers { get; } = new List<UserPlanTree>();

    public virtual ICollection<UserPlanTree> UserPlanTreeUsers { get; } = new List<UserPlanTree>();

    public virtual ICollection<UserSubscriptionPlan> UserSubscriptionPlans { get; } = new List<UserSubscriptionPlan>();

    public virtual ICollection<UserLogin> UserLogins { get; } = new List<UserLogin>();

    public virtual UserType? UserType { get; set; }
}
