namespace BusinessLayer.Services
{
    public interface IResendEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
        Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string otpCode);
        Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, string attachmentFilename, byte[]? attachmentBytes);
        Task<bool> SendReferralInvitationEmailAsync(string toEmail, string referrerName, string referralLink, string? customMessage);
        Task<bool> SendJobExpiredEmailAsync(string toEmail, string userName, string jobTitle, string? companyName, long jobId);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}
