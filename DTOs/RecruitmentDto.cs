using System.ComponentModel.DataAnnotations;

namespace WUIAM.DTOs
{
    // ==================== Job Posting DTOs ====================
    public class CreateJobPostingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Requirements { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; } = "Full-time";

        public Guid? DepartmentId { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class UpdateJobPostingDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Requirements { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public Guid? DepartmentId { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class JobPostingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = "Full-time";
        public string Status { get; set; } = "Draft";
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? Deadline { get; set; }
        public int ApplicationsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ==================== Application DTOs ====================
    public class CreateApplicationDto
    {
        [Required]
        [MaxLength(200)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(5000)]
        public string? CoverLetter { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public Guid? AssignedTo { get; set; }
    }

    public class ApplicationDto
    {
        public Guid Id { get; set; }
        public Guid JobPostingId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ResumeFilePath { get; set; }
        public string? CoverLetter { get; set; }
        public string Status { get; set; } = "New";
        public Guid? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? ScoredAt { get; set; }
        public decimal? OverallMatch { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ApplicationListDto
    {
        public List<ApplicationDto> Applications { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    // ==================== Application Score DTOs ====================
    public class ApplicationScoreDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public decimal TechnicalMatch { get; set; }
        public decimal CulturalFit { get; set; }
        public decimal EducationMatch { get; set; }
        public decimal ExperienceMatch { get; set; }
        public decimal OverallMatch { get; set; }
        public string? ScannedResumeText { get; set; }
        public string? AiAnalysisNotes { get; set; }
        public DateTime ScannedAt { get; set; }
    }

    // ==================== Interview DTOs ====================
    public class CreateInterviewDto
    {
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Phone Screen";

        [Required]
        public DateTime ScheduledFor { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class InterviewDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string Type { get; set; } = "Phone Screen";
        public DateTime ScheduledFor { get; set; }
        public string? MeetingLink { get; set; }
        public string? TeamsMeetingId { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ==================== Offer Letter DTOs ====================
    public class CreateOfferLetterDto
    {
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Salary { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [MaxLength(2000)]
        public string Benefits { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }
    }

    public class OfferLetterDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public string Benefits { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public DateTime? SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OfferResponseDto
    {
        [Required]
        [MaxLength(50)]
        public string Response { get; set; } = string.Empty; // Accept, Decline

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }

    // ==================== Query DTOs ====================
    public class CreateQueryDto
    {
        [Required]
        [MaxLength(3000)]
        public string Message { get; set; } = string.Empty;
    }

    public class QueryDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageFrom { get; set; } = "Applicant";
        public string? FromUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApplicantTrackingDto
    {
        public ApplicationDto Application { get; set; } = new();
        public List<InterviewDto> Interviews { get; set; } = new();
        public OfferLetterDto? OfferLetter { get; set; }
        public List<QueryDto> Queries { get; set; } = new();
    }

    // ==================== Recruitment Stats DTO ====================
    public class RecruitmentStatsDto
    {
        public int TotalJobPostings { get; set; }
        public int ActiveJobPostings { get; set; }
        public int TotalApplications { get; set; }
        public int ApplicationsByStatus { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
        public int ShortlistedCount { get; set; }
        public int InterviewCount { get; set; }
        public int OfferCount { get; set; }
        public int HiredCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal AverageMatchScore { get; set; }
        public List<DepartmentStatsDto> DepartmentBreakdown { get; set; } = new();
    }

    public class DepartmentStatsDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int ApplicationsCount { get; set; }
        public int ActiveJobs { get; set; }
    }

    // ==================== Public Stats DTO ====================
    public class PublicRecruitmentStatsDto
    {
        public int OpenPositions { get; set; }
        public int TotalApplications { get; set; }
        public List<PublicJobListingDto> FeaturedJobs { get; set; } = new();
        public string CompanyName { get; set; } = "Wigwe University";
    }

    public class PublicJobListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = "Full-time";
        public string? DepartmentName { get; set; }
        public DateTime? Deadline { get; set; }
        public int ApplicationsCount { get; set; }
    }
}
