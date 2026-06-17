using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WUIAM.Interfaces;

namespace WUIAM.Services
{
    public class AiResumeScanningService : IAiResumeScanningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiResumeScanningService> _logger;

        public AiResumeScanningService(HttpClient httpClient, IConfiguration configuration, ILogger<AiResumeScanningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResumeScore> ScanResumeAsync(string resumeText, string jobRequirements)
        {
            // Check if AI is enabled
            var aiEnabled = _configuration.GetValue<bool>("AI:Enabled");
            if (!aiEnabled)
            {
                // Fallback: Rule-based scoring
                return RuleBasedScoring(resumeText, jobRequirements);
            }

            var apiKey = _configuration["AI:OpenAiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("OpenAI API key not configured. Using rule-based scoring.");
                return RuleBasedScoring(resumeText, jobRequirements);
            }

            try
            {
                var prompt = @"You are an expert HR resume scanner. Analyze the resume against the job requirements and provide a structured JSON response.

Job Requirements:
{jobRequirements}

Resume Text:
{resumeText}

Please respond with ONLY a valid JSON object in this exact format (no markdown, no code blocks):
{{
  ""technicalMatch"": <number 0-100>,
  ""culturalFit"": <number 0-100>,
  ""educationMatch"": <number 0-100>,
  ""experienceMatch"": <number 0-100>,
  ""overallMatch"": <number 0-100>,
  ""aiAnalysisNotes"": ""<brief analysis of strengths, weaknesses, and recommendations>""
}}

Rules:
- technicalMatch: How well the candidate's technical skills match the requirements
- culturalFit: How well the candidate's background aligns with company culture
- educationMatch: How well the candidate's education matches the requirements
- experienceMatch: How well the candidate's experience matches the requirements
- overallMatch: Weighted average of all scores
- Keep scores realistic and professional";

                prompt = prompt.Replace("{jobRequirements}", jobRequirements)
                               .Replace("{resumeText}", resumeText);

                var requestBody = new
                {
                    model = _configuration["AI:Model"] ?? "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a professional HR resume analyzer. Always respond with valid JSON only." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.2,
                    max_tokens = 1000
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync(_configuration["AI:Endpoint"], jsonContent);
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();
                var parsed = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseText);

                // Extract the content from the response
                var choices = parsed.GetProperty("choices");
                var content = choices[0].GetProperty("message").GetProperty("content").GetString();

                // Clean up the content (remove markdown code blocks if present)
                content = content?.Replace("```json", "").Replace("```", "").Trim();

                if (content != null)
                {
                    var score = System.Text.Json.JsonSerializer.Deserialize<ResumeScore>(content);
                    if (score != null)
                        return score;
                }

                _logger.LogWarning("AI response could not be parsed. Using rule-based fallback.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI service for resume scanning.");
            }

            // Fallback to rule-based scoring
            return RuleBasedScoring(resumeText, jobRequirements);
        }

        private ResumeScore RuleBasedScoring(string resumeText, string jobRequirements)
        {
            var resumeLower = resumeText.ToLower();
            var requirementsLower = jobRequirements.ToLower();

            // Extract key skills/requirements from job description
            var requiredSkills = ExtractKeywords(jobRequirements);
            var foundSkills = requiredSkills.Count(s => resumeLower.Contains(s));

            var technicalMatch = requiredSkills.Any()
                ? (foundSkills / (decimal)requiredSkills.Count) * 100m
                : 50m;

            // Education matching
            var educationKeywords = new[] { "bachelor", "master", "phd", "degree", "diploma", "certificate", "b.a.", "b.s.", "m.s.", "mba" };
            var educationFound = educationKeywords.Count(k => resumeLower.Contains(k));
            var educationMatch = Math.Min(educationFound * 15m, 100m);

            // Experience matching
            var experienceKeywords = new[] { "year", "years", "experience", "worked", "professional", "junior", "senior", "lead" };
            var experienceFound = experienceKeywords.Count(k => resumeLower.Contains(k));
            var experienceMatch = Math.Min(experienceFound * 10m, 100m);

            // Cultural fit (based on keywords suggesting soft skills)
            var cultureKeywords = new[] { "team", "collaborat", "leadership", "communicat", "problem solv", "adapt", "creative" };
            var cultureFound = cultureKeywords.Count(k => resumeLower.Contains(k));
            var culturalFit = Math.Min(cultureFound * 12m, 100m);

            // Overall match (weighted)
            var overallMatch = (technicalMatch * 0.35m) +
                              (educationMatch * 0.2m) +
                              (experienceMatch * 0.25m) +
                              (culturalFit * 0.2m);

            var analysisNotes = GenerateAnalysisNotes(foundSkills, requiredSkills.Count, educationFound, experienceFound, overallMatch);

            return new ResumeScore
            {
                TechnicalMatch = Math.Round(technicalMatch, 1),
                CulturalFit = Math.Round(culturalFit, 1),
                EducationMatch = Math.Round(educationMatch, 1),
                ExperienceMatch = Math.Round(experienceMatch, 1),
                OverallMatch = Math.Round(overallMatch, 1),
                AiAnalysisNotes = analysisNotes
            };
        }

        private HashSet<string> ExtractKeywords(string text)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var words = text.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '\n').ToArray();
            var cleanText = new string(words);
            var terms = cleanText.Split(new[] { ' ', '\n', ',', ';', '.', '(', ')', '-' }, StringSplitOptions.RemoveEmptyEntries);

            // Filter common words and add meaningful terms
            var stopWords = new[] { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "from", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "shall", "should", "may", "might", "can", "could", "this", "that", "these", "those", "it", "its", "not", "no", "nor", "as", "if", "than", "then", "so", "yet", "both", "either", "neither", "each", "every", "all", "any", "few", "more", "most", "other", "some", "such", "only", "own", "same", "also", "just", "about", "above", "after", "again", "before", "between", "through", "during", "until", "while", "what", "when", "where", "why", "how", "which", "who", "whom" };

            foreach (var term in terms)
            {
                if (term.Length > 3 && !stopWords.Contains(term.ToLower()))
                    keywords.Add(term.ToLower());
            }

            return keywords;
        }

        private string GenerateAnalysisNotes(int skillsFound, int totalSkills, int eduMatches, int expMatches, decimal overallMatch)
        {
            var notes = new StringBuilder();

            notes.Append($"Keyword match: {skillsFound}/{totalSkills} technical keywords found. ");

            if (overallMatch >= 80)
                notes.Append("Excellent match - highly recommended for interview. ");
            else if (overallMatch >= 60)
                notes.Append("Good match - worth considering for further review. ");
            else if (overallMatch >= 40)
                notes.Append("Moderate match - may need additional screening. ");
            else
                notes.Append("Low match - unlikely to meet requirements. ");

            if (eduMatches < 2)
                notes.Append("Education qualifications not clearly stated. ");

            if (expMatches < 2)
                notes.Append("Professional experience indicators are limited. ");

            return notes.ToString().TrimEnd();
        }
    }
}
