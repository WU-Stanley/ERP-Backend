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
    }
}
