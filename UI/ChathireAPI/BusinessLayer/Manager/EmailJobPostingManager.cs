using BusinessEntityAndDTO.Common;
using BusinessEntityAndDTO.DTO;
using BusinessLayer.Common;
using BusinessLayer.Services;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer.Manager
{
    public interface IEmailJobPostingManager
    {
        Task<EmailJobPostingQueueDto> ReceiveInboundEmail(InboundEmailWebhookDto dto, UserContext userContext);
        Task<List<EmailJobPostingQueueDto>> GetAllQueueItems(EmailJobPostingFilterDto filter, UserContext userContext);
        Task<int> GetTotalQueueCount(EmailJobPostingFilterDto filter, UserContext userContext);
        Task<EmailJobPostingStatsDto> GetStats(UserContext userContext);
        Task<EmailJobPostingQueueDto?> GetQueueItemById(long id, UserContext userContext);
        Task<EmailJobPostingQueueDto> ProcessQueueItem(long id, UserContext userContext);
        Task<EmailJobPostingQueueDto> ApproveAndPublish(ApproveEmailJobPostingDto dto, UserContext userContext);
        Task<bool> RejectQueueItem(RejectEmailJobPostingDto dto, UserContext userContext);
        Task<bool> DeleteQueueItem(long id, UserContext userContext);
        Task<EmailJobPostingQueueDto> SimulateInboundEmail(SimulateInboundEmailDto dto, UserContext userContext);
        Task<EmailJobPostingQueueDto> SimulateInboundHotlistEmail(SimulateInboundHotlistEmailDto dto, UserContext userContext);
    }

    public class EmailJobPostingManager : BaseManager<EmailJobPostingManager>, IEmailJobPostingManager
    {
        private readonly EFContexts _context;
        private readonly IJobParserService _parserService;
        private readonly IHotlistParserService _hotlistParserService;
        private readonly IResendEmailService _emailService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger<EmailJobPostingManager> _logger;

        public EmailJobPostingManager(
            IServiceProvider serviceProvider,
            EFContexts context,
            IJobParserService parserService,
            IHotlistParserService hotlistParserService,
            IResendEmailService emailService,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<EmailJobPostingManager> logger,
            AutoMapper.IMapper mapper) : base(serviceProvider, logger, mapper)
        {
            _context = context;
            _parserService = parserService;
            _hotlistParserService = hotlistParserService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EmailJobPostingQueueDto> ReceiveInboundEmail(InboundEmailWebhookDto dto, UserContext userContext)
        {
            string rawFrom = dto.From ?? dto.Data?.From ?? string.Empty;
            string cleanEmail = ExtractPureEmail(rawFrom);
            string senderName = dto.SenderName ?? ExtractSenderDisplayName(rawFrom);
            string subject = dto.Subject ?? dto.Data?.Subject ?? "Job Opening";
            string bodyText = dto.Text ?? dto.Data?.Text ?? string.Empty;
            string bodyHtml = dto.Html ?? dto.Data?.Html ?? string.Empty;
            string? emailId = dto.Email_Id ?? dto.Data?.Email_Id ?? dto.Data?.Id;

            // If body is empty and Resend email_id is present, fetch full email body via Resend API
            if (string.IsNullOrWhiteSpace(bodyText) && string.IsNullOrWhiteSpace(bodyHtml) && !string.IsNullOrWhiteSpace(emailId))
            {
                try
                {
                    var (resendText, resendHtml, resendSubject, resendFrom) = await FetchEmailFromResend(emailId);
                    if (!string.IsNullOrWhiteSpace(resendText)) bodyText = resendText;
                    if (!string.IsNullOrWhiteSpace(resendHtml)) bodyHtml = resendHtml;
                    if (!string.IsNullOrWhiteSpace(resendSubject) && (string.IsNullOrWhiteSpace(subject) || subject == "Job Opening")) subject = resendSubject;
                    if (!string.IsNullOrWhiteSpace(resendFrom) && string.IsNullOrWhiteSpace(cleanEmail))
                    {
                        rawFrom = resendFrom;
                        cleanEmail = ExtractPureEmail(resendFrom);
                        senderName = ExtractSenderDisplayName(resendFrom);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to fetch email body from Resend API for email_id {EmailId}: {Message}", emailId, ex.Message);
                }
            }

            var queueItem = new EmailJobPostingQueue
            {
                SenderEmail = cleanEmail,
                SenderName = senderName,
                EmailSubject = subject,
                RawEmailBodyText = bodyText,
                RawEmailBodyHtml = bodyHtml,
                ReceivedDate = DateTime.UtcNow,
                Status = "Received"
            };

            _context.EmailJobPostingQueues.Add(queueItem);
            await _context.SaveChangesAsync();

            // Guard against empty sender email
            if (string.IsNullOrWhiteSpace(cleanEmail))
            {
                queueItem.Status = "Failed";
                queueItem.ErrorMessage = "Sender email address was not provided in the inbound webhook payload.";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return MapToDto(queueItem);
            }

            // 1. Verify Recruiter using BOTH dbo.User.Email AND dbo.User.AlternateEmail
            var matchedUser = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email != null && u.Email.Trim() != "" && u.Email.ToLower() == cleanEmail.ToLower())
                                       || (u.AlternateEmail != null && u.AlternateEmail.Trim() != "" && u.AlternateEmail.ToLower() == cleanEmail.ToLower()));

            if (matchedUser == null || matchedUser.Active != true)
            {
                queueItem.Status = "Failed";
                queueItem.ErrorMessage = $"Sender email '{cleanEmail}' is not registered with an active ChatHire recruiter account. Please sign up or send from your registered primary or alternate email.";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send rejection email notification
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            cleanEmail,
                            "Job Posting Not Processed - Unregistered Email Address",
                            $"<h3>ChatHire Job Posting Notice</h3><p>Hello,</p><p>We received a job posting submission from <strong>{cleanEmail}</strong>, but this email address is not registered with an active ChatHire employer or recruiter account.</p><p>Please <a href='https://chathire.com/auth/login'>login</a> to update your primary/alternate email or create an account at <a href='https://chathire.com'>chathire.com</a>.</p>");
                    }
                    catch { }
                });

                return MapToDto(queueItem);
            }

            // Determine if matched on Primary or Alternate
            bool isPrimary = matchedUser.Email != null && matchedUser.Email.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase);
            queueItem.UserId = matchedUser.Id;
            queueItem.MatchedEmailType = isPrimary ? "Primary" : "Alternate";

            // Find Consultancy ID
            var consultancyUser = await _context.ConsultancyUsers.FirstOrDefaultAsync(cu => cu.UserId == matchedUser.Id);
            if (consultancyUser != null)
            {
                queueItem.ConsultancyId = consultancyUser.ConsultancyId;
            }

            // Check recipient address and hotlist characteristics
            string recipient = dto.To ?? dto.RecipientEmail ?? string.Empty;
            if (dto.Data?.To != null)
            {
                recipient += " " + dto.Data.To.ToString();
            }
            if (dto.Data?.Received_For != null && dto.Data.Received_For.Count > 0)
            {
                recipient += " " + string.Join(" ", dto.Data.Received_For);
            }

            string rLower = recipient.ToLower();
            bool isSentToHotlistInbox = rLower.Contains("posthotlist") || rLower.Contains("hotlist@");
            bool isSentToJobPostingsInbox = rLower.Contains("postingjobs") || rLower.Contains("jobpostings") || rLower.Contains("jobs@");
            bool isHotlistContent = IsHotlistContent(subject, bodyText, bodyHtml);

            // If postingjobs@chathire.com / jobpostings@chathire.com receives a hotlist email, ignore it
            if (isHotlistContent && (isSentToJobPostingsInbox || (!isSentToHotlistInbox && !string.IsNullOrWhiteSpace(recipient))))
            {
                queueItem.Status = "Ignored";
                queueItem.ErrorMessage = "Hotlist emails sent to postingjobs@chathire.com / jobpostings@chathire.com are ignored. Please submit hotlist candidates to posthotlist@chathire.com.";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            cleanEmail,
                            "Notice: Hotlist submission received at Job Postings inbox",
                            $@"<h3>Hotlist Submission Notice</h3>
                            <p>Hello {matchedUser.Fname},</p>
                            <p>We received your consultant hotlist email, but it was sent to <strong>{(!string.IsNullOrWhiteSpace(recipient) ? recipient : "jobpostings@chathire.com")}</strong> which is only designated for regular job postings.</p>
                            <p>To automatically post bench consultants to your ChatHire Hotlist, please forward or send your hotlist email to: <br/>
                            <strong style='color:#4f46e5; font-size:16px;'>posthotlist@chathire.com</strong></p>
                            <p>This ensures your candidates are accurately parsed and published to your <a href='https://chathire.com/myhotlist'>My Hotlist</a> account.</p>");
                    }
                    catch { }
                });

                return MapToDto(queueItem);
            }

            bool isHotlist = isSentToHotlistInbox || isHotlistContent;

            if (isHotlist)
            {
                if (string.IsNullOrWhiteSpace(bodyText) && string.IsNullOrWhiteSpace(bodyHtml))
                {
                    queueItem.Status = "Failed";
                    queueItem.ErrorMessage = "Email body is empty. Hotlist candidate details must be provided in the email.";
                    queueItem.ProcessedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return MapToDto(queueItem);
                }

                ParsedHotlistDataDto parsedHotlist;
                try
                {
                    parsedHotlist = await _hotlistParserService.ParseHotlistEmailAsync(subject, bodyText, bodyHtml);
                    if (parsedHotlist.Candidates == null || parsedHotlist.Candidates.Count == 0)
                    {
                        throw new InvalidOperationException("No consultant candidate profiles could be extracted from this hotlist email.");
                    }
                    queueItem.ParsedJobJson = JsonSerializer.Serialize(parsedHotlist);
                    queueItem.Status = "Parsed";
                }
                catch (Exception ex)
                {
                    queueItem.Status = "Failed";
                    queueItem.ErrorMessage = $"Hotlist extraction error: {ex.Message}";
                    queueItem.ProcessedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return MapToDto(queueItem);
                }

                // Auto Publish to CandidateProfile
                try
                {
                    var createdCandidateIds = await PublishHotlistInternal(queueItem, parsedHotlist, matchedUser);
                    queueItem.Status = "HotlistPublished";
                    queueItem.ParsedJobJson = JsonSerializer.Serialize(parsedHotlist);
                    queueItem.ProcessedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Send success confirmation receipt for hotlist
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var candListHtml = string.Join("", parsedHotlist.Candidates.Select(c =>
                                $"<li style='margin-bottom:8px;'><strong>{c.Name}</strong> - {c.Title} ({(c.TotalExp.HasValue && c.TotalExp.Value > 0 ? $"{c.TotalExp.Value}+ yrs exp" : "Exp Not Specified")}, {c.Visa ?? "Visa Not Specified"}, {c.Location ?? "Remote"})<br/><span style='color:#4f46e5; font-size:13px;'>Skills: {string.Join(", ", c.Skills ?? new List<string>())}</span></li>"));

                            await _emailService.SendEmailAsync(
                                cleanEmail,
                                $"✅ Hotlist Published: {parsedHotlist.Candidates.Count} Candidate(s) Live on ChatHire",
                                $@"<h3>Your Hotlist Candidates are Live on ChatHire!</h3>
                                <p>Hello {matchedUser.Fname},</p>
                                <p>We have successfully parsed and published <strong>{parsedHotlist.Candidates.Count} candidate profile(s)</strong> from your hotlist submission.</p>
                                <div style='background:#f8f9fa; border:1px solid #e9ecef; padding:15px; border-radius:6px; margin:15px 0;'>
                                    <ul style='padding-left:20px; margin:0;'>
                                        {candListHtml}
                                    </ul>
                                </div>
                                <p><a href='https://chathire.com/myhotlist' style='background:#4f46e5; color:#ffffff; padding:10px 18px; text-decoration:none; border-radius:4px; font-weight:bold; display:inline-block;'>Manage Your Hotlist Candidates</a></p>");
                        }
                        catch { }
                    });
                }
                catch (Exception ex)
                {
                    queueItem.Status = "Failed";
                    queueItem.ErrorMessage = $"Failed to publish hotlist candidates: {ex.Message}";
                    await _context.SaveChangesAsync();
                }

                return MapToDto(queueItem);
            }

            // 2. Check Subscription Plan & Quota for Job Postings (Supports both Free Tier and Paid Plans)
            var subRepo = repositoryFactory.Get<IUserSubscriptionPlanRepository>();
            var quota = await subRepo.GetUserQuotaStatus(matchedUser.Id, userContext);

            var plan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(p => p.UserId == matchedUser.Id && p.Active == true && (p.EndDate == null || p.EndDate >= DateTime.UtcNow));

            if (quota.RemainingJobPostings <= 0)
            {
                queueItem.Status = "QuotaExceeded";
                queueItem.ErrorMessage = $"Recruiter '{matchedUser.Fname} {matchedUser.Lname}' has reached their job posting quota ({quota.UsedJobPostings}/{quota.MaxJobPostings} used). Please upgrade plan or apply promo code.";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            cleanEmail,
                            "ChatHire Notice - Job Posting Quota Reached",
                            $"<h3>Job Posting Quota Reached</h3><p>Hello {matchedUser.Fname},</p><p>We received your job posting <em>'{subject}'</em>, but your account has reached its current job posting limit ({quota.UsedJobPostings}/{quota.MaxJobPostings} used).</p><p>Please visit your <a href='https://chathire.com/recruiter/subscription'>Subscription Dashboard</a> to top up or apply a promo code.</p>");
                    }
                    catch { }
                });

                return MapToDto(queueItem);
            }

            // 3. Extraction & Validation (NEVER create or publish job content if email body is missing)
            if (string.IsNullOrWhiteSpace(bodyText) && string.IsNullOrWhiteSpace(bodyHtml))
            {
                queueItem.Status = "Failed";
                queueItem.ErrorMessage = "Email body is empty. Job posting content must be provided in the email and will never be fabricated or auto-generated.";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return MapToDto(queueItem);
            }

            ParsedJobDataDto parsedData;
            try
            {
                parsedData = await _parserService.ParseJobEmailAsync(subject, bodyText, bodyHtml);
                if (string.IsNullOrWhiteSpace(parsedData.Description) || parsedData.Description.Trim().Length < 15)
                {
                    throw new InvalidOperationException("Email does not contain a valid job description body.");
                }
                queueItem.ParsedJobJson = JsonSerializer.Serialize(parsedData);
                queueItem.Status = "Parsed";
            }
            catch (Exception ex)
            {
                queueItem.Status = "Failed";
                queueItem.ErrorMessage = $"Extraction error: {ex.Message}";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return MapToDto(queueItem);
            }

            // 4. Auto Publish to JobOpening
            try
            {
                long newJobId = await PublishJobInternal(queueItem, parsedData, matchedUser, plan);
                queueItem.CreatedJobOpeningId = newJobId;
                queueItem.Status = "Published";
                queueItem.ProcessedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send success confirmation receipt
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string locationSuffix = !string.IsNullOrWhiteSpace(parsedData.JobLocation) ? $" - {parsedData.JobLocation}" : "";
                        await _emailService.SendEmailAsync(
                            cleanEmail,
                            $"✅ Job Published: {parsedData.Name}{locationSuffix}",
                            $@"<h3>Your Job is Live on ChatHire!</h3>
                            <p>Hello {matchedUser.Fname},</p>
                            <p>Your job posting sent via email has been successfully processed and published.</p>
                            <div style='background:#f8f9fa; border:1px solid #e9ecef; padding:15px; border-radius:6px; margin:15px 0;'>
                                <h4 style='margin-top:0; color:#4f46e5;'>{parsedData.Name}</h4>
                                <p><strong>Location:</strong> {parsedData.JobLocation ?? "Remote"}</p>
                                <p><strong>Experience:</strong> {(parsedData.TotalExp.HasValue && parsedData.TotalExp.Value > 0 ? $"{parsedData.TotalExp.Value}+ years" : "Not specified")}</p>
                                <p><strong>Key Skills:</strong> {(parsedData.Skills != null && parsedData.Skills.Count > 0 ? string.Join(", ", parsedData.Skills) : "Not specified")}</p>
                            </div>
                            <p><a href='https://chathire.com/recruiter/job-openings/view/{newJobId}' style='background:#4f46e5; color:#ffffff; padding:10px 18px; text-decoration:none; border-radius:4px; font-weight:bold; display:inline-block;'>View / Edit Job Posting</a></p>
                            <p style='color:#6c757d; font-size:13px;'>Remaining job postings on your plan: {Math.Max(0, quota.RemainingJobPostings - 1)}</p>");
                    }
                    catch { }
                });
            }
            catch (Exception ex)
            {
                queueItem.Status = "Failed";
                queueItem.ErrorMessage = $"Failed to create regular job opening: {ex.Message}";
                await _context.SaveChangesAsync();
            }

            return MapToDto(queueItem);
        }

        public async Task<List<EmailJobPostingQueueDto>> GetAllQueueItems(EmailJobPostingFilterDto filter, UserContext userContext)
        {
            var repo = repositoryFactory.Get<IEmailJobPostingRepository>();
            return await repo.GetAllAsync(filter, userContext);
        }

        public async Task<int> GetTotalQueueCount(EmailJobPostingFilterDto filter, UserContext userContext)
        {
            var repo = repositoryFactory.Get<IEmailJobPostingRepository>();
            return await repo.GetTotalCountAsync(filter, userContext);
        }

        public async Task<EmailJobPostingStatsDto> GetStats(UserContext userContext)
        {
            var repo = repositoryFactory.Get<IEmailJobPostingRepository>();
            return await repo.GetStatsAsync(userContext);
        }

        public async Task<EmailJobPostingQueueDto?> GetQueueItemById(long id, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == id);
            return item != null ? MapToDto(item) : null;
        }

        public async Task<EmailJobPostingQueueDto> ProcessQueueItem(long id, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == id);
            if (item == null) throw new KeyNotFoundException($"Queue item with ID {id} not found.");

            var parsed = await _parserService.ParseJobEmailAsync(item.EmailSubject ?? string.Empty, item.RawEmailBodyText ?? string.Empty, item.RawEmailBodyHtml);
            item.ParsedJobJson = JsonSerializer.Serialize(parsed);
            item.Status = "Parsed";
            item.ProcessedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToDto(item);
        }

        public async Task<EmailJobPostingQueueDto> ApproveAndPublish(ApproveEmailJobPostingDto dto, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == dto.QueueId);
            if (item == null) throw new KeyNotFoundException($"Queue item with ID {dto.QueueId} not found.");

            // Match recruiter if not already matched
            User? user = null;
            if (item.UserId.HasValue)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == item.UserId.Value);
            }
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => (u.Email != null && u.Email.ToLower() == item.SenderEmail.ToLower())
                                                                 || (u.AlternateEmail != null && u.AlternateEmail.ToLower() == item.SenderEmail.ToLower()));
            }

            if (user == null)
            {
                throw new InvalidOperationException($"Cannot publish job: Sender '{item.SenderEmail}' is not matched with a registered recruiter account.");
            }

            var plan = await _context.UserSubscriptionPlans
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.Active == true);

            var parsedData = new ParsedJobDataDto
            {
                Name = dto.Name,
                Description = dto.Description,
                JobLocation = dto.JobLocation ?? "Remote",
                TotalExp = dto.TotalExp ?? 5,
                FromAmt = dto.FromAmt,
                ToAmt = dto.ToAmt,
                NumberOfOpening = dto.NumberOfOpening ?? 1,
                Skills = dto.Skills ?? new List<string>(),
                EmploymentTypes = dto.EmploymentTypes ?? new List<string>(),
                JobTypes = dto.JobTypes ?? new List<string>(),
                Visas = dto.Visas ?? new List<string>(),
                Locations = dto.Locations ?? new List<string>()
            };

            long newJobId = await PublishJobInternal(item, parsedData, user, plan);
            item.CreatedJobOpeningId = newJobId;
            item.Status = "Published";
            item.ParsedJobJson = JsonSerializer.Serialize(parsedData);
            item.ProcessedDate = DateTime.UtcNow;
            item.UpdatedBy = userContext.UserId;
            await _context.SaveChangesAsync();

            // Send success confirmation receipt
            _ = Task.Run(async () =>
            {
                try
                {
                    string locationSuffix = !string.IsNullOrWhiteSpace(parsedData.JobLocation) ? $" - {parsedData.JobLocation}" : "";
                    await _emailService.SendEmailAsync(
                        user.Email ?? item.SenderEmail,
                        $"✅ Job Published: {parsedData.Name}{locationSuffix}",
                        $@"<h3>Your Job is Live on ChatHire!</h3>
                        <p>Hello {user.Fname},</p>
                        <p>Your job posting has been approved and published.</p>
                        <div style='background:#f8f9fa; border:1px solid #e9ecef; padding:15px; border-radius:6px; margin:15px 0;'>
                            <h4 style='margin-top:0; color:#4f46e5;'>{parsedData.Name}</h4>
                            <p><strong>Location:</strong> {parsedData.JobLocation ?? "Remote"}</p>
                            <p><strong>Experience:</strong> {(parsedData.TotalExp.HasValue && parsedData.TotalExp.Value > 0 ? $"{parsedData.TotalExp.Value}+ years" : "Not specified")}</p>
                            <p><strong>Key Skills:</strong> {(parsedData.Skills != null && parsedData.Skills.Count > 0 ? string.Join(", ", parsedData.Skills) : "Not specified")}</p>
                        </div>
                        <p><a href='https://chathire.com/recruiter/job-openings/view/{newJobId}' style='background:#4f46e5; color:#ffffff; padding:10px 18px; text-decoration:none; border-radius:4px; font-weight:bold; display:inline-block;'>View / Edit Job Posting</a></p>");
                }
                catch { }
            });

            return MapToDto(item);
        }

        public async Task<bool> RejectQueueItem(RejectEmailJobPostingDto dto, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == dto.QueueId);
            if (item == null) return false;

            item.Status = "Rejected";
            item.ErrorMessage = dto.Reason ?? "Rejected manually by admin.";
            item.ProcessedDate = DateTime.UtcNow;
            item.UpdatedBy = userContext.UserId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQueueItem(long id, UserContext userContext)
        {
            var item = await _context.EmailJobPostingQueues.FirstOrDefaultAsync(q => q.Id == id);
            if (item == null) return false;

            _context.EmailJobPostingQueues.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<EmailJobPostingQueueDto> SimulateInboundEmail(SimulateInboundEmailDto dto, UserContext userContext)
        {
            var webhookDto = new InboundEmailWebhookDto
            {
                From = dto.SenderEmail,
                SenderName = dto.SenderName,
                To = dto.RecipientEmail ?? "jobpostings@chathire.com",
                RecipientEmail = dto.RecipientEmail ?? "jobpostings@chathire.com",
                Subject = dto.EmailSubject,
                Text = dto.EmailBody,
                Html = $"<pre>{dto.EmailBody}</pre>"
            };

            return await ReceiveInboundEmail(webhookDto, userContext);
        }

        public async Task<EmailJobPostingQueueDto> SimulateInboundHotlistEmail(SimulateInboundHotlistEmailDto dto, UserContext userContext)
        {
            var webhookDto = new InboundEmailWebhookDto
            {
                From = dto.SenderEmail,
                SenderName = dto.SenderName,
                To = dto.RecipientEmail ?? "posthotlist@chathire.com",
                RecipientEmail = dto.RecipientEmail ?? "posthotlist@chathire.com",
                Subject = dto.EmailSubject,
                Text = dto.EmailBody,
                Html = $"<pre>{dto.EmailBody}</pre>"
            };

            return await ReceiveInboundEmail(webhookDto, userContext);
        }

        private bool IsHotlistContent(string subject, string bodyText, string bodyHtml)
        {
            if (!string.IsNullOrWhiteSpace(subject))
            {
                string s = subject.ToLower();
                if (s.Contains("hotlist") || s.Contains("hot list") || s.Contains("bench list") || s.Contains("benchlist") || s.Contains("available consultant") || s.Contains("bench consultant") || s.Contains("available candidates") || s.Contains("consultant list") || s.Contains("consultants list"))
                {
                    return true;
                }
            }

            // Check if body starts with or strongly indicates hotlist
            string contentSample = (bodyText ?? string.Empty);
            if (contentSample.Length > 500) contentSample = contentSample.Substring(0, 500);
            contentSample = contentSample.ToLower();
            if (contentSample.Contains("hotlist") || contentSample.Contains("hot list") || contentSample.Contains("bench list") || contentSample.Contains("benchlist") || contentSample.Contains("available candidates") || contentSample.Contains("available consultants") || contentSample.Contains("consultant list"))
            {
                return true;
            }

            return false;
        }

        private bool IsHotlistSubmission(string recipient, string subject, string bodyText, string bodyHtml)
        {
            if (!string.IsNullOrWhiteSpace(recipient))
            {
                string r = recipient.ToLower();
                if (r.Contains("posthotlist") || r.Contains("hotlist@"))
                {
                    return true;
                }
            }

            return IsHotlistContent(subject, bodyText, bodyHtml);
        }

        private async Task<long> PublishJobInternal(EmailJobPostingQueue queueItem, ParsedJobDataDto parsed, User user, UserSubscriptionPlan? plan)
        {
            var now = DateTime.UtcNow;

            // Ensure ConsultancyUser exists for user
            var consultancyUser = await _context.ConsultancyUsers.FirstOrDefaultAsync(cu => cu.UserId == user.Id);
            if (consultancyUser == null)
            {
                consultancyUser = new ConsultancyUser
                {
                    UserId = user.Id,
                    Active = true,
                    Updated = now,
                    UpdatedBy = user.Id
                };
                _context.ConsultancyUsers.Add(consultancyUser);
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrWhiteSpace(parsed.Description) || parsed.Description.Trim().Length < 15)
            {
                throw new InvalidOperationException("Cannot publish job opening: Description is empty. ChatHire will not synthesize job content.");
            }

            string cleanName = JobParserService.CleanJobTitle(parsed.Name ?? "Job Opening");
            if (cleanName.Length > 50) cleanName = cleanName.Substring(0, 50).Trim();

            string cleanLocation = parsed.JobLocation ?? "Remote";
            if (cleanLocation.Length > 50) cleanLocation = cleanLocation.Substring(0, 50).Trim();

            string cleanCountry = parsed.Country ?? "US";
            if (cleanCountry.Length > 50) cleanCountry = cleanCountry.Substring(0, 50).Trim();

            string? cleanPostal = parsed.Postalcode;
            if (cleanPostal != null && cleanPostal.Length > 20) cleanPostal = cleanPostal.Substring(0, 20).Trim();

            var jobOpening = new JobOpening
            {
                Name = cleanName,
                Description = parsed.Description,
                ConsultancyUserId = consultancyUser.Id,
                JobLocation = cleanLocation,
                Postalcode = cleanPostal,
                Country = cleanCountry,
                TotalExp = parsed.TotalExp ?? 5,
                FromAmt = parsed.FromAmt,
                ToAmt = parsed.ToAmt,
                NumberOfOpening = parsed.NumberOfOpening ?? 1,
                PostedDate = now,
                LastDate = now.AddDays(30),
                Active = true,
                Updated = now,
                UpdatedBy = user.Id
            };

            _context.JobOpenings.Add(jobOpening);
            await _context.SaveChangesAsync(); // generate jobOpening.Id

            // 1. Add Skills (Extract and persist 5 top skills)
            var topSkills = (parsed.Skills ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();

            if (topSkills.Count > 0)
            {
                var allDbSkills = await _context.Skills.ToListAsync();
                foreach (var skillName in topSkills)
                {
                    var matchedSkill = allDbSkills.FirstOrDefault(s => s.Name != null && s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
                    int skillId;
                    if (matchedSkill != null)
                    {
                        skillId = (int)matchedSkill.Id;
                    }
                    else
                    {
                        var newSkill = new Skill { Name = skillName, Active = true, Updated = now, UpdatedBy = user.Id };
                        _context.Skills.Add(newSkill);
                        await _context.SaveChangesAsync();
                        allDbSkills.Add(newSkill);
                        skillId = (int)newSkill.Id;
                    }

                    _context.JobOpeningSkills.Add(new JobOpeningSkill
                    {
                        JobId = jobOpening.Id,
                        SkillId = skillId,
                        Active = true,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }
            }

            // 2. Add Visas (If no visa is mentioned in email, list all visa types)
            var allDbVisas = await _context.Visas.AsNoTracking().ToListAsync();
            var matchedVisaIds = new HashSet<short>();

            if (parsed.Visas != null && parsed.Visas.Count > 0)
            {
                foreach (var visaStr in parsed.Visas)
                {
                    if (string.IsNullOrWhiteSpace(visaStr)) continue;
                    string v = visaStr.Trim().ToLower();

                    if (v.Contains("usc") || v.Contains("us citizen") || v.Contains("citizen"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("USC", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("citizen")));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("green card") || v.Equals("gc"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("GC", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("green card")));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("gcead") || v.Contains("gc ead") || v.Contains("gc-ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("gcead"));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("h1b") || v.Contains("h-1b"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("H1B", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("h1b")));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("opt") || v.Contains("cpt") || v.Contains("ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.ToLower().Contains("opt") || x.Name.ToLower().Contains("ead")));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("tn"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.Equals("TN", StringComparison.OrdinalIgnoreCase));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("h4") || v.Contains("h4ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("h4"));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else if (v.Contains("l2") || v.Contains("l2ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("l2"));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                    else
                    {
                        var m = allDbVisas.FirstOrDefault(x => (x.Name != null && x.Name.ToLower() == v) || (x.Description != null && x.Description.ToLower().Contains(v)));
                        if (m != null) matchedVisaIds.Add(m.Id);
                    }
                }
            }

            // Fallback: If no visa is mentioned in email, map ALL visa types
            if (matchedVisaIds.Count == 0)
            {
                foreach (var dbVisa in allDbVisas)
                {
                    matchedVisaIds.Add(dbVisa.Id);
                }
            }

            foreach (var visaId in matchedVisaIds)
            {
                _context.JobOpeningVisaMaps.Add(new JobOpeningVisaMap
                {
                    JobOpeningId = jobOpening.Id,
                    VisaId = visaId,
                    Updated = now,
                    UpdatedBy = user.Id
                });
            }

            // 3. Add Employment Types (Work Arrangements: Remote, Onsite, Hybrid - default Hybrid if not given)
            var dbEmpTypes = await _context.EmploymentTypes.AsNoTracking().ToListAsync();
            var matchedEmpTypeIds = new HashSet<short>();

            var empTokens = new List<string>();
            if (parsed.EmploymentTypes != null) empTokens.AddRange(parsed.EmploymentTypes);
            if (parsed.IsRemote == true) empTokens.Add("Remote");
            if (!string.IsNullOrWhiteSpace(parsed.JobLocation)) empTokens.Add(parsed.JobLocation);

            foreach (var token in empTokens)
            {
                string t = token.ToLower();
                if (t.Contains("remote") || t.Contains("wfh") || t.Contains("work from home"))
                {
                    var m = dbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("remote"));
                    if (m != null) matchedEmpTypeIds.Add(m.Id);
                }
                else if (t.Contains("hybrid"))
                {
                    var m = dbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("hybrid"));
                    if (m != null) matchedEmpTypeIds.Add(m.Id);
                }
                else if (t.Contains("onsite") || t.Contains("on-site") || t.Contains("in office") || t.Contains("in person") || t.Contains("in-person"))
                {
                    var m = dbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("onsite"));
                    if (m != null) matchedEmpTypeIds.Add(m.Id);
                }
            }

            // Fallback: If no employment type (Remote/Onsite/Hybrid) is given, default to Hybrid
            if (matchedEmpTypeIds.Count == 0)
            {
                var hybrid = dbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("hybrid"))
                          ?? dbEmpTypes.FirstOrDefault(e => e.Id == 7);
                if (hybrid != null)
                {
                    matchedEmpTypeIds.Add(hybrid.Id);
                }
            }

            foreach (var empTypeId in matchedEmpTypeIds)
            {
                _context.JobOpeningEmploymentTypes.Add(new JobOpeningEmploymentType
                {
                    JobOpeningId = jobOpening.Id,
                    EmploymentTypeId = empTypeId,
                    Active = true,
                    Updated = now,
                    UpdatedBy = user.Id
                });
            }

            // 4. Add Job Types (C2C, W2-Contract, Full-Time - default C2C and W2-Contract if not given)
            var dbJobTypes = await _context.JobTypes.AsNoTracking().ToListAsync();
            var matchedJobTypeIds = new HashSet<short>();

            var jobTypeTokens = new List<string>();
            if (parsed.JobTypes != null) jobTypeTokens.AddRange(parsed.JobTypes);
            if (parsed.EmploymentTypes != null) jobTypeTokens.AddRange(parsed.EmploymentTypes);

            foreach (var token in jobTypeTokens)
            {
                string t = token.ToLower();
                if (t.Contains("c2c") || t.Contains("corp-to-corp") || t.Contains("corp 2 corp"))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("c2c"));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
                if (t.Contains("w2 - contract") || t.Contains("w2-contract") || t.Contains("w2 contract") || t.Contains("w-2 contract") || (t.Contains("w2") && !t.Contains("full")))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("w2 - contract"));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
                if (t.Contains("full time") || t.Contains("full-time") || t.Contains("fte") || t.Contains("permanent") || t.Contains("direct hire"))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && (j.Description.ToLower().Contains("full time") || j.Description.ToLower().Contains("full-time")));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
                if (t.Contains("1099"))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("1099"));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
                if (t.Contains("part-time") || t.Contains("part time"))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("part-time"));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
                if (t.Contains("internship"))
                {
                    var m = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("internship"));
                    if (m != null) matchedJobTypeIds.Add(m.Id);
                }
            }

            // Fallback: If no job type information is given, default to C2C AND W2-Contract
            if (matchedJobTypeIds.Count == 0)
            {
                var c2c = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("c2c"))
                       ?? dbJobTypes.FirstOrDefault(j => j.Id == 7);
                if (c2c != null) matchedJobTypeIds.Add(c2c.Id);

                var w2Contract = dbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("w2 - contract"))
                              ?? dbJobTypes.FirstOrDefault(j => j.Id == 8);
                if (w2Contract != null) matchedJobTypeIds.Add(w2Contract.Id);
            }

            foreach (var jobTypeId in matchedJobTypeIds)
            {
                _context.JobOpeningJobTypes.Add(new JobOpeningJobType
                {
                    JobOpeningId = jobOpening.Id,
                    JobTypeId = jobTypeId,
                    Active = true,
                    Updated = now,
                    UpdatedBy = user.Id
                });
            }

            // Add JobOpeningLocations (links CityId so location pin appears in search results and job cards)
            string rawLoc = parsed.JobLocation ?? cleanLocation;
            if (!string.IsNullOrWhiteSpace(rawLoc) && !rawLoc.Equals("Remote", StringComparison.OrdinalIgnoreCase))
            {
                string cityName = rawLoc;
                string? stateCode = null;

                var commaParts = rawLoc.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (commaParts.Length >= 2)
                {
                    cityName = commaParts[0].Trim();
                    var statePart = commaParts[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (statePart.Length > 0) stateCode = statePart[0].Trim();
                }

                City? matchedCity = null;
                if (!string.IsNullOrWhiteSpace(stateCode))
                {
                    matchedCity = await _context.Cities
                        .Include(c => c.IdStateNavigation)
                        .FirstOrDefaultAsync(c => c.City1.ToLower() == cityName.ToLower() && 
                            (c.IdStateNavigation.StateCode.ToLower() == stateCode.ToLower() || c.IdStateNavigation.StateName.ToLower() == stateCode.ToLower()));
                }

                if (matchedCity == null)
                {
                    matchedCity = await _context.Cities
                        .Include(c => c.IdStateNavigation)
                        .FirstOrDefaultAsync(c => c.City1.ToLower() == cityName.ToLower());
                }

                if (matchedCity != null)
                {
                    _context.JobOpeningLocations.Add(new JobOpeningLocation
                    {
                        JobOpeningId = jobOpening.Id,
                        CityId = matchedCity.Id,
                        NumberOfOpenings = jobOpening.NumberOfOpening ?? 1,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }
            }

            // Deduct / increment used job count
            if (plan != null)
            {
                plan.NoOfUsedJobPosting = (plan.NoOfUsedJobPosting ?? 0) + 1;
                plan.Updated = now;
                plan.UpdatedBy = user.Id;
            }

            await _context.SaveChangesAsync();
            return jobOpening.Id;
        }

        private async Task<List<long>> PublishHotlistInternal(EmailJobPostingQueue queueItem, ParsedHotlistDataDto parsedHotlist, User user)
        {
            var now = DateTime.UtcNow;
            var createdCandidateIds = new List<long>();

            // Ensure ConsultancyUser exists for user
            var consultancyUser = await _context.ConsultancyUsers.FirstOrDefaultAsync(cu => cu.UserId == user.Id);
            if (consultancyUser == null)
            {
                consultancyUser = new ConsultancyUser
                {
                    UserId = user.Id,
                    Active = true,
                    Updated = now,
                    UpdatedBy = user.Id
                };
                _context.ConsultancyUsers.Add(consultancyUser);
                await _context.SaveChangesAsync();
            }

            var allDbSkills = await _context.Skills.ToListAsync();
            var allDbVisas = await _context.Visas.AsNoTracking().ToListAsync();
            var allDbEmpTypes = await _context.EmploymentTypes.AsNoTracking().ToListAsync();
            var allDbJobTypes = await _context.JobTypes.AsNoTracking().ToListAsync();
            var defaultProfileStatus = await _context.ProfileStatuses.FirstOrDefaultAsync(ps => ps.Description.ToLower().Contains("active") || ps.Description.ToLower().Contains("available"))
                                    ?? await _context.ProfileStatuses.FirstOrDefaultAsync();
            short profileStatusId = defaultProfileStatus != null ? defaultProfileStatus.Id : (short)1;

            foreach (var candDto in parsedHotlist.Candidates)
            {
                string candName = !string.IsNullOrWhiteSpace(candDto.Name) ? candDto.Name.Trim() : "Consultant";
                if (candName.Length > 25) candName = candName.Substring(0, 25).Trim();

                string candTitle = !string.IsNullOrWhiteSpace(candDto.Title) ? candDto.Title.Trim() : "Software Consultant";
                if (candTitle.Length > 50) candTitle = candTitle.Substring(0, 50).Trim();

                string? candComment = candDto.Comment;
                if (candComment != null && candComment.Length > 500) candComment = candComment.Substring(0, 500).Trim();

                string? candEmail = candDto.Email ?? user.Email;
                if (candEmail != null && candEmail.Length > 100) candEmail = candEmail.Substring(0, 100).Trim();

                string? candPhone = candDto.Phone ?? user.Phone;
                if (candPhone != null && candPhone.Length > 25) candPhone = candPhone.Substring(0, 25).Trim();

                // Match City / State
                int? matchedCityId = null;
                if (!string.IsNullOrWhiteSpace(candDto.Location) && !candDto.Location.Equals("Remote", StringComparison.OrdinalIgnoreCase))
                {
                    string rawLoc = candDto.Location.Trim();
                    string cityName = rawLoc;
                    string? stateCode = null;

                    var commaParts = rawLoc.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (commaParts.Length >= 2)
                    {
                        cityName = commaParts[0].Trim();
                        var statePart = commaParts[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (statePart.Length > 0) stateCode = statePart[0].Trim();
                    }

                    City? cityObj = null;
                    if (!string.IsNullOrWhiteSpace(stateCode))
                    {
                        cityObj = await _context.Cities
                            .Include(c => c.IdStateNavigation)
                            .FirstOrDefaultAsync(c => c.City1.ToLower() == cityName.ToLower() && 
                                (c.IdStateNavigation.StateCode.ToLower() == stateCode.ToLower() || c.IdStateNavigation.StateName.ToLower() == stateCode.ToLower()));
                    }

                    if (cityObj == null)
                    {
                        cityObj = await _context.Cities
                            .Include(c => c.IdStateNavigation)
                            .FirstOrDefaultAsync(c => c.City1.ToLower() == cityName.ToLower());
                    }

                    if (cityObj != null)
                    {
                        matchedCityId = cityObj.Id;
                    }
                }

                // Match Visa
                short? matchedVisaId = null;
                if (!string.IsNullOrWhiteSpace(candDto.Visa))
                {
                    string v = candDto.Visa.Trim().ToLower();
                    if (v.Contains("usc") || v.Contains("us citizen") || v.Contains("citizen"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("USC", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("citizen")));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("green card") || v.Equals("gc"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("GC", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("green card")));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("gcead") || v.Contains("gc ead") || v.Contains("gc-ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("gcead"));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("h1b") || v.Contains("h-1b"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.Equals("H1B", StringComparison.OrdinalIgnoreCase) || x.Name.ToLower().Contains("h1b")));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("opt") || v.Contains("cpt") || v.Contains("ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && (x.Name.ToLower().Contains("opt") || x.Name.ToLower().Contains("ead")));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("tn"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.Equals("TN", StringComparison.OrdinalIgnoreCase));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("h4") || v.Contains("h4ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("h4"));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else if (v.Contains("l2") || v.Contains("l2ead"))
                    {
                        var m = allDbVisas.FirstOrDefault(x => x.Name != null && x.Name.ToLower().Contains("l2"));
                        if (m != null) matchedVisaId = m.Id;
                    }
                    else
                    {
                        var m = allDbVisas.FirstOrDefault(x => (x.Name != null && x.Name.ToLower() == v) || (x.Description != null && x.Description.ToLower().Contains(v)));
                        if (m != null) matchedVisaId = m.Id;
                    }
                }

                if (!matchedVisaId.HasValue && !string.IsNullOrWhiteSpace(candDto.Visa))
                {
                    var fallback = allDbVisas.FirstOrDefault(x => x.Name == "H1B");
                    if (fallback != null) matchedVisaId = fallback.Id;
                }

                bool isRemote = candDto.RemoteOnly ?? (candDto.Location?.ToLower().Contains("remote") == true);
                bool canRelocate = candDto.CanRelocate ?? (candDto.Location?.ToLower().Contains("relocat") == true || candDto.Location?.ToLower().Contains("open") == true);

                var profile = new CandidateProfile
                {
                    UserId = user.Id,
                    ConsultancyUserId = consultancyUser.Id,
                    CandidateName = candName,
                    Title = candTitle,
                    TotalExp = candDto.TotalExp ?? 5,
                    VisaId = matchedVisaId,
                    CityId = matchedCityId,
                    RemoteOnly = isRemote,
                    CanRelocate = canRelocate,
                    AnyLocation = candDto.AnyLocation ?? (!matchedCityId.HasValue && !isRemote),
                    Comment = candComment,
                    FromAmt = candDto.FromAmt,
                    ToAmt = candDto.ToAmt ?? candDto.FromAmt,
                    Email = candEmail,
                    Phone = candPhone,
                    Active = true,
                    StatusId = profileStatusId,
                    PostedDate = now,
                    Updated = now,
                    UpdatedBy = user.Id
                };

                _context.CandidateProfiles.Add(profile);
                await _context.SaveChangesAsync(); // Generates profile.Id

                candDto.CreatedCandidateProfileId = profile.Id;
                createdCandidateIds.Add(profile.Id);

                // Add Skills (Top 5)
                var topSkills = (candDto.Skills ?? new List<string>())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Select(s => s.Length > 50 ? s.Substring(0, 50).Trim() : s)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(5)
                    .ToList();

                foreach (var skillName in topSkills)
                {
                    var matchedSkill = allDbSkills.FirstOrDefault(s => s.Name != null && s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
                    int skillId;
                    if (matchedSkill != null)
                    {
                        skillId = (int)matchedSkill.Id;
                    }
                    else
                    {
                        var newSkill = new Skill { Name = skillName, Active = true, Updated = now, UpdatedBy = user.Id };
                        _context.Skills.Add(newSkill);
                        await _context.SaveChangesAsync();
                        allDbSkills.Add(newSkill);
                        skillId = (int)newSkill.Id;
                    }

                    _context.CandidateProfileSkills.Add(new CandidateProfileSkill
                    {
                        CandidateProfileid = profile.Id,
                        SkillId = skillId,
                        Active = true,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }

                // Add Employment Types (Remote=3, Onsite=1, Hybrid=7)
                var empTypeIds = new HashSet<short>();
                if (isRemote)
                {
                    var remote = allDbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("remote"));
                    if (remote != null) empTypeIds.Add(remote.Id);
                }
                if (canRelocate || matchedCityId.HasValue)
                {
                    var onsite = allDbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("onsite"));
                    if (onsite != null) empTypeIds.Add(onsite.Id);
                    var hybrid = allDbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("hybrid"));
                    if (hybrid != null) empTypeIds.Add(hybrid.Id);
                }
                if (empTypeIds.Count == 0)
                {
                    var remote = allDbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("remote"));
                    if (remote != null) empTypeIds.Add(remote.Id);
                    var hybrid = allDbEmpTypes.FirstOrDefault(e => e.Name != null && e.Name.ToLower().Contains("hybrid"));
                    if (hybrid != null) empTypeIds.Add(hybrid.Id);
                }

                foreach (var etId in empTypeIds)
                {
                    _context.CandidateProfileEmploymentTypes.Add(new CandidateProfileEmploymentType
                    {
                        CandidateProfileId = profile.Id,
                        EmploymentTypeId = etId,
                        Active = true,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }

                // Add Pref Job Types (C2C=7, W2-Contract=8)
                var jobTypeIds = new HashSet<short>();
                var c2c = allDbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("c2c")) ?? allDbJobTypes.FirstOrDefault(j => j.Id == 7);
                if (c2c != null) jobTypeIds.Add(c2c.Id);

                var w2Contract = allDbJobTypes.FirstOrDefault(j => j.Description != null && j.Description.ToLower().Contains("w2 - contract")) ?? allDbJobTypes.FirstOrDefault(j => j.Id == 8);
                if (w2Contract != null) jobTypeIds.Add(w2Contract.Id);

                foreach (var jtId in jobTypeIds)
                {
                    _context.CandidatePrefJobTypes.Add(new CandidatePrefJobType
                    {
                        CandidateProfileId = profile.Id,
                        JobTypeId = jtId,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }

                // Add CandidatePrefLocation
                if (matchedCityId.HasValue)
                {
                    _context.CandidatePrefLocations.Add(new CandidatePrefLocation
                    {
                        CandidateId = profile.Id,
                        CityId = matchedCityId.Value,
                        Updated = now,
                        UpdatedBy = user.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return createdCandidateIds;
        }

        private EmailJobPostingQueueDto MapToDto(EmailJobPostingQueue item)
        {
            ParsedJobDataDto? parsedJob = null;
            ParsedHotlistDataDto? parsedHotlist = null;

            if (!string.IsNullOrWhiteSpace(item.ParsedJobJson))
            {
                try
                {
                    if (item.Status == "HotlistPublished" || item.ParsedJobJson.Contains("\"Candidates\""))
                    {
                        parsedHotlist = JsonSerializer.Deserialize<ParsedHotlistDataDto>(item.ParsedJobJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    else
                    {
                        parsedJob = JsonSerializer.Deserialize<ParsedJobDataDto>(item.ParsedJobJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
                catch { }
            }

            return new EmailJobPostingQueueDto
            {
                Id = item.Id,
                SenderEmail = item.SenderEmail,
                SenderName = item.SenderName,
                EmailSubject = item.EmailSubject,
                RawEmailBodyText = item.RawEmailBodyText,
                RawEmailBodyHtml = item.RawEmailBodyHtml,
                UserId = item.UserId,
                ConsultancyId = item.ConsultancyId,
                MatchedEmailType = item.MatchedEmailType,
                Status = item.Status,
                ParsedJobJson = item.ParsedJobJson,
                ParsedJob = parsedJob,
                ParsedHotlist = parsedHotlist,
                CreatedJobOpeningId = item.CreatedJobOpeningId,
                ErrorMessage = item.ErrorMessage,
                RetryCount = item.RetryCount,
                ReceivedDate = item.ReceivedDate,
                ProcessedDate = item.ProcessedDate,
                UpdatedBy = item.UpdatedBy
            };
        }

        private string ExtractPureEmail(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var match = Regex.Match(raw, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value.Trim().ToLower() : raw.Trim().ToLower();
        }

        private string ExtractSenderDisplayName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var match = Regex.Match(raw, @"^([^<]+)<");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim().Trim('\"', '\'');
            }
            return raw.Split('@')[0].Trim();
        }

        private async Task<(string? text, string? html, string? subject, string? from)> FetchEmailFromResend(string emailId)
        {
            var apiKey = _configuration?["Resend:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey)) return (null, null, null, null);

            using var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var resp = await client.GetAsync($"https://api.resend.com/emails/receiving/{emailId}");
            if (!resp.IsSuccessStatusCode)
            {
                resp = await client.GetAsync($"https://api.resend.com/emails/{emailId}");
            }

            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string? text = root.TryGetProperty("text", out var t) ? t.GetString() : null;
                string? html = root.TryGetProperty("html", out var h) ? h.GetString() : null;
                string? subject = root.TryGetProperty("subject", out var s) ? s.GetString() : null;
                string? from = root.TryGetProperty("from", out var f) ? f.GetString() : null;

                // If text and html are null, Resend inbound email delivers raw RFC 822 / MIME file in raw.download_url
                if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(html))
                {
                    if (root.TryGetProperty("raw", out var rawElem) && rawElem.TryGetProperty("download_url", out var dlElem))
                    {
                        string? downloadUrl = dlElem.GetString();
                        if (!string.IsNullOrWhiteSpace(downloadUrl))
                        {
                            try
                            {
                                using var rawResp = await client.GetAsync(downloadUrl);
                                if (rawResp.IsSuccessStatusCode)
                                {
                                    using var stream = await rawResp.Content.ReadAsStreamAsync();
                                    var message = await MimeKit.MimeMessage.LoadAsync(stream);
                                    text = message.TextBody;
                                    html = message.HtmlBody;
                                    if (string.IsNullOrWhiteSpace(subject) && !string.IsNullOrWhiteSpace(message.Subject))
                                    {
                                        subject = message.Subject;
                                    }
                                    if (string.IsNullOrWhiteSpace(from) && message.From != null && message.From.Count > 0)
                                    {
                                        from = message.From.ToString();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to download and parse raw MIME email for email_id {EmailId}", emailId);
                            }
                        }
                    }
                }

                return (text, html, subject, from);
            }
            return (null, null, null, null);
        }
    }
}
