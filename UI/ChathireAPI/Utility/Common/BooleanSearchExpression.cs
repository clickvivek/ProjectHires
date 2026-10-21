using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utility.Common
{
    public class BooleanSearchClause
    {
        public List<string> AndTerms { get; set; } = new List<string>();
        public List<string> NotTerms { get; set; } = new List<string>();
    }

    public class BooleanSearchExpression
    {
        public bool IsEmpty => OrClauses.Count == 0;
        public bool HasBooleanLogic { get; set; } = false;

        // Distinct positive terms to query database for candidate matching jobs
        public List<string> CandidateDbTerms { get; set; } = new List<string>();

        // (Clause 1: AndTerms & NotTerms) OR (Clause 2: AndTerms & NotTerms) ...
        public List<BooleanSearchClause> OrClauses { get; set; } = new List<BooleanSearchClause>();

        public static BooleanSearchExpression Parse(IEnumerable<string>? inputStrings)
        {
            var expr = new BooleanSearchExpression();
            if (inputStrings == null) return expr;

            var rawCombined = string.Join(" ", inputStrings.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (string.IsNullOrWhiteSpace(rawCombined)) return expr;

            // Check if string contains boolean operators or multiple space-separated words
            bool hasOr = Regex.IsMatch(rawCombined, @"\bOR\b|\|\|", RegexOptions.IgnoreCase);
            bool hasAnd = Regex.IsMatch(rawCombined, @"\bAND\b|&&", RegexOptions.IgnoreCase);
            bool hasNot = Regex.IsMatch(rawCombined, @"\bNOT\b", RegexOptions.IgnoreCase);
            bool hasQuotes = rawCombined.Contains("\"");
            bool hasMultipleWords = rawCombined.Contains(" ");

            // If it's a single word without quotes or operators, keep it simple
            if (!hasOr && !hasAnd && !hasNot && !hasQuotes && !hasMultipleWords)
            {
                expr.HasBooleanLogic = false;
                var singleClause = new BooleanSearchClause();
                singleClause.AndTerms.Add(rawCombined);
                expr.OrClauses.Add(singleClause);
                expr.CandidateDbTerms.Add(rawCombined);
                return expr;
            }

            // Treat multi-word queries (e.g. "fiori ui5") or explicit boolean queries as Boolean logic
            expr.HasBooleanLogic = true;

            // Split by OR
            var orParts = Regex.Split(rawCombined, @"\bOR\b|\|\|", RegexOptions.IgnoreCase);
            foreach (var orPart in orParts)
            {
                if (string.IsNullOrWhiteSpace(orPart)) continue;

                var clause = new BooleanSearchClause();

                // Split orPart into AND and NOT segments, extracting quoted phrases or individual words
                // Matches optional NOT/negation, quotes, or space-delimited words
                var tokens = Regex.Matches(orPart, @"(?<not>\bNOT\s+|-)?(?:""(?<quoted>[^""]+)""|(?<word>[^\s]+))", RegexOptions.IgnoreCase);

                bool nextIsNot = false;
                foreach (Match m in tokens)
                {
                    var val = m.Value.Trim();
                    if (val.Equals("AND", StringComparison.OrdinalIgnoreCase) || val.Equals("&&", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (val.Equals("NOT", StringComparison.OrdinalIgnoreCase))
                    {
                        nextIsNot = true;
                        continue;
                    }

                    // Extract actual token content
                    string term = "";
                    if (m.Groups["quoted"].Success && !string.IsNullOrWhiteSpace(m.Groups["quoted"].Value))
                    {
                        term = m.Groups["quoted"].Value.Trim();
                    }
                    else if (m.Groups["word"].Success && !string.IsNullOrWhiteSpace(m.Groups["word"].Value))
                    {
                        term = m.Groups["word"].Value.Trim();
                    }
                    else
                    {
                        term = val;
                    }

                    if (string.IsNullOrWhiteSpace(term)) continue;

                    // Clean punctuation at ends if not quoted (e.g. trailing commas)
                    if (!m.Groups["quoted"].Success)
                    {
                        term = term.Trim(',', ';', '.');
                        if (string.IsNullOrWhiteSpace(term)) continue;
                    }

                    // Handle NOT prefix if matched directly
                    if (m.Groups["not"].Success || nextIsNot || term.StartsWith("-"))
                    {
                        term = term.TrimStart('-');
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            clause.NotTerms.Add(term);
                        }
                        nextIsNot = false;
                    }
                    else
                    {
                        clause.AndTerms.Add(term);
                        if (!expr.CandidateDbTerms.Any(t => t.Equals(term, StringComparison.OrdinalIgnoreCase)))
                        {
                            expr.CandidateDbTerms.Add(term);
                        }
                    }
                }

                if (clause.AndTerms.Any() || clause.NotTerms.Any())
                {
                    expr.OrClauses.Add(clause);
                }
            }

            return expr;
        }

        public bool IsMatch(string fullJobText)
        {
            if (IsEmpty) return true;
            if (string.IsNullOrWhiteSpace(fullJobText)) return false;

            // Evaluates (Clause1 OR Clause2 OR ...)
            foreach (var clause in OrClauses)
            {
                bool clausePasses = true;

                // All AndTerms must be present
                foreach (var term in clause.AndTerms)
                {
                    if (!ContainsTerm(fullJobText, term))
                    {
                        clausePasses = false;
                        break;
                    }
                }

                if (!clausePasses) continue;

                // None of the NotTerms can be present
                foreach (var notTerm in clause.NotTerms)
                {
                    if (ContainsTerm(fullJobText, notTerm))
                    {
                        clausePasses = false;
                        break;
                    }
                }

                if (clausePasses) return true;
            }

            return false;
        }

        private static bool ContainsTerm(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return true;
            // Case-insensitive boundary or substring check
            // For single alphanumeric words, use word boundary; for phrases or symbols (e.g. C++, .NET), use substring
            if (Regex.IsMatch(term, @"^[a-zA-Z0-9]+$"))
            {
                return Regex.IsMatch(text, $@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase);
            }
            return text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
