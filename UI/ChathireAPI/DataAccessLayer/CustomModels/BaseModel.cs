using BusinessEntityAndDTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class BaseModel<I> where I : struct
    {
        public virtual I Id { get; set; }
        public virtual DateTime? Updated { get; set; }
        public virtual long? UpdatedBy { get; set; }

        public virtual void PopulateModified(UserContext userContext)
        {
            this.Updated = DateTime.UtcNow;
            this.UpdatedBy = userContext.UserId;
        }

        public static void PopulateModified(List<BaseModel<I>> baseModels, UserContext userContext)
        {
            foreach (var baseModel in baseModels)
            {
                baseModel.Updated = DateTime.UtcNow;
                baseModel.UpdatedBy = userContext.UserId;
            }
        }
    }
    //CandidatePrefLocations
    public partial class CandidatePrefLocation : BaseModel<long> { }

    //CandidatePrefJobTypes
    public partial class CandidatePrefJobType : BaseModel<long> { }
    //CandidateProfileDomain
    public partial class CandidateProfileDomain : BaseModel<long> { }
    //CandidateProfileSkill
    public partial class CandidateProfileSkill : BaseModel<long> { }

    //City
    public partial class City : BaseModel<int> { }
    //Skill
    public partial class Skill : BaseModel<int> { }
    //User
    public partial class User : BaseModel<long> { }
    //CandidateProfile
    public partial class CandidateProfile : BaseModel<long> { }
    public partial class UserType : BaseModel<long> { }

    public partial class ConsultancyUser : BaseModel<long> { }
    //CandidateAvailability
    public partial class CandidateAvailability : BaseModel<short> { }
    public partial class Visa : BaseModel<short> { }
    //Status
    public partial class Status  : BaseModel<short> { }

    //JobType
    public partial class JobType : BaseModel<short> { }

    public partial class EmploymentType : BaseModel<short> { }

    public partial class Domain : BaseModel<short> { }

    public partial class Category : BaseModel<int> { }

    //State
    public partial class State : BaseModel<int> { }

    public partial class JobOpening : BaseModel<long> { }

    public partial class JobOpeningSkill : BaseModel<long> { }

    public partial class JobOpeningVisaMap : BaseModel<long> { }
    public partial class JobOpeningLocation : BaseModel<long> { }
    public partial class JobOpeningEmploymentType : BaseModel<long> { }
    public partial class JobOpeningJobType : BaseModel<long> { }
    public partial class JobOpeningCandidateProfileMap : BaseModel<long> { }

    //CandidateProfileEmploymentType
    public partial class CandidateProfileEmploymentType : BaseModel<long> { }
    public partial class Consultancy : BaseModel<long> { }
    public partial class CandidateDocument : BaseModel<long> { }
    public partial class JobOpeningProfileConsultancyComment : BaseModel<long> { }
    public partial class SubscriptionPlan : BaseModel<long> { }
    public partial class UserSubscriptionPlan : BaseModel<long> { }
    

}
