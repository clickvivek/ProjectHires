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
}
