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
}
