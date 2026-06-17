using System;
using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required] // Replaced 'Unique' with 'Required' as a standard validation attribute  
        public string Token { get; set; }

        public Guid UserId { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public DateTime? RevokedOn { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? Browser { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public bool IsActive => RevokedOn == null && !IsExpired;
    }
}