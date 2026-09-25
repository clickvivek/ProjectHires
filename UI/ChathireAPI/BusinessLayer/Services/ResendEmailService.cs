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
                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY");
                }
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

        public async Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string otpCode)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var fromEmail = _configuration["Resend:FromEmail"] ?? "no-reply@notifications.chathire.com";
                var fromName = _configuration["Resend:FromName"] ?? "ChatHire";

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    _logger.LogWarning("Resend API key is not configured in appsettings.json [Resend:ApiKey]. Password Reset OTP code generated: {OtpCode} for {Email}", otpCode, toEmail);
                    Console.WriteLine($"[RESEND DEV PASSWORD RESET OTP LOG] Email: {toEmail} | OTP: {otpCode}");
                    return true;
                }

                var htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 28px; border: 1px solid #E9D8E8; border-radius: 12px; background-color: #ffffff;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h2 style='color: #72246C; margin: 0; font-size: 22px;'>Reset Your Password</h2>
                            <p style='color: #667085; font-size: 14px; margin-top: 6px;'>ChatHire Account Security</p>
                        </div>
                        <p style='color: #344054; font-size: 15px; line-height: 1.5;'>We received a request to reset the password for your ChatHire account. Please use the 6-digit verification code below to set a new password:</p>
                        <div style='background-color: #F7EFF7; border: 1px solid #E9D8E8; padding: 18px; border-radius: 10px; text-align: center; margin: 24px 0;'>
                            <span style='font-size: 32px; font-weight: 700; letter-spacing: 8px; color: #72246C;'>{otpCode}</span>
                        </div>
                        <p style='color: #667085; font-size: 13px; line-height: 1.5;'>This code is valid for <strong>10 minutes</strong>. If you did not request a password reset, you can safely ignore this email — your password will remain unchanged.</p>
                        <hr style='border: none; border-top: 1px solid #E4E7EC; margin: 24px 0;' />
                        <p style='color: #98A2B3; font-size: 12px; text-align: center; margin: 0;'>&copy; {DateTime.UtcNow.Year} ChatHire. All rights reserved.</p>
                    </div>";

                var payload = new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { toEmail },
                    subject = $"{otpCode} is your ChatHire password reset code",
                    html = htmlBody
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Password Reset OTP Email sent successfully via Resend to {Email}", toEmail);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send Password Reset OTP email via Resend to {Email}. Status: {Status}, Error: {Response}", toEmail, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending Password Reset OTP email via Resend to {Email}", toEmail);
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

        public async Task<bool> SendReferralInvitationEmailAsync(string toEmail, string referrerName, string referralLink, string? customMessage)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var fromEmail = _configuration["Resend:FromEmail"] ?? "no-reply@notifications.chathire.com";
                var fromName = _configuration["Resend:FromName"] ?? "ChatHire";

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    _logger.LogWarning("Resend API key is not configured. Referral invitation logged for {Email} from {Referrer}", toEmail, referrerName);
                    Console.WriteLine($"[RESEND DEV REFERRAL LOG] To: {toEmail} | Referrer: {referrerName} | Link: {referralLink}");
                    return true;
                }

                string personalizedNote = !string.IsNullOrWhiteSpace(customMessage)
                    ? $@"<div style='background-color: #f8fafc; border-left: 4px solid #39756F; padding: 14px 18px; margin: 20px 0; border-radius: 4px; font-style: italic; color: #334155; font-size: 14px;'>
                            &ldquo;{System.Net.WebUtility.HtmlEncode(customMessage)}&rdquo;
                            <div style='font-style: normal; font-weight: 600; margin-top: 6px; color: #0f172a; font-size: 13px;'>— {referrerName}</div>
                         </div>"
                    : "";

                var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>You're invited to ChatHire</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f1f5f9; padding: 30px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.06);' cellspacing='0' cellpadding='0' border='0'>
                    <!-- Header Banner -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e293b 0%, #39756F 100%); padding: 36px 30px; text-align: center;'>
                            <div style='display: inline-block; background-color: rgba(255,255,255,0.15); padding: 6px 14px; border-radius: 20px; color: #ffffff; font-size: 13px; font-weight: 600; letter-spacing: 0.5px; margin-bottom: 12px; text-transform: uppercase;'>
                                Exclusive Invitation &amp; Bonus
                            </div>
                            <h1 style='color: #ffffff; margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;'>
                                {referrerName} invited you to ChatHire!
                            </h1>
                            <p style='color: #e2e8f0; font-size: 15px; margin: 10px 0 0 0;'>
                                Join today and unlock free recruiting benefits &amp; instant candidate matching.
                            </p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 32px 30px;'>
                            <p style='color: #1e293b; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;'>
                                Hello,
                            </p>
                            <p style='color: #334155; font-size: 15px; line-height: 1.6; margin: 0 0 16px 0;'>
                                <strong>{referrerName}</strong> thinks you'll love using <strong>ChatHire</strong> — the next-generation hiring platform connecting recruiters, bench sales managers, and top tech talent seamlessly with AI matching and real-time chat.
                            </p>

                            {personalizedNote}

                            <!-- Colorful Reward Cards -->
                            <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px; padding: 20px; margin: 24px 0;'>
                                <div style='font-size: 14px; font-weight: 700; color: #0f172a; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 14px;'>
                                    🎁 Special Welcome Rewards When You Join:
                                </div>
                                
                                <table width='100%' cellspacing='0' cellpadding='0' border='0'>
                                    <tr>
                                        <td style='padding: 10px 0; border-bottom: 1px solid #edf2f7;'>
                                            <div style='display: flex; align-items: flex-start;'>
                                                <div style='font-size: 20px; margin-right: 12px;'>💼</div>
                                                <div>
                                                    <div style='font-weight: 700; color: #1e3a8a; font-size: 15px;'>Recruiters &amp; Employers</div>
                                                    <div style='color: #475569; font-size: 13px; margin-top: 2px;'>
                                                        Get <strong>3 Months of Free Job Postings</strong> + instant resume downloads and candidate chat access!
                                                    </div>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 12px 0 4px 0;'>
                                            <div style='display: flex; align-items: flex-start;'>
                                                <div style='font-size: 20px; margin-right: 12px;'>⚡</div>
                                                <div>
                                                    <div style='font-weight: 700; color: #065f46; font-size: 15px;'>Bench Sales Recruiters</div>
                                                    <div style='color: #475569; font-size: 13px; margin-top: 2px;'>
                                                        Enjoy <strong>Unlimited Hotlist &amp; Profile Postings</strong> to market your consultants directly to prime vendors!
                                                    </div>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <!-- Call to Action Button -->
                            <div style='text-align: center; margin: 32px 0 24px 0;'>
                                <a href='{referralLink}' target='_blank' style='display: inline-block; background-color: #39756F; color: #ffffff; text-decoration: none; padding: 14px 34px; font-size: 16px; font-weight: 700; border-radius: 8px; box-shadow: 0 4px 12px rgba(57,117,111,0.35); text-align: center;'>
                                    Claim Free Access &amp; Sign Up &rarr;
                                </a>
                            </div>

                            <p style='color: #64748b; font-size: 13px; text-align: center; margin: 0;'>
                                Or copy and paste this link into your browser:<br />
                                <a href='{referralLink}' style='color: #39756F; word-break: break-all;'>{referralLink}</a>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 30px; border-top: 1px solid #e2e8f0; text-align: center;'>
                            <p style='color: #64748b; font-size: 12px; margin: 0 0 6px 0;'>
                                This invitation was sent to <strong>{toEmail}</strong> on behalf of {referrerName}.
                            </p>
                            <p style='color: #94a3b8; font-size: 11px; margin: 0;'>
                                &copy; {DateTime.UtcNow.Year} ChatHire Inc. All rights reserved. &bull; <a href='https://chathire.com/#/terms' style='color: #64748b; text-decoration: underline;'>Terms</a> &bull; <a href='https://chathire.com/#/privacy' style='color: #64748b; text-decoration: underline;'>Privacy</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                var payload = new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { toEmail },
                    subject = $"{referrerName} invited you to ChatHire (Get 3 Months Free Postings)",
                    html = htmlBody
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Referral invite email sent successfully via Resend to {Email} from {Referrer}", toEmail, referrerName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send referral invite email via Resend to {Email}. Status: {Status}, Error: {Response}", toEmail, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending referral invite email via Resend to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                var fromEmail = _configuration["Resend:FromEmail"] ?? "no-reply@notifications.chathire.com";
                var fromName = _configuration["Resend:FromName"] ?? "ChatHire";

                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_RESEND_API_KEY")
                {
                    _logger.LogWarning("Resend API key is not configured. Email to {Email} [Subject: {Subject}] logged to console.", toEmail, subject);
                    return true;
                }

                var payload = new
                {
                    from = $"{fromName} <{fromEmail}>",
                    to = new[] { toEmail },
                    subject = subject,
                    html = htmlContent
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully via Resend to {Email}", toEmail);
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
                _logger.LogError(ex, "Exception occurred while sending email via Resend to {Email}", toEmail);
                return false;
            }
        }
    }
}
