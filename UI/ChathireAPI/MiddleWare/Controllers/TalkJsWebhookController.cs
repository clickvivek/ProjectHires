using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using DataAccessLayer.Models;
using BusinessLayer.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MiddleWare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/talkjs")]
    [AllowAnonymous]
    public class TalkJsWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TalkJsWebhookController> _logger;
        private readonly EFContexts _dbContext;
        private readonly IResendEmailService _resendEmailService;

        public TalkJsWebhookController(
            IConfiguration configuration,
            ILogger<TalkJsWebhookController> logger,
            EFContexts dbContext,
            IResendEmailService resendEmailService)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            _resendEmailService = resendEmailService;
        }

        /// <summary>
        /// TalkJS Webhook receiver for notification.triggered events.
        /// Fires when a recipient is offline or has unread messages after the configured TalkJS timeout.
        /// </summary>
        [HttpPost("webhook")]
        [HttpPost("")]
        public async Task<IActionResult> HandleWebhook()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                rawBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(rawBody))
            {
                return BadRequest(new { error = "Empty request body" });
            }

            var secretKey = _configuration["TalkJS:SecretKey"];
            var signatureHeader = Request.Headers["X-TalkJS-Signature"].FirstOrDefault();

            // Verify signature when header is present
            if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(signatureHeader))
            {
                if (!VerifySignature(rawBody, signatureHeader, secretKey))
                {
                    _logger.LogWarning("TalkJS Webhook signature verification failed. Header: {Signature}", signatureHeader);
                    return Unauthorized(new { error = "Invalid TalkJS signature" });
                }
            }

            JObject payload;
            try
            {
                payload = JObject.Parse(rawBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse TalkJS webhook JSON payload.");
                return BadRequest(new { error = "Invalid JSON payload" });
            }

            var eventType = payload["type"]?.ToString();
            _logger.LogInformation("Received TalkJS Webhook Event: {EventType}", eventType);

            // Process unread notification events
            if (string.Equals(eventType, "notification.triggered", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(eventType, "message.sent", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessNotificationEventAsync(payload);
            }

            // Always return 200 OK so TalkJS confirms receipt
            return Ok(new
            {
                status = "success",
                message = "Webhook received",
                eventType = eventType
            });
        }


        /// <summary>
        /// Test endpoint to trigger a sample unread reminder email directly.
        /// </summary>
        [HttpGet("test-reminder")]
        [HttpPost("test-reminder")]
        public async Task<IActionResult> TestReminder(
            [FromQuery] string? toEmail = "vivek1234@sharklasers.com",
            [FromQuery] string? senderName = "Suresh Acharya",
            [FromQuery] string? messageText = "Hello from Suresh Acharya! Nice to connect with you.")
        {
            var recipientEmail = toEmail ?? "vivek1234@sharklasers.com";
            var sender = senderName ?? "Suresh Acharya";
            var message = messageText ?? "Hello! You have an unread message waiting for you on ChatHire.";

            // Attempt to find user by email to get friendly name
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == recipientEmail || u.UserName == recipientEmail);

            var recipientName = user != null ? $"{user.Fname} {user.Lname}".Trim() : "Member";
            if (string.IsNullOrWhiteSpace(recipientName)) recipientName = "Member";

            var subject = $"New message from {sender} on ChatHire";
            var htmlBody = BuildEmailTemplate(recipientName, sender, message, "test-conv");

            var sent = await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody);

            return Ok(new
            {
                success = sent,
                recipient = recipientEmail,
                recipientName = recipientName,
                sender = sender,
                subject = subject,
                messagePreview = message
            });
        }

        /// <summary>
        /// Generates a TalkJS authentication token (JWT) and HMAC signature for identity verification.
        /// </summary>
        [HttpGet("token")]
        [HttpGet("auth")]
        public IActionResult GetToken([FromQuery] string? userId)
        {
            var targetUserId = userId;
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                targetUserId = User?.Claims?.FirstOrDefault(c => c.Type == "id" || c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return BadRequest(new { error = "userId parameter is required." });
            }

            var appId = _configuration["TalkJS:AppId"] ?? "tKzUD2dn";
            var secretKey = _configuration["TalkJS:SecretKey"] ?? "sk_test_R1ul8bBmiFIAsBG9C0CYsIDzK2R8ka2V";

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secretKey))
            {
                return StatusCode(500, new { error = "TalkJS AppId or SecretKey not configured." });
            }

            // 1. Generate JWT Token according to TalkJS specifications
            var exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payloadJson = $"{{\"tokenType\":\"user\",\"iss\":\"{appId}\",\"sub\":\"{targetUserId}\",\"exp\":{exp}}}";
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
            var rawToSign = $"{headerBase64}.{payloadBase64}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var tokenSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToSign));
            var tokenSignatureBase64 = Base64UrlEncode(tokenSignatureBytes);
            var jwtToken = $"{rawToSign}.{tokenSignatureBase64}";

            // 2. Generate Classic HMAC-SHA256 Hex Signature
            var userHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(targetUserId));
            var classicSignature = BitConverter.ToString(userHashBytes).Replace("-", "").ToLowerInvariant();

            // Check if client requested exclusively plain text (e.g., from fetch text())
            var accept = Request.Headers["Accept"].ToString();
            if (accept.Contains("text/plain") && !accept.Contains("application/json"))
            {
                return Content(jwtToken, "text/plain");
            }

            return Ok(new
            {
                token = jwtToken,
                signature = classicSignature,
                userId = targetUserId,
                expiresAt = exp
            });
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Check TalkJS Webhook configuration status.
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var appId = _configuration["TalkJS:AppId"];
            var secretKey = _configuration["TalkJS:SecretKey"];
            var sendGridKey = _configuration["Email:APIKey"];
            var resendKey = _configuration["Resend:ApiKey"];

            return Ok(new
            {
                talkJsAppIdConfigured = !string.IsNullOrEmpty(appId),
                talkJsAppId = appId,
                talkJsSecretKeyConfigured = !string.IsNullOrEmpty(secretKey),
                sendGridConfigured = !string.IsNullOrEmpty(sendGridKey) && sendGridKey.StartsWith("SG."),
                resendConfigured = !string.IsNullOrEmpty(resendKey) && resendKey != "YOUR_RESEND_API_KEY",
                webhookUrlPath = "/api/TalkJsWebhook/webhook",
                serverTimeUtc = DateTime.UtcNow
            });
        }

        private async Task ProcessNotificationEventAsync(JObject payload)
        {
            try
            {
                var data = payload["data"] as JObject ?? payload;
                var recipientObj = data["recipient"] as JObject;
                var senderObj = data["sender"] as JObject;
                var conversationObj = data["conversation"] as JObject;
                var messagesArray = data["messages"] as JArray;

                var recipientId = recipientObj?["id"]?.ToString();
                var recipientName = recipientObj?["name"]?.ToString();
                var recipientEmail = ExtractEmail(recipientObj?["email"]);

                // Fallback: If TalkJS recipient does not have an email, look up the user in database by ID
                if (string.IsNullOrWhiteSpace(recipientEmail) && long.TryParse(recipientId, out long uId))
                {
                    var dbUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == uId);
                    if (dbUser != null)
                    {
                        recipientEmail = dbUser.Email;
                        if (string.IsNullOrWhiteSpace(recipientName))
                        {
                            recipientName = $"{dbUser.Fname} {dbUser.Lname}".Trim();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    _logger.LogWarning("TalkJS notification could not find recipient email for RecipientId: {RecipientId}", recipientId);
                    return;
                }

                if (string.IsNullOrWhiteSpace(recipientName))
                {
                    recipientName = "Member";
                }

                var senderName = senderObj?["name"]?.ToString() ?? "Someone";
                var convId = conversationObj?["id"]?.ToString() ?? "";

                // Get the message preview text
                var messageSnippet = "You have a new message waiting on ChatHire.";
                if (messagesArray != null && messagesArray.Count > 0)
                {
                    var lastMsg = messagesArray.Last;
                    var text = lastMsg?["text"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        messageSnippet = text;
                    }
                }
                else if (data["message"] != null)
                {
                    var text = data["message"]?["text"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        messageSnippet = text;
                    }
                }

                if (messageSnippet.Length > 280)
                {
                    messageSnippet = messageSnippet.Substring(0, 277) + "...";
                }

                var subject = $"New message from {senderName} on ChatHire";
                var htmlBody = BuildEmailTemplate(recipientName, senderName, messageSnippet, convId);

                await SendEmailAsync(recipientEmail, recipientName, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TalkJS notification event.");
            }
        }

        private string? ExtractEmail(JToken? emailToken)
        {
            if (emailToken == null) return null;
            if (emailToken.Type == JTokenType.Array)
            {
                var arr = emailToken as JArray;
                return arr?.FirstOrDefault()?.ToString();
            }
            if (emailToken.Type == JTokenType.String)
            {
                return emailToken.ToString();
            }
            return null;
        }

        private bool VerifySignature(string rawBody, string signatureHeader, string secretKey)
        {
            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return string.Equals(computedSignature, signatureHeader.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TalkJS signature.");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            // 1. Try SendGrid
            var sendGridKey = _configuration["Email:APIKey"];
            if (!string.IsNullOrWhiteSpace(sendGridKey) && sendGridKey.StartsWith("SG."))
            {
                try
                {
                    var client = new SendGridClient(sendGridKey);
                    var fromEmail = _configuration["Email:SenderEmail"] ?? "support@hires.co";
                    var fromName = _configuration["Email:SenderName"] ?? "ChatHire";
                    var from = new EmailAddress(fromEmail, fromName);
                    var to = new EmailAddress(toEmail, toName);
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
                    var response = await client.SendEmailAsync(msg);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("TalkJS unread reminder sent via SendGrid to {Email}", toEmail);
                        return true;
                    }
                    _logger.LogWarning("SendGrid email send returned non-success code: {Status}", response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending unread reminder via SendGrid to {Email}", toEmail);
                }
            }

            // 2. Try Resend
            var resendKey = _configuration["Resend:ApiKey"];
            if (!string.IsNullOrWhiteSpace(resendKey) && resendKey != "YOUR_RESEND_API_KEY")
            {
                try
                {
                    var resendResult = await _resendEmailService.SendEmailAsync(toEmail, subject, htmlBody);
                    if (resendResult)
                    {
                        _logger.LogInformation("TalkJS unread reminder sent via Resend to {Email}", toEmail);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending unread reminder via Resend to {Email}", toEmail);
                }
            }

            // 3. Fallback / Dev Log
            _logger.LogInformation("[TALKJS UNREAD EMAIL NOTIFICATION] To: {Email} ({Name}) | Subject: {Subject}", toEmail, toName, subject);
            Console.WriteLine($"[TALKJS UNREAD REMINDER] Sent to: {toEmail} ({toName}) | Subject: {subject}");
            return true;
        }

        private string BuildEmailTemplate(string recipientName, string senderName, string messageSnippet, string conversationId)
        {
            var chatUrl = "https://chathire.com/#/chat";

            return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>New message on ChatHire</title>
</head>
<body style='margin: 0; padding: 24px 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
  <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0'>
    <tr>
      <td align='center'>
        <table role='presentation' width='580' border='0' cellspacing='0' cellpadding='0' style='background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.06); border: 1px solid #e2e8f0;'>
          
          <!-- Brand Header -->
          <tr>
            <td style='background-color: #70246D; padding: 22px 32px; text-align: left;'>
              <table role='presentation' width='100%' border='0' cellspacing='0' cellpadding='0'>
                <tr>
                  <td>
                    <span style='color: #ffffff; font-size: 22px; font-weight: 700; letter-spacing: -0.5px;'>ChatHire</span>
                  </td>
                  <td align='right'>
                    <span style='color: #f5edf5; font-size: 13px; font-weight: 500; background: rgba(255,255,255,0.15); padding: 4px 10px; border-radius: 12px;'>Unread Message</span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Main Content -->
          <tr>
            <td style='padding: 32px 32px 24px 32px;'>
              <h2 style='color: #0f172a; font-size: 19px; margin-top: 0; margin-bottom: 12px; font-weight: 600;'>
                You have a new unread message
              </h2>
              <p style='color: #475569; font-size: 15px; line-height: 1.5; margin-bottom: 20px;'>
                Hi <strong>{recipientName}</strong>, <strong>{senderName}</strong> sent you a message on ChatHire:
              </p>

              <!-- Message Quote Card -->
              <div style='background-color: #f8fafc; border-left: 4px solid #70246D; border-radius: 6px; padding: 16px 20px; margin-bottom: 28px;'>
                <p style='margin: 0; color: #1e293b; font-size: 15px; line-height: 1.6; font-style: italic;'>
                  &ldquo;{messageSnippet}&rdquo;
                </p>
                <div style='margin-top: 8px; font-size: 12px; color: #94a3b8; font-style: normal;'>
                  From: {senderName}
                </div>
              </div>

              <!-- Action Button -->
              <table role='presentation' border='0' cellspacing='0' cellpadding='0' style='margin-bottom: 28px;'>
                <tr>
                  <td align='center' style='border-radius: 8px; background-color: #70246D;'>
                    <a href='{chatUrl}' target='_blank' style='font-size: 15px; font-weight: 600; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 8px; display: inline-block;'>
                      Reply on ChatHire &rarr;
                    </a>
                  </td>
                </tr>
              </table>

              <p style='color: #64748b; font-size: 13px; line-height: 1.5; margin: 0;'>
                You can reply directly by opening ChatHire. If you have any questions, feel free to visit our platform.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center;'>
              <p style='color: #94a3b8; font-size: 12px; margin: 0; line-height: 1.5;'>
                This email was sent by ChatHire because you have unread messages.
                <br>&copy; {DateTime.UtcNow.Year} ChatHire. All rights reserved.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
