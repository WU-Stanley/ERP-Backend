namespace WUIAM.Interfaces
{
    public interface IAiResumeScanningService
    {
        Task<ResumeScore> ScanResumeAsync(string resumeText, string jobRequirements);
    }

    public class ResumeScore
    {
        public decimal TechnicalMatch { get; set; }
        public decimal CulturalFit { get; set; }
        public decimal EducationMatch { get; set; }
        public decimal ExperienceMatch { get; set; }
        public decimal OverallMatch { get; set; }
        public string AiAnalysisNotes { get; set; } = string.Empty;
    }
}
