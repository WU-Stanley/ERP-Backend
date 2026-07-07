using System;
using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    public class InterviewInterviewer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InterviewScheduleId { get; set; }
        public InterviewSchedule? InterviewSchedule { get; set; }

        public Guid? EmployeeId { get; set; }
        public EmployeeDetails? Employee { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Name { get; set; }

        // Feedback Fields
        [MaxLength(50)]
        public string FeedbackStatus { get; set; } = "Pending"; // Pending, Submitted

        public int? Rating { get; set; } // 1-5

        [MaxLength(2000)]
        public string? Comments { get; set; }

        [MaxLength(50)]
        public string? Recommendation { get; set; } // Hire, No Hire, Strong Hire

        public DateTime? SubmittedAt { get; set; }
    }
}
