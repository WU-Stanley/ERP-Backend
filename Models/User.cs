using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using WUIAM.Models;
using Newtonsoft.Json;
namespace WUIAM
{
    public class User
    {

        [Key]
        public Guid Id { get; set; }

        [StringLength(90)]
        [Unicode(false)]
        public string? UserName { get; set; }

        [StringLength(90)]
        [Unicode(false)]
        public required string FirstName { get; set; }

        [StringLength(90)]
        [Unicode(false)]
        public required string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [Unicode(false)]
        public required string UserEmail { get; set; }

        [Required]
        [Unicode(false)]
        public required string Password { get; set; }

        public string? ResetPasswordToken { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public bool IsSuspended { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }

        public DateTime? DateLastLoggedIn { get; set; }

        [Column("CreatedByID")]
        public Guid CreatedById { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [Column("SingleSignOnEnabled")]
        public bool SingleSignOnEnabled { get; set; }

        [Column("SessionID")]
        [Unicode(false)]
        public string? SessionId { get; set; }

        public DateTime? SessionTime { get; set; }

        private UserType userType;
        public Guid UserTypeId { get; set; }
        [ForeignKey("UserTypeId")]
        public UserType UserType { get => userType; set => userType = value; }

        public bool TwoFactorEnabled { get; set; } = true;

        [JsonIgnore]
        public List<MFAToken> MFATokens { get; set; } = new List<MFAToken>();
        public EmployeeDetails? Employee { get; set; }

        public Student? Student { get; set; }
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    }

    public class EmploymentType
    {
        [Key]
        public Guid Id { get; set; }
        public required string Name { get; set; }       // e.g., "FullTime", "Contract", "Adjunct"
        public string Description { get; set; } = string.Empty; // Optional: e.g., "Full-time permanent staff"
        public bool IsActive { get; set; } = true;
    }

}
