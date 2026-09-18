namespace BusinessLayer.Services
{
    public interface IResendEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
        Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, string attachmentFilename, byte[]? attachmentBytes);
    }
}
