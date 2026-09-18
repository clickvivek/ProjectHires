using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BusinessLayer.Services
{
    public class ResendEmailService : IResendEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<ResendEmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var fromEmail = _configuration["Resend:FromEmail"] ?? "no-reply@notifications.chathire.com";
                var fromName = _configuration["Resend:FromName"] ?? "ChatHire";

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    _logger.LogWarning("Resend API key is not configured in appsettings.json [Resend:ApiKey]. OTP code generated: {OtpCode} for {Email}", otpCode, toEmail);
                    // Log OTP for local dev if key is missing
                    Console.WriteLine($"[RESEND DEV OTP LOG] Email: {toEmail} | OTP: {otpCode}");
                    return true;
                }

                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 24px; border: 1px solid #e5e7eb; border-radius: 8px;'>
                        <h2 style='color: #4F46E5; margin-bottom: 16px; text-align: center;'>Verify Your Email</h2>
                        <p style='color: #374151; font-size: 15px; line-height: 1.5;'>Thank you for creating an account with ChatHire. Please use the verification code below to complete your registration:</p>
                        <div style='background-color: #f3f4f6; padding: 18px; border-radius: 8px; text-align: center; margin: 24px 0;'>
                            <span style='font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #111827;'>{otpCode}</span>
                        </div>
                        <p style='color: #6b7280; font-size: 13px; line-height: 1.4;'>This OTP is valid for 10 minutes. If you did not sign up for ChatHire, please ignore this email.</p>
                        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;' />
                        <p style='color: #9ca3af; font-size: 12px; text-align: center;'>&copy; {DateTime.UtcNow.Year} ChatHire. All rights reserved.</p>
                    </div>";

                var payload = new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { toEmail },
                    subject = $"{otpCode} is your ChatHire verification code",
                    html = htmlBody
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("OTP Email sent successfully via Resend to {Email}", toEmail);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send OTP email via Resend to {Email}. Status: {Status}, Error: {Response}", toEmail, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending OTP email via Resend to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, string attachmentFilename, byte[]? attachmentBytes)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var fromEmail = _configuration["Resend:FromEmail"] ?? "no-reply@notifications.chathire.com";
                var fromName = _configuration["Resend:FromName"] ?? "ChatHire";

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    _logger.LogWarning("Resend API key is not configured in appsettings.json [Resend:ApiKey]. Email with subject '{Subject}' not sent to {Email}", subject, toEmail);
                    return true;
                }

                object payload;
                if (attachmentBytes != null && attachmentBytes.Length > 0 && !string.IsNullOrWhiteSpace(attachmentFilename))
                {
                    payload = new
                    {
                        from = $"{fromName} <{fromEmail}>",
                        to = new[] { toEmail },
                        subject = subject,
                        html = htmlContent,
                        attachments = new[]
                        {
                            new
                            {
                                filename = attachmentFilename,
                                content = Convert.ToBase64String(attachmentBytes)
                            }
                        }
                    };
                }
                else
                {
                    payload = new
                    {
                        from = $"{fromName} <{fromEmail}>",
                        to = new[] { toEmail },
                        subject = subject,
                        html = htmlContent
                    };
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email with attachment sent successfully via Resend to {Email} | Subject: {Subject}", toEmail, subject);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email via Resend to {Email}. Status: {Status}, Error: {Response}", toEmail, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email with attachment via Resend to {Email}", toEmail);
                return false;
            }
        }
    }
}
