using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    public class InterviewSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "Phone Screen";

        public DateTime ScheduledFor { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(200)]
        public string? TeamsMeetingId { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled";

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InterviewInterviewer> Interviewers { get; set; } = new List<InterviewInterviewer>();
    }
}
