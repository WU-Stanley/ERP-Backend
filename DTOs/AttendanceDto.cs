using System;
using System.ComponentModel.DataAnnotations;

namespace WUIAM.DTOs
{
    public class AttendanceRecordDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "Absent";
        public string? Notes { get; set; }
    }

    public class AttendanceSettingsDto
    {
        public Guid Id { get; set; }
        public bool EnableWebAttendance { get; set; }
        public string? AllowedWifiIps { get; set; }
        public TimeSpan? StandardCheckInTime { get; set; }
        public TimeSpan? StandardCheckOutTime { get; set; }
    }

    public class CheckInRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        public string? IpAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class CheckOutRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
        public string? IpAddress { get; set; }
        public string? Notes { get; set; }
    }
}
