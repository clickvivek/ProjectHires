namespace BusinessLayer.Services
{
    public interface IResendEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }
}
