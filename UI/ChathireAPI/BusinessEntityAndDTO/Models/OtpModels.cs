namespace BusinessEntityAndDTO.Models
{
    public class VerifyOtpModel
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }

    public class ResendOtpModel
    {
        public string Email { get; set; } = null!;
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordWithOtpModel
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class CreateAdminUserModel
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string? Phone { get; set; }
    }
}
