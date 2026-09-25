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
    public interface IHotlistParserService
    {
        Task<ParsedHotlistDataDto> ParseHotlistEmailAsync(string subject, string bodyText, string? bodyHtml = null);
    }

    public class HotlistParserService : IHotlistParserService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<HotlistParserService> _logger;
        private readonly HttpClient _httpClient;

        public HotlistParserService(IConfiguration config, ILogger<HotlistParserService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ParsedHotlistDataDto> ParseHotlistEmailAsync(string subject, string bodyText, string? bodyHtml = null)
        {
            string rawContent = !string.IsNullOrWhiteSpace(bodyText) ? bodyText : StripHtml(bodyHtml ?? string.Empty);
            string contentToParse = CleanPreambleAndSignatures(rawContent);

            if (string.IsNullOrWhiteSpace(contentToParse) || contentToParse.Trim().Length < 15)
            {
                throw new InvalidOperationException("Hotlist email body contains no consultant profiles or content.");
            }

            // Try Gemini Flash if API key configured
            string geminiKey = _config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(geminiKey))
            {
                try
                {
                    var geminiResult = await ParseWithGeminiAsync(subject, contentToParse, geminiKey);
                    if (geminiResult != null && geminiResult.Candidates != null && geminiResult.Candidates.Count > 0)
                    {
                        foreach (var cand in geminiResult.Candidates)
                        {
                            cand.Title = JobParserService.FormatJobTitleCase(cand.Title);
                            if (cand.Skills == null || cand.Skills.Count == 0)
                            {
                                cand.Skills = ExtractSkillsFromText(cand.Title + " " + cand.Comment);
                            }
                            cand.Skills = cand.Skills.Take(5).ToList();
                        }
                        return geminiResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini Hotlist parsing failed or not configured, using deterministic text extraction.");
                }
            }

            // Rule-based engine fallback
            var ruleResult = ParseWithRuleBasedEngine(subject, contentToParse);
            return ruleResult;
        }

        private async Task<ParsedHotlistDataDto?> ParseWithGeminiAsync(string subject, string body, string apiKey)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            string prompt = $@"You are an expert HR bench sales recruiter and parser. The recruiter sent an email containing a HOTLIST of available bench candidates / consultants.
CRITICAL MANDATES:
1. Extract ALL candidate profiles listed in the email (whether in table format, numbered list, bullet list, key-value blocks, or paragraph summaries).
2. For each candidate extract:
   - candidateName: Name of the consultant (e.g., 'Vikram S.', 'John Doe', 'Candidate 1')
   - title: Professional Role/Title in Title Case without location (e.g. 'Senior Java Full Stack Developer', 'Data Engineer - Snowflake', 'AWS DevOps Engineer')
   - totalExp: Number of years of experience as an integer (e.g. 8)
   - visa: Visa / Work Authorization (e.g. 'H1B', 'USC', 'Green Card', 'OPT EAD', 'H4 EAD', 'TN', 'ANY')
   - location: Current City, State or 'Remote'
   - remoteOnly: true/false
   - canRelocate: true/false
   - fromAmt: hourly rate if mentioned (integer, e.g. 70)
   - skills: Exactly 5 top key technical skills/technologies required or possessed by this candidate (e.g. ['Java', 'Spring Boot', 'Microservices', 'Kafka', 'AWS'])
   - comment: 1-2 sentence professional bio/summary of experience and strengths
   - employmentTypes: ['Remote', 'Onsite', 'Hybrid']
   - jobTypes: ['C2C', 'W2']
3. DO NOT fabricate candidates. Only extract consultants physically listed in the email.

Email Subject: {subject}
Email Content:
{body}

Respond ONLY with valid JSON matching this schema:
{{
  ""emailSubject"": ""{subject}"",
  ""recruiterNotes"": ""Any general notes from recruiter"",
  ""candidates"": [
    {{
      ""candidateName"": ""Candidate Name"",
      ""title"": ""Candidate Job Title"",
      ""totalExp"": 8,
      ""visa"": ""H1B"",
      ""location"": ""Dallas, TX"",
      ""remoteOnly"": false,
      ""canRelocate"": true,
      ""fromAmt"": 70,
      ""skills"": [""Skill1"", ""Skill2"", ""Skill3"", ""Skill4"", ""Skill5""],
      ""employmentTypes"": [""Hybrid""],
      ""jobTypes"": [""C2C""],
      ""comment"": ""Summary of experience""
    }}
  ]
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
                _logger.LogWarning("Gemini Hotlist API call returned status {StatusCode}", response.StatusCode);
                return null;
            }

            string respString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respString);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return null;

            string jsonText = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
            var parsed = JsonSerializer.Deserialize<ParsedHotlistDataDto>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return parsed;
        }

        public ParsedHotlistDataDto ParseWithRuleBasedEngine(string subject, string body)
        {
            var result = new ParsedHotlistDataDto
            {
                EmailSubject = subject,
                Candidates = new List<ParsedHotlistCandidateDto>()
            };

            var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();

            // Check if tabular text with delimiters like | or tabs
            var tableRows = lines.Where(l => l.Contains("|") || l.Contains("\t")).ToList();
            if (tableRows.Count >= 2)
            {
                foreach (var row in tableRows)
                {
                    // Skip table headers
                    if (Regex.IsMatch(row, @"(?i)(name|title|role|exp|visa|location|rate)\s*\|", RegexOptions.IgnoreCase)) continue;
                    if (Regex.IsMatch(row, @"^[\s\-:|+=]+$")) continue;

                    var cols = row.Split(new[] { '|', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(c => c.Trim())
                                  .Where(c => !string.IsNullOrWhiteSpace(c))
                                  .ToList();

                    if (cols.Count >= 2)
                    {
                        var cand = ParseCandidateFromTokens(cols);
                        if (!string.IsNullOrWhiteSpace(cand.Title))
                        {
                            result.Candidates.Add(cand);
                        }
                    }
                }
            }

            // If not a table or table yielded 0, parse by numbered items (e.g. "1. John Doe - Java Developer...") or bullet blocks
            if (result.Candidates.Count == 0)
            {
                var currentBlock = new StringBuilder();
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, @"^\s*(?:\d+[\.\)]|\[\d+\]|•|\*|-|Candidate\s*\d+\s*[:\-])", RegexOptions.IgnoreCase))
                    {
                        if (currentBlock.Length > 0)
                        {
                            var cand = ParseCandidateFromText(currentBlock.ToString());
                            if (!string.IsNullOrWhiteSpace(cand.Title)) result.Candidates.Add(cand);
                            currentBlock.Clear();
                        }
                    }
                    currentBlock.AppendLine(line);
                }

                if (currentBlock.Length > 0)
                {
                    var cand = ParseCandidateFromText(currentBlock.ToString());
                    if (!string.IsNullOrWhiteSpace(cand.Title)) result.Candidates.Add(cand);
                }
            }

            // If still empty, parse the entire text as a single consultant profile
            if (result.Candidates.Count == 0)
            {
                var cand = ParseCandidateFromText(body);
                if (!string.IsNullOrWhiteSpace(cand.Title))
                {
                    result.Candidates.Add(cand);
                }
            }

            return result;
        }

        private ParsedHotlistCandidateDto ParseCandidateFromTokens(List<string> cols)
        {
            var cand = new ParsedHotlistCandidateDto();
            string fullLine = string.Join(" | ", cols);

            // If first column looks like a name (e.g. 2 words or single name without technical words)
            if (cols.Count >= 3 && !Regex.IsMatch(cols[0], @"(?i)\b(developer|engineer|architect|lead|analyst|manager|consultant|admin|tester|qa)\b"))
            {
                cand.CandidateName = cols[0];
                cand.Title = cols[1];
            }
            else
            {
                cand.Title = cols[0];
            }

            cand.Title = JobParserService.CleanJobTitle(cand.Title);

            // Extract experience
            var expMatch = Regex.Match(fullLine, @"\b(\d+)(?:\+|\s*-\s*\d+)?\s*(?:years?|yrs?)\b", RegexOptions.IgnoreCase);
            if (expMatch.Success && int.TryParse(expMatch.Groups[1].Value, out int exp))
            {
                cand.TotalExp = exp;
            }

            // Extract Visa
            cand.Visa = ExtractVisaFromText(fullLine);

            // Extract Location
            cand.Location = ExtractLocationFromText(fullLine, out bool isRemote, out bool canRelocate);
            cand.RemoteOnly = isRemote;
            cand.CanRelocate = canRelocate;

            // Extract Rate
            var rateMatch = Regex.Match(fullLine, @"(?:\$|USD\s*)(\d+)(?:\s*(?:-|to)\s*\$?(\d+))?\s*(?:\/|\s*per\s*)?(?:hr|hour|c2c|w2)?", RegexOptions.IgnoreCase);
            if (rateMatch.Success && short.TryParse(rateMatch.Groups[1].Value, out short rate))
            {
                cand.FromAmt = rate;
            }

            // Extract Skills
            cand.Skills = ExtractSkillsFromText(fullLine);
            cand.Comment = fullLine;

            return cand;
        }

        private ParsedHotlistCandidateDto ParseCandidateFromText(string block)
        {
            var cand = new ParsedHotlistCandidateDto();

            // Check for explicit Name:
            var nameMatch = Regex.Match(block, @"(?im)^\s*(?:Name|Consultant\s*Name|Candidate\s*Name|Consultant)\s*[:\-]\s*([^\r\n]+)");
            if (nameMatch.Success) cand.CandidateName = nameMatch.Groups[1].Value.Trim();

            // Check for explicit Title / Role:
            var roleMatch = Regex.Match(block, @"(?im)^\s*(?:Title|Role|Position|Technology|Skill|Profile)\s*[:\-]\s*([^\r\n]+)");
            if (roleMatch.Success)
            {
                cand.Title = roleMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Take first line as title / summary
                var firstLine = block.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                firstLine = Regex.Replace(firstLine, @"^\s*(?:\d+[\.\)]|\[\d+\]|•|\*|-|Candidate\s*\d+\s*[:\-])\s*", "");

                // Check if first line contains "Name - Title" or "Name : Title"
                var nameTitleMatch = Regex.Match(firstLine, @"^([A-Z][a-zA-Z\.\s]{1,30}?)\s*(?:[-–—|:]|–)\s*([A-Za-z0-9\s/+#\.\(\)]+)$");
                if (nameTitleMatch.Success && !Regex.IsMatch(nameTitleMatch.Groups[1].Value, @"(?i)\b(developer|engineer|architect|lead|analyst|manager|consultant|admin|tester|qa|sr|senior|jr|junior)\b"))
                {
                    if (string.IsNullOrWhiteSpace(cand.CandidateName))
                    {
                        cand.CandidateName = nameTitleMatch.Groups[1].Value.Trim();
                    }
                    cand.Title = nameTitleMatch.Groups[2].Value.Trim();
                }
                else
                {
                    cand.Title = firstLine;
                }
            }

            cand.Title = JobParserService.CleanJobTitle(cand.Title);

            // If name is still empty, check if Title has "Name - Role" pattern
            if (string.IsNullOrWhiteSpace(cand.CandidateName) && !string.IsNullOrWhiteSpace(cand.Title))
            {
                var split = cand.Title.Split(new[] { " - ", " – ", " — ", " : ", " | " }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length >= 2 && split[0].Trim().Length <= 30 && !Regex.IsMatch(split[0], @"(?i)\b(developer|engineer|architect|lead|analyst|manager|consultant|admin|tester|qa)\b"))
                {
                    cand.CandidateName = split[0].Trim();
                    cand.Title = string.Join(" - ", split.Skip(1)).Trim();
                }
            }

            // Extract experience
            var expMatch = Regex.Match(block, @"(?im)^\s*(?:Experience|Exp|Total\s*Exp)\s*[:\-]\s*(\d+)", RegexOptions.IgnoreCase);
            if (!expMatch.Success)
            {
                expMatch = Regex.Match(block, @"\b(\d+)(?:\+|\s*-\s*\d+)?\s*(?:years?|yrs?)\b", RegexOptions.IgnoreCase);
            }
            if (expMatch.Success && int.TryParse(expMatch.Groups[1].Value, out int exp))
            {
                cand.TotalExp = exp;
            }

            // Extract Visa
            cand.Visa = ExtractVisaFromText(block);

            // Extract Location
            cand.Location = ExtractLocationFromText(block, out bool isRemote, out bool canRelocate);
            cand.RemoteOnly = isRemote;
            cand.CanRelocate = canRelocate;

            // Extract Rate
            var rateMatch = Regex.Match(block, @"(?im)(?:Rate|Bill\s*Rate|Pay\s*Rate|Hourly\s*Rate|Comp|Salary)\s*[:\-]\s*\$?(\d+)(?:\s*(?:-|to)\s*\$?(\d+))?", RegexOptions.IgnoreCase);
            if (!rateMatch.Success)
            {
                rateMatch = Regex.Match(block, @"(?:\$|USD\s*)(\d+)(?:\s*(?:-|to)\s*\$?(\d+))?\s*(?:\/|\s*per\s*)?(?:hr|hour|c2c|w2)?", RegexOptions.IgnoreCase);
            }
            if (rateMatch.Success && short.TryParse(rateMatch.Groups[1].Value, out short rate))
            {
                cand.FromAmt = rate;
            }

            // Extract Skills
            cand.Skills = ExtractSkillsFromText(block);
            cand.Comment = block.Trim();

            return cand;
        }

        private static List<string> ExtractSkillsFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var skillKeywords = new[]
            {
                ".NET", "Dotnet", ".NET Core", "C#", "Production Support", "Application Development",
                "Dynamics CRM", "Microsoft Dynamics", "Dynamics 365", "D365", "M365", "Power Platform", "Power Automate",
                "Power Apps", "Dataverse", "Power BI", "Copilot", "Azure AI", "ALM", "ABAP", "APIs",
                "Mainframe", "COBOL", "DB2", "JCL", "CICS", "VSAM",
                "Java", "Spring Boot", "Spring", "Microservices", "Python", "React", "Angular", "Vue", "Node.js",
                "ASP.NET", "AWS", "Azure", "GCP", "Google Cloud", "Kubernetes", "Docker", "SQL", "PostgreSQL", "MongoDB", "Kafka",
                "Spark", "Hadoop", "Snowflake", "Databricks", "DevOps", "CI/CD", "Terraform", "Selenium", "QA", "Automation",
                "Data Engineer", "Machine Learning", "AI", "Salesforce", "ServiceNow", "SAP", "Golang", "Rust", "Swift", "Kotlin",
                "TypeScript", "JavaScript", "HTML", "CSS", "GraphQL", "Redis", "Elasticsearch", "Oracle", "Linux"
            };

            var extracted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kw in skillKeywords)
            {
                string pat = $@"(?:^|[\s,;:(\[/•*\-])" + Regex.Escape(kw) + @"(?:$|[\s,;:)\]/!?*•\-])";
                if (Regex.IsMatch(text, pat, RegexOptions.IgnoreCase))
                {
                    extracted.Add(kw.Equals("Dotnet", StringComparison.OrdinalIgnoreCase) ? ".NET" : kw);
                    if (extracted.Count >= 5) break;
                }
            }

            return extracted.Take(5).ToList();
        }

        private static string ExtractVisaFromText(string text)
        {
            if (Regex.IsMatch(text, @"\b(us\s*citizen|usc)\b", RegexOptions.IgnoreCase)) return "USC";
            if (Regex.IsMatch(text, @"\b(green\s*card|gc)\b", RegexOptions.IgnoreCase)) return "GC";
            if (Regex.IsMatch(text, @"\b(gcead|gc\s*ead)\b", RegexOptions.IgnoreCase)) return "GCEAD";
            if (Regex.IsMatch(text, @"\b(h1b|h-1b)\b", RegexOptions.IgnoreCase)) return "H1B";
            if (Regex.IsMatch(text, @"\b(opt\s*ead|opt)\b", RegexOptions.IgnoreCase)) return "OPT";
            if (Regex.IsMatch(text, @"\b(cpt)\b", RegexOptions.IgnoreCase)) return "CPT";
            if (Regex.IsMatch(text, @"\b(h4\s*ead|h4ead)\b", RegexOptions.IgnoreCase)) return "H4EAD";
            if (Regex.IsMatch(text, @"\b(l2\s*ead|l2ead)\b", RegexOptions.IgnoreCase)) return "L2EAD";
            if (Regex.IsMatch(text, @"\b(tn\s*visa|tn\s*status|tn)\b", RegexOptions.IgnoreCase)) return "TN";
            if (Regex.IsMatch(text, @"\b(ead)\b", RegexOptions.IgnoreCase)) return "EAD";
            return "ANY";
        }

        private static string ExtractLocationFromText(string text, out bool isRemote, out bool canRelocate)
        {
            isRemote = Regex.IsMatch(text, @"\b(remote|work\s*from\s*home|wfh)\b", RegexOptions.IgnoreCase);
            canRelocate = Regex.IsMatch(text, @"\b(relocat|open\s*to\s*relocate|nationwide|any\s*location)\b", RegexOptions.IgnoreCase);

            var locMatch = Regex.Match(text, @"\b([A-Za-z\s]{2,20}),\s*([A-Z]{2})\b");
            if (locMatch.Success)
            {
                return $"{locMatch.Groups[1].Value.Trim()}, {locMatch.Groups[2].Value.ToUpper()}";
            }

            if (isRemote) return "Remote";
            return "US";
        }

        private static string CleanPreambleAndSignatures(string rawBody)
        {
            if (string.IsNullOrWhiteSpace(rawBody)) return string.Empty;

            string cleaned = rawBody;
            cleaned = Regex.Replace(cleaned, @"(?m)^\s*-+\s*Forwarded message\s*-+\s*$", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?(?:unsubscri|mailing\s*list|prohirespowerhouse|fe_web_member|broadcast\s+requirements|update\s+your\s+contact).*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^\s*(?:hi|hello|dear|greetings|good\s+morning|good\s+day)\b.*$", "");
            cleaned = Regex.Replace(cleaned, @"(?im)^.*?(?:hope\s+you\s+are\s+doing\s+well|hope\s+this\s+email\s+finds\s+you\s+well).*$", "");
            return cleaned.Trim();
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            string text = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</p>", "\n\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</tr>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</td>", " | ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</li>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<[^>]+>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            return Regex.Replace(text, @"[ \t]+", " ").Trim();
        }
    }
}
