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
        public bool IsActive => RevokedOn == null && !IsExpired;
    }
}