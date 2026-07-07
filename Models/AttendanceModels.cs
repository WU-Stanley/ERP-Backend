using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WUIAM.Models
{
    public class AttendanceRecord : SoftDeleteEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeId { get; set; }

        public DateTime Date { get; set; } // The date of the attendance record

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string Status { get; set; } = "Absent"; // Present, Absent, Late, HalfDay

        public string? Notes { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; }
    }

    public class AttendanceSettings : SoftDeleteEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool EnableWebAttendance { get; set; } = true;

        public string? AllowedWifiIps { get; set; } // Comma-separated IP addresses or ranges
        
        public TimeSpan? StandardCheckInTime { get; set; } // To determine if "Late"
        public TimeSpan? StandardCheckOutTime { get; set; }
    }
}
