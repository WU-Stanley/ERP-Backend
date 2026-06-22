using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    // ==================== Job Postings ====================
    public class JobPosting
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; } = "Full-time";

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Active, Closed, Archived

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Guid CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime? Deadline { get; set; }
        public int ApplicationsCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }

    // ==================== Applications ====================
    public class JobApplication
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid JobPostingId { get; set; }
        public JobPosting? JobPosting { get; set; }

        [Required, MaxLength(150)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        public string? ResumeFilePath { get; set; }

        [MaxLength(5000)]
        public string? CoverLetter { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "New"; // New, Under Review, Shortlisted, Interviewing, Offer, Rejected, Withdrawn

        public Guid? AssignedTo { get; set; }
        public User? AssignedToUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(50)]
        public string IctOnboardingStatus { get; set; } = "NotStarted";

        [MaxLength(200)]
        public string? MicrosoftUserPrincipalName { get; set; }

        [MaxLength(100)]
        public string? MicrosoftUserId { get; set; }

        public DateTime? MicrosoftAccountProvisionedAt { get; set; }
        public Guid? MicrosoftAccountProvisionedBy { get; set; }

        [MaxLength(2000)]
        public string? MicrosoftProvisioningError { get; set; }
    }

    // ==================== AI Resume Scores ====================
    public class ApplicationScore
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        public decimal TechnicalMatch { get; set; }
        public decimal CulturalFit { get; set; }
        public decimal EducationMatch { get; set; }
        public decimal ExperienceMatch { get; set; }
        public decimal OverallMatch { get; set; }

        [MaxLength(5000)]
        public string? ScannedResumeText { get; set; }

        [MaxLength(2000)]
        public string? AiAnalysisNotes { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }

    // ==================== Interviews ====================
    public class Interview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "Phone Screen"; // Phone Screen, Technical, HR, Panel, Final

        public DateTime ScheduledFor { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(200)]
        public string? TeamsMeetingId { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Cancelled, NoShow

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ==================== Offer Letters ====================
    public class OfferLetter
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Position { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        [MaxLength(int.MaxValue)]
        public string? Benefits { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Viewed, Accepted, Declined, Expired

        public DateTime StartDate { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SignedAt { get; set; }

        public Guid? SignedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? SignedName { get; set; }

        [MaxLength(int.MaxValue)]
        public string? SignatureData { get; set; }
    }

    // ==================== Queries ====================
    public class RecruitmentQuery
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        [Required, MaxLength(5000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(20)]
        public string MessageFrom { get; set; } = "Applicant"; // Applicant, HR

        public Guid? FromUserId { get; set; }
        public User? FromUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
