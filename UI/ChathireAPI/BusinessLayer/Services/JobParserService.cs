using BusinessEntityAndDTO.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public interface IJobParserService
    {
        Task<ParsedJobDataDto> ParseJobEmailAsync(string subject, string bodyText, string? bodyHtml = null);
    }

    public class JobParserService : IJobParserService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JobParserService> _logger;
        private readonly HttpClient _httpClient;

        public JobParserService(IConfiguration config, ILogger<JobParserService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ParsedJobDataDto> ParseJobEmailAsync(string subject, string bodyText, string? bodyHtml = null)
        {
            string rawContent = !string.IsNullOrWhiteSpace(bodyText) ? bodyText : (bodyHtml ?? string.Empty);
            if (rawContent.Contains("<") && rawContent.Contains(">"))
            {
                rawContent = StripHtml(rawContent);
            }
            
            // Clean forward headers and metadata lines first
            string contentToParse = CleanForwardHeadersAndMetadata(rawContent);

            if (string.IsNullOrWhiteSpace(contentToParse) || contentToParse.Trim().Length < 15)
            {
                throw new InvalidOperationException("Email body contains no job description or content. Job postings will NOT be created automatically when email content is missing.");
            }

            // Try Gemini Flash if API key configured
            string geminiKey = _config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(geminiKey))
            {
                try
                {
                    var geminiResult = await ParseWithGeminiAsync(subject, contentToParse, geminiKey);
                    if (geminiResult != null && !string.IsNullOrWhiteSpace(geminiResult.Name) && !string.IsNullOrWhiteSpace(geminiResult.Description))
                    {
                        // Clean title without letting raw body override Gemini's parsed title
                        geminiResult.Name = CleanJobTitle(geminiResult.Name, null);
                        geminiResult.Description = PostProcessHtmlDescription(geminiResult.Description);
                        _logger.LogInformation("Gemini parsed Name='{Name}', JobLocation='{JobLocation}'", geminiResult.Name, geminiResult.JobLocation);
                        if (string.IsNullOrWhiteSpace(geminiResult.JobLocation) || geminiResult.JobLocation.Equals("null", StringComparison.OrdinalIgnoreCase) || geminiResult.JobLocation.Equals("Not Specified", StringComparison.OrdinalIgnoreCase))
                        {
                            var loc = ExtractLocation(subject, rawContent, out bool isRemote) ?? ExtractLocation(subject, contentToParse, out isRemote);
                            _logger.LogInformation("Deterministic ExtractLocation fallback returned '{Location}', isRemote={IsRemote}", loc, isRemote);
                            geminiResult.JobLocation = loc;
                            if (geminiResult.IsRemote != true && isRemote) geminiResult.IsRemote = true;
                        }
                        return geminiResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini parsing failed or not configured, using deterministic email text extraction.");
                }
            }

            // Strictly extracts ONLY what is physically written in the email
            var ruleResult = ParseWithRuleBasedEngine(subject, contentToParse);
            ruleResult.Description = PostProcessHtmlDescription(ruleResult.Description);
            return ruleResult;
        }

        private async Task<ParsedJobDataDto?> ParseWithGeminiAsync(string subject, string body, string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            string prompt = $@"You are an expert HR job description parser and content sanitizer. The recruiter sent the following job posting email.
CRITICAL MANDATES:
1. ALL job content in the email (Overview, Role Description, Skills, Responsibilities, Qualifications, Experience, Location, Requirements, Nice to have, etc.) is highly relevant.
2. PRESERVE 100% OF THE JOB CONTENT EXACTLY AS WRITTEN. DO NOT summarize, shorten, abbreviate, or omit ANY numbers, year ranges (e.g., '8-12+ years', '3-5+ years', '12+ Months'), or bullet points.
3. Clean out:
   - Recruiter email intros and preamble chatter ('Hello Professionals', 'This is [Name] from [Company] working as an IT Recruiter...', 'Please find attached job description below...', 'if you are interested please forward your resume to...', 'We have an immediate opportunity with one of our clients', etc.)
   - Top mailing list / unsubscribe banners ('Remove/unsubscribe | Update your contact...')
   - Top sender envelope headers ('From: manish...', 'Reply to: ...')
   - Recruiter phone numbers ('V: 832-481-0435', 'P: ...', etc.) and email addresses ('E: ...')
   - Bottom recruiting portal / marketing ads ('Sign-Up for your account with PROHIRES...', 'Hire our IT Recruiter...')
4. FORMATTING & WEIRD CHARACTER CLEANING MANDATE:
   - Clean up broken Unicode characters, question marks ('?', '??', '???', 'â€¢') that replaced bullets, icons, or emojis into clean standard HTML bullet lists (<ul><li>...</li></ul>) or structured tags. NEVER output raw '?' or '??' as bullets or field dividers.
   - Separate metadata fields (e.g. 'Role: Power Platform Architect', 'Location: Indianapolis, IN', 'Duration: 12+ Months') into distinct clean HTML paragraphs:
     <p><strong>Role:</strong> Power Platform Architect</p>
     <p><strong>Location:</strong> Indianapolis, IN / San Fransico , CA -onsite role -5 Days a Week</p>
     <p><strong>Duration:</strong> 12+ Months</p>
   - Format section headers with <h4><strong>Section Header</strong></h4> (e.g., '<h4><strong>Mandatory Skills:</strong></h4>', '<h4><strong>Experience</strong></h4>', '<h4><strong>Skills</strong></h4>', '<h4><strong>Nice to Have</strong></h4>', '<h4><strong>Technical/Functional Skills</strong></h4>', '<h4><strong>Roles & Responsibilities</strong></h4>').
   - Format bullet lists with <ul><li>...</li></ul>. Make sure all numbers and text (like '8-12+ years in enterprise application/platform architecture.') are 100% preserved.
   - DO NOT include an <h3> title heading at the top of the description HTML since the title is already rendered in the UI header card.
5. JOB TITLE MANDATE:
   - Extract ONLY the core job role / title into `name` (e.g., 'Power Platform Architect', 'Project Manager Data Engineer', 'Senior Java Developer').
   - MUST REMOVE ALL City, State, Country, or Location mentions (e.g. 'Stamford, CT', 'Stamford Ct', 'Plano, TX', 'Austin TX', 'Remote', 'Jersey City NJ', etc.).
   - MUST REMOVE ALL hyphens, dashes ('-', '–', '—'), pipes ('|'), slashes ('/'), asterisks, parentheses, quotes, and unwanted punctuation from the job title. DO NOT leave hyphens or location in `name`.
   - MUST format in clean Title/Camel Case with tech acronyms (AWS, GCP, AZURE, SAP, IBM, SQL, QA, CRM, ERP, D365, .NET, UI, UX, ETL, AI, ML, etc.) in UPPERCASE.
6. Extract location (City, State or 'Remote'/'Hybrid') into jobLocation.
7. SKILLS MANDATE: Extract EXACTLY 5 of the top, most relevant and specific technical and functional skills/technologies required for this job role into `skills` (e.g. ['Power Apps', 'Power Automate', 'Power Platform Governance', 'Azure AD', 'ALM']). You MUST ALWAYS find 5 skills from the job description.
8. WORK ARRANGEMENT / EMPLOYMENT TYPE: Extract work arrangement into `employmentTypes`: 'Remote', 'Onsite', or 'Hybrid' if mentioned. If not mentioned, leave empty.
9. JOB TYPE: Extract contract/tax term into `jobTypes`: 'C2C', 'W2-Contract', 'Full Time', '1099' if mentioned. If not mentioned, leave empty.
10. VISAS: Extract any explicitly mentioned work authorizations / visas (e.g. 'USC', 'GC', 'H1B', 'OPT', 'EAD', 'TN', 'ANY') into `visas`. If no specific visa is mentioned in the email, leave `visas` empty.

Email Subject: {subject}
Email Content:
{body}

Respond ONLY with valid JSON matching this schema:
{{
  ""name"": ""Clean Job Title without hyphens or location"",
  ""description"": ""Full complete HTML description containing ALL the text from the email"",
  ""jobLocation"": ""City, State or 'Remote'"",
  ""isRemote"": false,
  ""country"": ""US"",
  ""postalcode"": null,
  ""totalExp"": null,
  ""fromAmt"": null,
  ""toAmt"": null,
  ""numberOfOpening"": 1,
  ""skills"": [""Skill1"", ""Skill2"", ""Skill3"", ""Skill4"", ""Skill5""],
  ""employmentTypes"": [],
  ""jobTypes"": [],
  ""visas"": []
}}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json",
                    temperature = 0.0
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API call returned status {StatusCode}", response.StatusCode);
                return null;
            }

            string respString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respString);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return null;

            string jsonText = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
            var parsed = JsonSerializer.Deserialize<ParsedJobDataDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return parsed;
        }

        public ParsedJobDataDto ParseWithRuleBasedEngine(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(body) || body.Trim().Length < 15)
            {
                throw new InvalidOperationException("Email body contains no job description or content.");
            }

            var result = new ParsedJobDataDto();

            // 1. Clean Job Title (checks Position: / Role: or strips Fwd/Re, location, country, state, rate suffixes)
            result.Name = CleanJobTitle(subject, body);

            // 2. Extract Location & Remote intelligently
            result.JobLocation = ExtractLocation(subject, body, out bool isRemote);
            result.IsRemote = isRemote;

            // 3. Extract Experience (supports "Experience Required: 6-8", "6+ years", etc.)
            var expMatch = Regex.Match(body, @"(?im)^\s*(?:experience\s*(?:required)?|total\s*experience|exp)\s*[:\-]\s*(\d+)(?:\s*[-–to+]\s*(\d+))?", RegexOptions.IgnoreCase);
            if (!expMatch.Success)
            {
                expMatch = Regex.Match(body, @"(?:at\s+least|minimum|min)?\s*(\d+)\s*(?:\+|-\s*\d+)?\s*(?:years?|yrs?)(?:\s+of)?\s+(?:hands[- ]on\s+)?(?:experience|exp)\b", RegexOptions.IgnoreCase);
            }
            if (!expMatch.Success)
            {
                expMatch = Regex.Match(body, @"\b(?:experience|exp)\s*[:\-]\s*(\d+)\s*(?:\+|-\s*\d+)?\s*(?:years?|yrs?)", RegexOptions.IgnoreCase);
            }
            if (expMatch.Success && int.TryParse(expMatch.Groups[1].Value, out int exp))
            {
                result.TotalExp = exp;
            }

            // 4. Extract Rate / Salary (ONLY if mentioned in the body)
            var rateMatch = Regex.Match(body, @"(?:\$|USD\s*)(\d+)(?:\s*(?:-|to)\s*\$?(\d+))?\s*(?:\/|\s*per\s*)?(?:hr|hour|c2c|w2|annual|k|year)?", RegexOptions.IgnoreCase);
            if (rateMatch.Success)
            {
                if (short.TryParse(rateMatch.Groups[1].Value, out short fromAmt))
                {
                    result.FromAmt = fromAmt;
                }
                if (rateMatch.Groups[2].Success && short.TryParse(rateMatch.Groups[2].Value, out short toAmt))
                {
                    result.ToAmt = toAmt;
                }
            }

            // 5. Extract Skills (find top 5 matching skills from email text)
            var skillKeywords = new[]
            {
                ".NET", "Dotnet", ".NET Core", "C#", "Production Support", "Application Development",
                "Dynamics CRM", "Microsoft Dynamics", "Dynamics 365", "D365", "M365", "Power Platform", "Power Automate",
                "Power Apps", "Dataverse", "Power BI", "Copilot", "Copilot Studio", "Azure AI", "ALM", "ABAP", "APIs",
                "Mainframe", "COBOL", "DB2", "JCL", "CICS", "VSAM", "Endevor", "IMS", "PL/I", "Assembler",
                "Java", "Spring Boot", "Spring", "Microservices", "Python", "React", "Angular", "Vue", "Node.js",
                "ASP.NET", "AWS", "Azure", "GCP", "Google Cloud", "Kubernetes", "Docker", "SQL", "PostgreSQL", "MongoDB", "Kafka",
                "Spark", "Hadoop", "Snowflake", "Databricks", "DevOps", "CI/CD", "Terraform", "Selenium", "QA", "Automation",
                "Data Engineer", "Machine Learning", "AI", "Salesforce", "ServiceNow", "SAP", "Golang", "Rust", "Swift", "Kotlin",
                "TypeScript", "JavaScript", "HTML", "CSS", "GraphQL", "Redis", "Elasticsearch", "Oracle", "Linux"
            };

            var extractedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kw in skillKeywords)
            {
                string pat = $@"(?:^|[\s,;:(\[/•*\-])" + Regex.Escape(kw) + @"(?:$|[\s,;:)\]/!?*•\-])";
                if (Regex.IsMatch(body, pat, RegexOptions.IgnoreCase) || Regex.IsMatch(subject, pat, RegexOptions.IgnoreCase))
                {
                    extractedSkills.Add(kw.Equals("Dotnet", StringComparison.OrdinalIgnoreCase) ? ".NET" : kw);
                    if (extractedSkills.Count >= 5) break;
                }
            }
            result.Skills = extractedSkills.Take(5).ToList();

            // 6. Extract Employment Types (Work Arrangements: Remote, Onsite, Hybrid)
            var empTypes = new List<string>();
            if (Regex.IsMatch(body, @"\b(hybrid|in\s*person\s*interview|hybrid\s*mode)\b", RegexOptions.IgnoreCase)) empTypes.Add("Hybrid");
            else if (Regex.IsMatch(body, @"\b(remote|work\s*from\s*home|wfh)\b", RegexOptions.IgnoreCase)) empTypes.Add("Remote");
            else if (Regex.IsMatch(body, @"\b(onsite|on-site|in\s*office)\b", RegexOptions.IgnoreCase)) empTypes.Add("Onsite");
            result.EmploymentTypes = empTypes;

            // 7. Extract Job Types (C2C, W2-Contract, Full Time, 1099)
            var jobTypes = new List<string>();
            if (Regex.IsMatch(body, @"\b(c2c|corp-to-corp|corp\s*2\s*corp)\b", RegexOptions.IgnoreCase)) jobTypes.Add("C2C");
            if (Regex.IsMatch(body, @"\b(w2|w-2)\b", RegexOptions.IgnoreCase)) jobTypes.Add("W2-Contract");
            if (Regex.IsMatch(body, @"\b(full\s*time|fte|permanent|direct\s*hire)\b", RegexOptions.IgnoreCase)) jobTypes.Add("Full Time");
            if (Regex.IsMatch(body, @"\b(1099)\b", RegexOptions.IgnoreCase)) jobTypes.Add("1099");
            result.JobTypes = jobTypes;

            // 8. Extract Visas (ONLY if explicitly mentioned)
            var visas = new List<string>();
            if (Regex.IsMatch(body, @"\b(us\s*citizen|usc)\b", RegexOptions.IgnoreCase)) visas.Add("US Citizen");
            if (Regex.IsMatch(body, @"\b(green\s*card|gc)\b", RegexOptions.IgnoreCase)) visas.Add("Green Card");
            if (Regex.IsMatch(body, @"\b(h1b|h-1b)\b", RegexOptions.IgnoreCase)) visas.Add("H1B");
            if (Regex.IsMatch(body, @"\b(opt|cpt|ead)\b", RegexOptions.IgnoreCase)) visas.Add("OPT/EAD");
            if (Regex.IsMatch(body, @"\b(tn\s*visa|tn\s*status)\b", RegexOptions.IgnoreCase)) visas.Add("TN");
            result.Visas = visas;

            // 9. Format Description directly from the recruiter's email text - preserving 100% of the content
            result.Description = CleanDescriptionToHtml(body, result.Name);

            if (string.IsNullOrWhiteSpace(result.Description))
            {
                throw new InvalidOperationException("Could not extract a valid job description from the provided email body.");
            }

            return result;
        }

        private string CleanForwardHeadersAndMetadata(string rawBody)
        {
            if (string.IsNullOrWhiteSpace(rawBody)) return string.Empty;

            string cleaned = rawBody;

            // Remove email forward blocks
            cleaned = Regex.Replace(cleaned, @"(?m)^\s*-+\s*Forwarded message\s*-+\s*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"(?m)^\s*Begin forwarded message:?\s*$", "", RegexOptions.IgnoreCase);
            
            // Remove top unsubscribe / mailing list banners & URL links
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?(?:unsubscri|mailing\s*list|prohirespowerhouse|fe_web_member|broadcast\s+requirements|update\s+your\s+contact).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^\s*[|•*–—\-\s]*(?:Update|Subscribe|Remove|Unsubscribe)\b.*$", "");

            // Remove standard email header lines at the top of forwarded emails (e.g. "From: ...", "To: ...", "Date: ...", "Subject: ...", "Reply-To: ...")
            cleaned = Regex.Replace(cleaned, @"(?im)^[ \t]*(?:From|Reply\s*to|Reply-To|Sent|Date|Subject|To|Cc|Bcc)\s*[*:]*[:\s][^\r\n]*", "");
            
            // Remove recruiter greetings and introductions/preamble chatter
            cleaned = Regex.Replace(cleaned, @"(?im)^[ \t]*(?:hi|hello|dear|greetings|good\s+morning|good\s+day)\s+[^\r\n]*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:this\s+is\s+[A-Za-z\s]+from\s+[A-Za-z0-9\s]+(?:,\s*)?(?:working\s+as|as\s+an?\s+IT\s+Recruiter|an?\s+IT\s+Recruiter|Talent\s+Acquisition)).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:please\s+find\s+(?:the\s+)?(?:attached\s+)?job\s+description\s+(?:below|herewith|attached)).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:if\s+you\s+are\s+interested\s*,?\s*please\s+(?:forward|send|share|reply\s+with)\s+your\s+(?:updated\s+)?resume).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:share\s+your\s+(?:updated\s+)?resume\s+(?:at|to)\s+[\w.\-+]+@[\w.\-]+\.[a-z]{2,}).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:hope\s+you\s+are\s+doing\s+well|hope\s+this\s+email\s+finds\s+you\s+well).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?\b(?:let\s+me\s+know\s+if\s+you\s+are\s+interested|please\s+let\s+me\s+know\s+your\s+interest|kindly\s+let\s+me\s+know).*$", "");

            // Remove standalone email lines from top or signature
            cleaned = Regex.Replace(cleaned, @"(?im)^[ \t]*(?:E|Email|Mail)\s*[*:]*[ \t]*[\w\.\-+]+@[\w\.\-]+\.[a-z]{2,}[ \t]*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^[ \t]*[\w\.\-+]+@[\w\.\-]+\.[a-z]{2,}[ \t]*$", "");

            // Remove phone number lines (e.g. "V: 832-481-0435", "P: ...", "Phone: ...")
            cleaned = Regex.Replace(cleaned, @"(?im)^[ \t]*(?:V|P|T|Cell|Phone|Direct|M|Mobile|Tel|Fax|Contact|Voice)\s*[*:]*[ \t]*[\d\(\)\- \t\.\+xext]+$", "");

            // Remove recruiting portal marketing footers / broadcast promotions / subscription notices
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?(?:Sign[- ]Up\s+for\s+your\s+account|PROHIRES|POWERHOUSE|Recruiting\s+Portal|broadcast\s+requirements|Hire\s+our\s+IT\s+Recruiter|Hire\s+(?:an?|our)\s+recruiter|at\s+just\s+\$?\d+\/month|\$499\/month|Hotlist|Powered\s+by|Disclaimer\s*:|Confidentiality\s*Notice|To\s*unsubscribe|Click\s+here\s+to\s+unsubscribe|Update\s+your\s+contact|Remove\s*\/?\s*unsubscribe|Subscribe\s+to\s+mailing\s+list).*$", "");

            return cleaned.Trim();
        }

        private string PostProcessHtmlDescription(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            string cleaned = html;

            // 1. Remove promo / marketing footer blocks and unsubscribe elements inside HTML tags
            cleaned = Regex.Replace(cleaned, @"(?is)<(?:p|div|li|h\d|span|a)[^>]*>[^<]*(?:Sign[- ]Up\s+for\s+your\s+account|PROHIRES|POWERHOUSE|Recruiting\s+Portal|broadcast\s+requirements|Hire\s+our\s+IT\s+Recruiter|Hire\s+(?:an?|our)\s+recruiter|\$499\/month|unsubscri|mailing\s*list|fe_web_member|prohirespowerhouse|Reply\s*to\s*[:*]).*?<\/(?:p|div|li|h\d|span|a)>", "", RegexOptions.IgnoreCase);

            // 2. Remove recruiter email chatter / intro paragraphs leftover
            cleaned = Regex.Replace(cleaned, @"(?is)<p>\s*.*?(?:this\s+is\s+[A-Za-z\s]+from|working\s+as\s+an?\s+IT\s+Recruiter|immediate\s+opportunity\s+with\s+one\s+of\s+our\s+clients|find\s+attached\s+job\s+description|forward\s+your\s+resume\s+to|Hello\s+Professionals|Dear\s+Professionals).*?<\/p>", "", RegexOptions.IgnoreCase);

            // 3. Remove standalone promo / header lines or stray banner fragments
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?(?:Sign[- ]Up\s+for\s+your\s+account|PROHIRES|POWERHOUSE|Hire\s+our\s+IT\s+Recruiter|\$499\/month|unsubscri|mailing\s*list|fe_web_member|prohirespowerhouse).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?is)<p>\s*[|•*–—\-\s]*(?:Update|Subscribe|Remove|Unsubscribe)\s*<\/p>", "", RegexOptions.IgnoreCase);

            // 4. Remove recruiter phone and email lines from description HTML if any
            cleaned = Regex.Replace(cleaned, @"(?im)<(?:p|li|span|div)[^>]*>\s*(?:(?:E|Email|Mail|V|P|T|Cell|Phone|Direct|M|Mobile|Tel|Fax|Contact|Voice)\s*[*:]*\s*)?[\w.\-+]+@[\w.\-]+\.[a-z]{2,}\s*<\/(?:p|li|span|div)>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"(?im)<(?:p|li|span|div)[^>]*>\s*(?:V|P|T|Cell|Phone|Direct|M|Mobile|Tel|Fax|Contact|Voice)\s*[*:]*\s*[\d()\-\s.+xext]+\s*<\/(?:p|li|span|div)>", "", RegexOptions.IgnoreCase);

            // 5. Clean up corrupted UTF-8 / ASCII encoding glitches
            cleaned = cleaned.Replace("â€¢", "•")
                             .Replace("â€\"", "–")
                             .Replace("â€™", "'")
                             .Replace("â€œ", "\"")
                             .Replace("â€ ", "\"")
                             .Replace("â€¦", "...")
                             .Replace("&nbsp;", " ");

            // 6. Fix concatenated ?? or emoji fields like "?? Location: Stamford, CT ?? Work Model: 100% Onsite..."
            cleaned = Regex.Replace(cleaned, @"(?:\?{1,3}|[•·📍🏛️💼🎯✅✔☑👉🔹📌])\s*([A-Za-z\s]{3,30}:)", "<br/><strong>$1</strong>");
            cleaned = Regex.Replace(cleaned, @"(?i)(?:Location|Work\s*Model|Engagement|Focus|Duration|Experience|Rate|Client|Visa|Position|Role):\*", "<strong>$0</strong>");
            cleaned = Regex.Replace(cleaned, @":\*", ":");

            // 7. Remove rogue ? or ?? at the start of bullet lines, paragraphs, or lists (preserve all digits/numbers/ranges)
            cleaned = Regex.Replace(cleaned, @"(?m)^\s*(?:\?{1,3}|[•·▪▫►])\s+", "• ");
            cleaned = Regex.Replace(cleaned, @"<li[^>]*>\s*(?:\?{1,3}|[•·▪▫►])\s*", "<li>");
            cleaned = Regex.Replace(cleaned, @"<p[^>]*>\s*(?:\?{1,3}|[•·▪▫►])\s*", "<p>");
            cleaned = Regex.Replace(cleaned, @"<li[^>]*>\s*[-–—]\s+", "<li>");
            cleaned = Regex.Replace(cleaned, @"<p[^>]*>\s*[-–—]\s+", "<p>");

            // 8. Fix stray asterisk formatting artifacts like "<strong>Focus:*</strong>" or "Focus:*"
            cleaned = Regex.Replace(cleaned, @"\b([A-Za-z0-9\s]+):\*+", "<strong>$1:</strong>");
            cleaned = Regex.Replace(cleaned, @"<strong>([A-Za-z0-9\s]+):\*+<\/strong>", "<strong>$1:</strong>");

            // 9. Remove empty paragraphs, lists, and divs leftover from removals
            cleaned = Regex.Replace(cleaned, @"<p>\s*<\/p>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<ul>\s*<\/ul>", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"<div>\s*<\/div>", "", RegexOptions.IgnoreCase);

            return cleaned.Trim();
        }

        private static readonly Regex BulletStartRegex = new Regex(@"^(?:[•·\u2022\u2023\u25E6\u2043\u2219\u25AA\u25AB►▪▫*~]|\d+[\.\)]\s+|[-–—]\s+|\+\s+|[oO]\s+|\?{1,2}\s+|\.\s*|[📍🏛️💼🎯✅✔☑👉🔹📌]+\s*)\s*", RegexOptions.Compiled);
        private static readonly Regex MetadataLineRegex = new Regex(@"^(?:[📍🏛️💼🎯✅✔☑👉🔹📌\?•*\-\s]*)(Role|Position|Job\s*Title|Location|Work\s*Model|Engagement|Focus|Job\s*Type|Employment\s*Type|Experience(?:\s*Required)?|Total\s*Exp|Skills|Key\s*Skills|Mandatory\s*Skills|Work\s*Authorization|Rate|Client|Duration|Salary|Tax\s*Term|Openings?)\s*[:\-]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string CleanDescriptionToHtml(string rawBody, string title)
        {
            string cleaned = CleanForwardHeadersAndMetadata(rawBody);
            if (string.IsNullOrWhiteSpace(cleaned)) return string.Empty;

            // Pre-process: separate concatenated ?? or emoji fields or merged metadata fields into separate lines
            cleaned = Regex.Replace(cleaned, @"(?:\?{1,3}|[•·📍🏛️💼🎯✅✔☑👉🔹📌])\s*([A-Za-z\s]{3,30}:)", "\n$1");
            cleaned = Regex.Replace(cleaned, @"(?<=[^\r\n])\s+(Role|Position|Job\s*Title|Location|Work\s*Model|Engagement|Duration|Rate|Client|Experience|Mandatory\s*Skills|Skills|Requirements|Qualifications)\s*:", "\n$1:", RegexOptions.IgnoreCase);

            var rawLines = cleaned.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            bool inList = false;
            bool inSignature = false;
            bool inJobSection = false;
            var currentParagraph = new StringBuilder();
            var signatureLines = new List<string>();

            void FlushParagraph()
            {
                if (currentParagraph.Length > 0)
                {
                    string text = currentParagraph.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine($"<p>{ConvertMarkdownFormatting(text)}</p>");
                    }
                    currentParagraph.Clear();
                }
            }

            void CloseList()
            {
                if (inList)
                {
                    sb.AppendLine("</ul>");
                    inList = false;
                }
            }

            bool seenFirstValidContent = false;

            for (int i = 0; i < rawLines.Length; i++)
            {
                string line = rawLines[i].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    FlushParagraph();
                    continue;
                }

                // If before first valid content, skip redundant title echoes
                if (!seenFirstValidContent)
                {
                    if (Regex.IsMatch(line, @"^(?:Job\s*Title|Position|Role)\s*[:\-]\s*" + Regex.Escape(title) + @"\s*$", RegexOptions.IgnoreCase) ||
                        line.Equals(title, StringComparison.OrdinalIgnoreCase))
                    {
                        // Keep if it contains more details like location or work model
                        if (line.Length > title.Length + 10)
                        {
                            seenFirstValidContent = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                seenFirstValidContent = true;

                // Detect start of signature block at the bottom
                if (Regex.IsMatch(line, @"^(?:Warm\s*Regards|Best\s*Regards|Kind\s*Regards|Regards\s*,?|Sincerely\s*,?|Thanks\s*(?:&|and)?\s*Regards|Thank\s*you\s*,?|Thanks\s*,?)", RegexOptions.IgnoreCase))
                {
                    FlushParagraph();
                    CloseList();
                    inSignature = true;
                }

                if (inSignature)
                {
                    signatureLines.Add(ConvertMarkdownFormatting(line));
                    continue;
                }

                // Check for Section Header with inline bullet (e.g. "Technical/Functional Skills • Development & design...")
                var inlineBulletMatch = Regex.Match(line, @"^(Technical\/Functional\s+Skills|Roles\s*&\s*Responsibilities|Requirements|Qualifications|Key\s+Responsibilities|Mandatory\s+Skills)\s*[•\*\-►▪▫\?📍🏛️💼🎯✅✔☑👉🔹📌]\s*(.*)$", RegexOptions.IgnoreCase);
                if (inlineBulletMatch.Success)
                {
                    FlushParagraph();
                    CloseList();
                    string header = inlineBulletMatch.Groups[1].Value.Trim().TrimEnd('–', '-');
                    string firstBullet = inlineBulletMatch.Groups[2].Value.Trim();
                    sb.AppendLine($"<h4><strong>{header}</strong></h4>");
                    inJobSection = true;
                    if (!string.IsNullOrWhiteSpace(firstBullet))
                    {
                        sb.AppendLine("<ul>");
                        sb.AppendLine($"  <li>{ConvertMarkdownFormatting(firstBullet)}</li>");
                        inList = true;
                    }
                    continue;
                }

                // Check for Bullet Point (•, *, -, +, ►, ▪, numbers like 1., or "o ")
                if (BulletStartRegex.IsMatch(line))
                {
                    // Check if it's actually a metadata line like "📍 Location: Stamford, CT"
                    if (MetadataLineRegex.IsMatch(line))
                    {
                        FlushParagraph();
                        CloseList();
                        string cleanMeta = Regex.Replace(line, @"^[📍🏛️💼🎯✅✔☑👉🔹📌\?•*\-\s]+", "");
                        sb.AppendLine($"<p>{ConvertMarkdownFormatting(cleanMeta)}</p>");
                        continue;
                    }

                    FlushParagraph();
                    if (!inList)
                    {
                        sb.AppendLine("<ul>");
                        inList = true;
                    }
                    string bulletText = BulletStartRegex.Replace(line, "").Trim();
                    sb.AppendLine($"  <li>{ConvertMarkdownFormatting(bulletText)}</li>");
                }
                // Check for Section Header (e.g. "Mandatory Skills:", "Experience", "Skills", "Nice to Have", "Job Description", "Roles & Responsibilities")
                else if (IsSectionHeaderLineOrHeading(line))
                {
                    FlushParagraph();
                    CloseList();
                    string cleanHeader = Regex.Replace(line, @"^[:\-•*# \.–📍🏛️💼🎯✅✔☑👉🔹📌]+|[:\-•*# \.–📍🏛️💼🎯✅✔☑👉🔹📌]+$", "").Trim();
                    sb.AppendLine($"<h4><strong>{cleanHeader}</strong></h4>");
                    inJobSection = true;
                }
                // Check for standalone metadata lines like "Role: ...", "Location: ...", "Duration: ..."
                else if (MetadataLineRegex.IsMatch(line))
                {
                    FlushParagraph();
                    CloseList();
                    string cleanMeta = Regex.Replace(line, @"^[📍🏛️💼🎯✅✔☑👉🔹📌\?•*\-\s]+", "");
                    sb.AppendLine($"<p>{ConvertMarkdownFormatting(cleanMeta)}</p>");
                }
                // If we are inside a Job Description / Responsibilities / Skills / Qualifications section and line looks like a requirement bullet
                else if (inJobSection && (Regex.IsMatch(line, @"^\d+\+\s*years\b", RegexOptions.IgnoreCase) || (inList && line.Length < 300)))
                {
                    FlushParagraph();
                    if (!inList)
                    {
                        sb.AppendLine("<ul>");
                        inList = true;
                    }
                    sb.AppendLine($"  <li>{ConvertMarkdownFormatting(line)}</li>");
                }
                else
                {
                    if (inList)
                    {
                        CloseList();
                    }

                    if (currentParagraph.Length > 0)
                    {
                        currentParagraph.Append(" ");
                    }
                    currentParagraph.Append(line);
                }
            }

            FlushParagraph();
            CloseList();

            if (signatureLines.Count > 0)
            {
                sb.AppendLine($"<p>{string.Join("<br/>", signatureLines)}</p>");
            }

            return sb.ToString().Trim();
        }

        private static string ConvertMarkdownFormatting(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
            // Clean metadata label prefix formatting: e.g. "Focus:* ...", "Role: **DotNet..." -> "<strong>Focus:</strong> ...", "<strong>Role:</strong> DotNet..."
            text = Regex.Replace(text, @"\b(Focus|Location|Work\s*Model|Engagement|Role|Position|Job\s*Title|Job\s*Type|Employment\s*Type|Job\s*Description|Experience(?:\s*Required)?|Total\s*Exp|Skills|Key\s*Skills|Mandatory\s*Skills|Work\s*Authorization|Rate|Client|Duration|Salary)\s*[:]\*?\s*", "<strong>$1:</strong> ", RegexOptions.IgnoreCase);
            
            // Convert mismatched or matched double/single asterisks: **bold**, **bold*, *bold**, *bold* -> <strong>bold</strong>
            text = Regex.Replace(text, @"\*{1,2}([^\*\r\n]+?)\*{1,2}", "<strong>$1</strong>");
            
            // Remove any trailing asterisk artifacts: e.g. "Contract.*" -> "Contract." or "text *" -> "text"
            text = Regex.Replace(text, @"\.\*\s*$", ".");
            text = Regex.Replace(text, @"\s*\*\s*$", "");
            text = Regex.Replace(text, @":\*", ":");

            return text;
        }

        private bool IsSectionHeaderLineOrHeading(string line)
        {
            string clean = line.Trim();
            string stripped = clean.Trim(':', '-', '•', '*', '#', ' ');
            if (stripped.Length > 60 || string.IsNullOrWhiteSpace(stripped)) return false;

            if (clean.TrimEnd('*', ' ').EndsWith(":") && stripped.Length < 40) return true;
            
            return SectionBlacklist.Contains(stripped) ||
                   Regex.IsMatch(stripped, @"^(?:Key\s+|Core\s+|Primary\s+)?Responsibilities$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:Key\s+|Required\s+|Technical\s+|Core\s+|Mandatory\s+)?Skills(?:\s*&\s*Requirements)?$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:Basic\s+|Minimum\s+|Preferred\s+|Required\s+)?Qualifications$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:Job\s+|Role\s+)?(?:Description|Overview|Summary|Requirements|Responsibilities|Qualifications|Experience)$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:What\s+You('?ll|\s+Will)\s+Do|What\s+We\s+Are\s+Looking\s+For|Who\s+You\s+Are)$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:Must\s+Have(?:\s*[-–—]\s*Key\s+Requirements)?|Nice\s+to\s+Have|Key\s+Deliverables)$", RegexOptions.IgnoreCase) ||
                   Regex.IsMatch(stripped, @"^(?:About\s+(?:Us|The\s+Company|The\s+Role|The\s+Job))$", RegexOptions.IgnoreCase) ||
                   stripped.Equals("Experience", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("Skills", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("Mandatory Skills", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("Nice to Have", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("ROLE DESCRIPTION", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("Technical/Functional Skills", StringComparison.OrdinalIgnoreCase) ||
                   stripped.Equals("Roles & Responsibilities", StringComparison.OrdinalIgnoreCase);
        }

        public static string CleanJobTitle(string rawSubject, string? body = null)
        {
            // If body has labeled Position: / Job Title: / Role: and rawSubject was generic, use that
            if (!string.IsNullOrWhiteSpace(body) && (string.IsNullOrWhiteSpace(rawSubject) || rawSubject.Equals("Job Opening", StringComparison.OrdinalIgnoreCase) || rawSubject.Length < 4))
            {
                var posMatch = Regex.Match(body, @"(?im)^\s*(?:Position|Job\s*Title|Role|Designation|Requirement)\s*[:\-]\s*([^\r\n]+)");
                if (posMatch.Success)
                {
                    string candidate = CleanJobTitle(posMatch.Groups[1].Value, null);
                    if (!string.IsNullOrWhiteSpace(candidate) && candidate.Length >= 4 && !SectionBlacklist.Contains(candidate.Trim()))
                    {
                        return FormatJobTitleCase(candidate);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rawSubject)) return "Job Opening";

            string title = rawSubject.Trim();

            // 1. Remove prefixes: Fwd:, Re:, Urgent Requirement for, Immediate Requirement for, Position:, Hiring for, Opportunity for, Hot Requirement:, Need:, etc.
            title = Regex.Replace(title, @"^(?:fwd\s*:|re\s*:|position\s*:|role\s*:|title\s*:|job\s*title\s*:|req\s*(?:id)?\s*:?|urgent\s*:|urgent\s+requirement\s*(?:for|:)?|urgent\s+need\s*:?|urgent\s+opening\s*:?|immediate\s+requirement\s*(?:for|:)?|immediate\s+opportunity\s*(?:for|:)?|immediate\s+need\s*:?|immediate\s+opening\s*:?|requirement\s*(?:for|:)?|job\s+opening\s*:?|job\s+opportunity\s*(?:for|:)?|job\s+posting\s*:?|opportunity\s+(?:for|with|to)?\s*:?|hot\s+requirement\s*(?:for|:)?|hot\s+need\s*:?|hot\s+opening\s*:?|hot\s+opportunity\s*:?|hot\s*:?|direct\s+client\s+requirement\s*(?:for|:)?|direct\s+client\s*:?|exclusive\s+requirement\s*(?:for|:)?|new\s+requirement\s*(?:for|:)?|new\s+opening\s*:?|open\s+position\s*(?:for|:)?|urgently\s+hiring\s*(?:for)?|hiring\s+(?:for\s*)?(?:an?|the)?|looking\s+for\s*(?:an?|the)?|need\s+(?:for|:)?|need\s*:|urgent)\s*[:-]?\s*", "", RegexOptions.IgnoreCase).Trim();
            title = Regex.Replace(title, @"^(?:for|with|in|at|on|an?|the|–|-|:)\s+", "", RegexOptions.IgnoreCase).Trim();

            // 2. Remove parenthetical rate/salary/location: e.g. "($75/hr)", "(Plano, TX)", "(Austin, TX, US)", "(Remote)", "(In Person Interview)"
            title = Regex.Replace(title, @"\s*\([^)]*\)", "", RegexOptions.IgnoreCase).Trim();
            title = Regex.Replace(title, @"\s*\[[^\]]*\]", "", RegexOptions.IgnoreCase).Trim();

            char[] trimChars = new[] { '•', '*', '-', ',', '|', '/', ':', ';', ' ', '\t', '\r', '\n', '–', '—', '·', '►', '>', '<', '?', '!', '~', '_', '#', '\"', '\'' };
            title = title.Trim(trimChars);
            title = Regex.Replace(title, @"[\u0080-\uFFFF]+$", "").Trim(trimChars);

            string multiWordCities = @"(?:New\s+York|San\s+Francisco|Los\s+Angeles|Las\s+Vegas|Santa\s+Clara|San\s+Jose|San\s+Diego|Salt\s+Lake\s+City|Baton\s+Rouge|Little\s+Rock|Kansas\s+City|Oklahoma\s+City|Virginia\s+Beach|Jersey\s+City|Saint\s+Paul|St\.?\s+Louis|Fort\s+Worth|El\s+Paso|Des\s+Moines|Grand\s+Rapids|Colorado\s+Springs|North\s+[A-Za-z]+|South\s+[A-Za-z]+|East\s+[A-Za-z]+|West\s+[A-Za-z]+)";
            string singleWordCities = @"(?:Stamford|Plano|Dallas|Irving|Frisco|Austin|Houston|Edison|Princeton|Atlanta|Charlotte|Raleigh|Tampa|Orlando|Miami|Chicago|Columbus|Cincinnati|Cleveland|Phoenix|Seattle|Boston|Philadelphia|Pittsburgh|Minneapolis|Detroit|Denver|Indianapolis|Baltimore|Milwaukee|Nashville|Memphis|Louisville|Jacksonville|Richmond|Hartford|Newark|Trenton|Albany|Buffalo|Rochester|Omaha|Tulsa|Wichita|Arlington|Aurora|Boise|Portland)";
            string usStates = @"(?:AL|AK|AZ|AR|CA|CO|CT|DE|FL|GA|HI|ID|IL|IN|IA|KS|KY|LA|ME|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|OH|OK|OR|PA|RI|SC|SD|TN|TX|UT|VT|VA|WA|WV|WI|WY|Ct|Tx|Nj|Ny|Ca|Fl|Ga|Il|Oh|Pa|Nc|Va|Wa|Ma|Texas|California|Florida|Connecticut|New\s+Jersey|New\s+York|Virginia|Illinois|Washington|Georgia|Ohio|Pennsylvania|North\s+Carolina|Michigan|Massachusetts|Colorado|Arizona|Maryland|Missouri|Indiana|Tennessee|Minnesota|Wisconsin)";

            // Iteratively strip trailing location, state, city, work-arrangement, rate suffixes
            bool changed = true;
            while (changed)
            {
                string before = title;

                // Remove trailing parenthetical
                title = Regex.Replace(title, @"\s*\([^)]*\)\s*$", "", RegexOptions.IgnoreCase).Trim();

                // Remove trailing work arrangements / interview notes:
                // e.g. "- Day 1 Onsite", "– Day 1 Onsite", "- Onsite", "- Hybrid", "- Remote", "- In Person Interview", "Day 1 Onsite"
                title = Regex.Replace(title, @"(?:\s*[-|–|—|•|/|\|,]\s*|\s+\b(?:is|with|mode)\b\s+)?\b(?:Day\s*\d+\s*(?:Onsite|Hybrid|In\s*Office)|\d+\s*Days?\s*(?:Onsite|Hybrid|In\s*Office)|(?:100%\s*)?(?:Remote|Hybrid|Onsite|On-site|In-Person|In\s*Person(?:\s*Interview)?))\b\s*$", "", RegexOptions.IgnoreCase).Trim();

                // Remove trailing City, State [, Country] or City State (e.g. "- Jersey City, NJ", ", Jersey City, NJ", "- Plano, TX, USA", "- Stamford Ct", "– Stamford Ct")
                title = Regex.Replace(title, $@"(?:\s*[-|–|—|•|/|\|,]\s*|\s+\b(?:in|at|for|location)\b\s+)(?:{multiWordCities}|{singleWordCities}|[A-Za-z\s]{{2,25}})(?:,\s*|\s+)\b(?:{usStates})\b(?:\s*,\s*\b(?:US|USA|United States)\b)?\s*$", "", RegexOptions.IgnoreCase).Trim();

                // Remove trailing City alone (with leading delimiter, e.g. "- Jersey City", ", Jersey City", "- Plano", "– Stamford")
                title = Regex.Replace(title, $@"(?:\s*[-|–|—|•|/|\|,]\s*|\s+\b(?:in|at|for)\b\s+)(?:{multiWordCities}|{singleWordCities})\s*$", "", RegexOptions.IgnoreCase).Trim();

                // Remove trailing State or Country alone (e.g. "- NJ", ", TX", "- US", "in USA", "- CT", "– Ct")
                title = Regex.Replace(title, $@"(?:\s*[-|–|—|•|/|\|,]\s*|\s+\b(?:in|at)\b\s+)\b(?:{usStates}|US|USA|United States)\b\s*$", "", RegexOptions.IgnoreCase).Trim();

                // Remove trailing tax terms / rates (e.g. "- C2C", "- W2", "- Contract", "- $70/hr")
                title = Regex.Replace(title, @"(?:\s*[-|–|—|•|/|\|,]\s*)\b(?:\$?\d+.*|C2C|W2|Contract|Full\s*Time|FTE|1099|Direct\s*Hire|Urgent)\b\s*$", "", RegexOptions.IgnoreCase).Trim();

                title = title.Trim(trimChars);
                title = Regex.Replace(title, @"[\u0080-\uFFFF]+$", "").Trim(trimChars);

                changed = !title.Equals(before, StringComparison.Ordinal);
            }

            // Remove any remaining hyphens/dashes, en-dashes, em-dashes, pipes, slashes, asterisks, question marks, unwanted symbols
            title = Regex.Replace(title, @"[-–—|/]+", " ");
            title = Regex.Replace(title, @"[*#?~!_`""'<>]+", "");
            title = Regex.Replace(title, @"\s+", " ").Trim(trimChars);

            if (string.IsNullOrWhiteSpace(title) || SectionBlacklist.Contains(title.Trim())) title = "Job Opening";

            return FormatJobTitleCase(title);
        }

        private static readonly HashSet<string> AcronymsUpper = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AWS", "GCP", "SAP", "IBM", "AZURE", "API", "APIs", "REST", "SQL", "QA", "SDET", "ETL", "UI", "UX",
            "AI", "ML", "NLP", "BI", "ERP", "CRM", "D365", "M365", "ALM", "PL", "AZ", "JS", "OOB", "UAT",
            "DBA", "PHP", "HTML", "CSS", "CI/CD", "DevOps", "NoSQL",
            "IT", "HR", "C2C", "W2", "RPA", "GIS", "IAM", "SIEM", "SOC", "SRE", "PMP", "Scrum",
            "C#", ".NET", "iOS", "Android", "Golang", "JavaScript", "TypeScript", "Node.js", "Vue.js", "React.js"
        };

        private static readonly HashSet<string> MinorWordsLower = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and", "or", "for", "with", "in", "of", "to", "on", "at", "by", "a", "an", "the"
        };

        public static string FormatJobTitleCase(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "Job Opening";

            // Tokenize while preserving word delimiters like spaces, slashes, dashes, parentheses
            var tokens = Regex.Split(title.Trim(), @"(\s+|[-/(),])");
            var sb = new StringBuilder();

            bool isFirstWord = true;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (string.IsNullOrEmpty(token)) continue;

                if (Regex.IsMatch(token, @"^(\s+|[-/(),])$"))
                {
                    sb.Append(token);
                    continue;
                }

                // Match known acronyms (e.g. AWS, GCP, AZURE, SAP, IBM, SQL, QA, .NET)
                var matchedAcronym = AcronymsUpper.FirstOrDefault(a => a.Equals(token, StringComparison.OrdinalIgnoreCase));
                if (matchedAcronym != null)
                {
                    sb.Append(matchedAcronym);
                }
                else if (!isFirstWord && MinorWordsLower.Contains(token))
                {
                    sb.Append(token.ToLowerInvariant());
                }
                else
                {
                    // Camel / Title case: first letter uppercase, remainder lowercase
                    if (token.Length == 1)
                    {
                        sb.Append(char.ToUpperInvariant(token[0]));
                    }
                    else
                    {
                        sb.Append(char.ToUpperInvariant(token[0]))
                          .Append(token.Substring(1).ToLowerInvariant());
                    }
                }

                isFirstWord = false;
            }

            return sb.ToString().Trim();
        }

        private static readonly Dictionary<string, string> StateMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AL", "AL" }, { "Alabama", "AL" },
            { "AK", "AK" }, { "Alaska", "AK" },
            { "AZ", "AZ" }, { "Arizona", "AZ" },
            { "AR", "AR" }, { "Arkansas", "AR" },
            { "CA", "CA" }, { "California", "CA" },
            { "CO", "CO" }, { "Colorado", "CO" },
            { "CT", "CT" }, { "Connecticut", "CT" },
            { "DE", "DE" }, { "Delaware", "DE" },
            { "FL", "FL" }, { "Florida", "FL" },
            { "GA", "GA" }, { "Georgia", "GA" },
            { "HI", "HI" }, { "Hawaii", "HI" },
            { "ID", "ID" }, { "Idaho", "ID" },
            { "IL", "IL" }, { "Illinois", "IL" },
            { "IN", "IN" }, { "Indiana", "IN" },
            { "IA", "IA" }, { "Iowa", "IA" },
            { "KS", "KS" }, { "Kansas", "KS" },
            { "KY", "KY" }, { "Kentucky", "KY" },
            { "LA", "LA" }, { "Louisiana", "LA" },
            { "ME", "ME" }, { "Maine", "ME" },
            { "MD", "MD" }, { "Maryland", "MD" },
            { "MA", "MA" }, { "Massachusetts", "MA" },
            { "MI", "MI" }, { "Michigan", "MI" },
            { "MN", "MN" }, { "Minnesota", "MN" },
            { "MS", "MS" }, { "Mississippi", "MS" },
            { "MO", "MO" }, { "Missouri", "MO" },
            { "MT", "MT" }, { "Montana", "MT" },
            { "NE", "NE" }, { "Nebraska", "NE" },
            { "NV", "NV" }, { "Nevada", "NV" },
            { "NH", "NH" }, { "New Hampshire", "NH" },
            { "NJ", "NJ" }, { "New Jersey", "NJ" },
            { "NM", "NM" }, { "New Mexico", "NM" },
            { "NY", "NY" }, { "New York", "NY" },
            { "NC", "NC" }, { "North Carolina", "NC" },
            { "ND", "ND" }, { "North Dakota", "ND" },
            { "OH", "OH" }, { "Ohio", "OH" },
            { "OK", "OK" }, { "Oklahoma", "OK" },
            { "OR", "OR" }, { "Oregon", "OR" },
            { "PA", "PA" }, { "Pennsylvania", "PA" },
            { "RI", "RI" }, { "Rhode Island", "RI" },
            { "SC", "SC" }, { "South Carolina", "SC" },
            { "SD", "SD" }, { "South Dakota", "SD" },
            { "TN", "TN" }, { "Tennessee", "TN" },
            { "TX", "TX" }, { "Texas", "TX" },
            { "UT", "UT" }, { "Utah", "UT" },
            { "VT", "VT" }, { "Vermont", "VT" },
            { "VA", "VA" }, { "Virginia", "VA" },
            { "WA", "WA" }, { "Washington", "WA" },
            { "WV", "WV" }, { "West Virginia", "WV" },
            { "WI", "WI" }, { "Wisconsin", "WI" },
            { "WY", "WY" }, { "Wyoming", "WY" }
        };

        private static string NormalizeState(string rawState)
        {
            if (StateMap.TryGetValue(rawState.Trim(), out var abbr)) return abbr;
            return rawState.ToUpper().Trim();
        }

        private string? ExtractLocation(string subject, string body, out bool isRemote)
        {
            isRemote = false;

            string fullStateNames = @"Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|Florida|Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|New\s+Hampshire|New\s+Jersey|New\s+Mexico|New\s+York|North\s+Carolina|North\s+Dakota|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode\s+Island|South\s+Carolina|South\s+Dakota|Tennessee|Texas|Utah|Vermont|Virginia|Washington|West\s+Virginia|Wisconsin|Wyoming";
            string stateCodes = @"AL|AK|AZ|AR|CA|CO|CT|DE|FL|GA|HI|ID|IL|IN|IA|KS|KY|LA|ME|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|OH|OK|OR|PA|RI|SC|SD|TN|TX|UT|VT|VA|WA|WV|WI|WY";
            string allStatesPattern = $@"(?:{stateCodes}|{fullStateNames})";
            string multiWordCities = @"(?:New\s+York|San\s+Francisco|Los\s+Angeles|Las\s+Vegas|Santa\s+Clara|San\s+Jose|San\s+Diego|Salt\s+Lake\s+City|Baton\s+Rouge|Little\s+Rock|Kansas\s+City|Oklahoma\s+City|Virginia\s+Beach|Jersey\s+City|Saint\s+Paul|St\.?\s+Louis|Fort\s+Worth|El\s+Paso|Des\s+Moines|Grand\s+Rapids|Colorado\s+Springs|North\s+[A-Za-z]+|South\s+[A-Za-z]+)";
            string singleWordCities = @"(?:Stamford|Plano|Dallas|Irving|Frisco|Austin|Houston|Edison|Princeton|Atlanta|Charlotte|Raleigh|Tampa|Orlando|Miami|Chicago|Columbus|Cincinnati|Cleveland|Phoenix|Seattle|Boston|Philadelphia|Pittsburgh|Minneapolis|Detroit|Denver|Indianapolis|Baltimore|Milwaukee|Nashville|Memphis|Louisville|Jacksonville|Richmond|Hartford|Newark|Trenton|Albany|Buffalo|Rochester|Omaha|Tulsa|Wichita|Arlington|Aurora|Boise|Portland)";

            // 1. Check for labeled location in body (e.g. "Location : Charlotte NC (Onsite)", "Location: Plano, TX", "Work Location - Dallas, TX", "Based in Atlanta GA")
            var labelPattern = @"(?:Location|Work\s*Location|Job\s*Location|Client\s*Location|Office\s*Location|Base\s*Location|Current\s*Location|Primary\s*Location|Position\s*Location|Place|Based\s+in|City(?:\s*[\/&]\s*State)?)\s*(?:[:\-–—|]|\bis\b|\bis\s+in\b|\s)\s*([^\r\n]+)";
            var labeledLineMatch = Regex.Match(body, labelPattern, RegexOptions.IgnoreCase);
            if (labeledLineMatch.Success)
            {
                string lineVal = labeledLineMatch.Groups[1].Value.Trim();
                
                if (Regex.IsMatch(lineVal, @"\b(remote|work\s+from\s+home|wfh)\b", RegexOptions.IgnoreCase))
                {
                    isRemote = true;
                }

                var cityStateMatch = Regex.Match(lineVal, $@"\b({multiWordCities}|{singleWordCities}|[A-Za-z]{{2,25}}(?:[ \t]+[A-Za-z]{{2,25}})?)(?:,\s*|[ \t]+)\b({allStatesPattern})\b", RegexOptions.IgnoreCase);
                if (cityStateMatch.Success)
                {
                    string city = CleanCityName(cityStateMatch.Groups[1].Value);
                    string state = NormalizeState(cityStateMatch.Groups[2].Value);
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        return $"{city}, {state}";
                    }
                }

                var cityMatch = Regex.Match(lineVal, $@"\b({multiWordCities}|{singleWordCities})\b", RegexOptions.IgnoreCase);
                if (cityMatch.Success)
                {
                    string city = CleanCityName(cityMatch.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        var stateMatch = Regex.Match(lineVal, $@"\b({allStatesPattern})\b", RegexOptions.IgnoreCase);
                        if (stateMatch.Success)
                        {
                            return $"{city}, {NormalizeState(stateMatch.Groups[1].Value)}";
                        }
                        return city;
                    }
                }

                if (isRemote) return "Remote";
            }

            // 2. Check for City, State or City State in the Subject Line (e.g. "Technical Program Manager - Charlotte, NC" or "Charlotte NC")
            if (!string.IsNullOrWhiteSpace(subject))
            {
                var subjMatch = Regex.Match(subject, $@"\b({multiWordCities}|{singleWordCities}|[A-Za-z]{{2,20}}(?:[ \t]+[A-Za-z]{{2,20}})?)(?:,\s*|[ \t]+)\b({allStatesPattern})\b", RegexOptions.IgnoreCase);
                if (subjMatch.Success)
                {
                    string rawCity = subjMatch.Groups[1].Value;
                    string rawState = subjMatch.Groups[2].Value;
                    if (IsValidCityStateMatch(rawCity, rawState, subject.Substring(subjMatch.Index, subjMatch.Length)))
                    {
                        string city = CleanCityName(rawCity);
                        string state = NormalizeState(rawState);
                        if (!string.IsNullOrWhiteSpace(city)) return $"{city}, {state}";
                    }
                }
            }

            // 3. Check for known City + State anywhere across the body (any line)
            var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine) || IsSectionHeaderLine(trimmedLine)) continue;

                var knownCityMatch = Regex.Match(trimmedLine, $@"\b({multiWordCities}|{singleWordCities})(?:,\s*|[ \t]+)\b({allStatesPattern})\b", RegexOptions.IgnoreCase);
                if (knownCityMatch.Success)
                {
                    string city = CleanCityName(knownCityMatch.Groups[1].Value);
                    string state = NormalizeState(knownCityMatch.Groups[2].Value);
                    if (!string.IsNullOrWhiteSpace(city)) return $"{city}, {state}";
                }

                var lineMatch = Regex.Match(trimmedLine, $@"\b([A-Z][a-z]+(?:[ \t]+[A-Z][a-z]+)?)(?:,\s*|[ \t]+)\b({allStatesPattern})\b");
                if (lineMatch.Success)
                {
                    string rawCity = lineMatch.Groups[1].Value;
                    string rawState = lineMatch.Groups[2].Value;
                    if (IsValidCityStateMatch(rawCity, rawState, trimmedLine.Substring(lineMatch.Index, lineMatch.Length)))
                    {
                        string city = CleanCityName(rawCity);
                        string state = NormalizeState(rawState);
                        if (!string.IsNullOrWhiteSpace(city)) return $"{city}, {state}";
                    }
                }
            }

            // 4. Check for known cities alone anywhere in body
            var generalCityMatch = Regex.Match(body, $@"\b({multiWordCities}|{singleWordCities})\b", RegexOptions.IgnoreCase);
            if (generalCityMatch.Success)
            {
                string city = CleanCityName(generalCityMatch.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(city))
                {
                    var nearby = body.Substring(generalCityMatch.Index, Math.Min(60, body.Length - generalCityMatch.Index));
                    var stateMatch = Regex.Match(nearby, $@"\b({allStatesPattern})\b", RegexOptions.IgnoreCase);
                    if (stateMatch.Success)
                    {
                        return $"{city}, {NormalizeState(stateMatch.Groups[1].Value)}";
                    }
                    return city;
                }
            }

            // 5. Check for explicit Remote / Hybrid markers
            if (Regex.IsMatch(body, @"\b(remote|work\s+from\s+home|wfh)\b", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(subject, @"\b(remote|work\s+from\s+home|wfh)\b", RegexOptions.IgnoreCase))
            {
                isRemote = true;
                return "Remote";
            }

            return null;
        }

        private static readonly HashSet<string> FalseStateWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "in", "or", "me", "ok", "oh", "id", "co", "la", "is", "as", "at", "to", "by", "on", "no", "so", "do", "go", "up", "if", "my", "we", "he", "it", "us", "am"
        };

        private static bool IsValidCityStateMatch(string city, string state, string matchedSegment)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state)) return false;
            if (SectionBlacklist.Contains(city.Trim())) return false;

            if (FalseStateWords.Contains(state.Trim()))
            {
                if (!matchedSegment.Contains(",") && state != state.ToUpperInvariant())
                {
                    return false;
                }
            }
            return true;
        }

        private static readonly HashSet<string> SectionBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Summary", "Description", "Overview", "Title", "Position", "Role", "About", "Details", "Client",
            "Company", "Location", "Requirement", "Requirements", "Responsibilities", "Qualifications", "Note",
            "Notice", "Urgent", "Immediate", "Developer", "Engineer", "Lead", "Senior", "Junior", "Full Stack",
            "Candidate", "Profile", "Experience", "Skills", "Salary", "Rate", "Education", "Benefits"
        };

        private bool IsSectionHeaderLine(string line)
        {
            string clean = line.Trim().Trim(':', '-', '•', '*', '#');
            return SectionBlacklist.Contains(clean);
        }

        private string CleanCityName(string rawCity)
        {
            if (string.IsNullOrWhiteSpace(rawCity)) return string.Empty;
            rawCity = Regex.Replace(rawCity, @"^(?:based\s+in|based|in|at|for|near|around|location|work\s*location)\s+", "", RegexOptions.IgnoreCase).Trim();
            rawCity = Regex.Replace(rawCity, @"[:\-–—|/]+$", "").Trim();
            var words = rawCity.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var cleanWords = words.Where(w => !SectionBlacklist.Contains(w.Trim()) && !w.Equals("Based", StringComparison.OrdinalIgnoreCase)).ToList();
            if (cleanWords.Count == 0) return string.Empty;
            return string.Join(" ", cleanWords);
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            string text = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</(?:p|div|h[1-6]|tr|li)>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</td>", " | ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<[^>]+>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }
    }
}
