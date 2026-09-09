using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.DataAccess;

namespace BusinessEntityAndDTO.DTO
{
    public partial class UserDto
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
        public long? ConsultancyUserId { get; set; }

        public List<ConsultancyUserDtoForReturn>? ConsultancyUsers { get; set; }

    }

    public partial class UserDtoForInsert
    {
        public string? Fname { get; set; } = null!;

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

    }

    public partial class UserDtoForReturn
    {
        public long Id { get; set; }

        public string Fname { get; set; } = null!;

        public string? UserName { get; set; }

        public string Email { get; set; } = null!;

        public int? CityId { get; set; }

        public string? Phone { get; set; }

        public string? Linkedin { get; set; }

        public bool? Active { get; set; }

        public string? ProfilePic { get; set; }

        public string? Gender { get; set; }

        public long? UserTypeId { get; set; }

        public string? Lname { get; set; }
        public long? ConsultancyUserId { get; set; }

        public List<ConsultancyUserDto>? ConsultancyUsers { get; set; } 


    }


    public partial class UserDetailsDtoForInsert
    {
        public UserDtoForInsert? UserDtoForInsert { get; set; }

        public long? ConsultancyID { get; set; }

    }

    public partial class UserDetailsDtoForUpdate
    {
        public UserDto? UserDtoForUpdate { get; set; }

        public long? ConsultancyID { get; set; }
        public string? PublicProfileUserName { get; set; }
    }
    


    public partial class ConsultancyUserDto
    {
        public long Id { get; set; }

        public long? ConsultancyId { get; set; }

        public bool? Active { get; set; }

        public DateTime? Updated { get; set; }

        public string? PublicProfileUserName { get; set; }

        public long UserId { get; set; }

        public int? NoOfViews { get; set; }

        public long? ParentId { get; set; }

        public int? NoOfPosting { get; set; }

        public long? UpdatedBy { get; set; }

        public UserDto? User { get; set; }

        public virtual ConsultancyDto? Consultancy { get; set; }

    }

    public partial class ConsultancyUserDtoForReturn
    {
        public long Id { get; set; }

        public long? ConsultancyId { get; set; }

        public bool? Active { get; set; }

        public DateTime? Updated { get; set; }

        public string? PublicProfileUserName { get; set; }

        public long UserId { get; set; }

        public int? NoOfViews { get; set; }

        public long? ParentId { get; set; }

        public int? NoOfPosting { get; set; }

        public long? UpdatedBy { get; set; }

        public virtual ConsultancyDto? Consultancy { get; set; }

    }
    public partial class ConsultancyUserInsertDto
    {
        
        public long? ConsultancyId { get; set; }

        public long UserId { get; set; }

        public DateTime? Updated { get; set; }

        public long? UpdatedBy { get; set; }
        public string? PublicProfileUserName { get; set; }

    }



}
