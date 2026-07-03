using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WUIAM.Models
{
    /// <summary>
    /// Represents an audit log entry tracking system actions (login, create, update, delete, etc.).
    /// </summary>
    public class AuditLog
    {
        /// <summary>Unique identifier for the audit log entry.</summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Type of action performed. Examples: Login, Logout, Create, Update, Delete, 
        /// RoleChange, PermissionChange, PasswordReset, etc.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>Foreign key to the user who performed the action (nullable for system actions).</summary>
        public Guid? UserId { get; set; }

        /// <summary>Foreign key to the admin who impersonated the user (nullable).</summary>
        public Guid? ImpersonatorId { get; set; }

        /// <summary>Navigation to the admin who impersonated the user.</summary>
        [ForeignKey("ImpersonatorId")]
        public User? Impersonator { get; set; }

        /// <summary>Navigation to the user who performed the action.</summary>
        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>
        /// Name of the entity that was affected by the action.
        /// Examples: "User", "Department", "LeaveRequest", "Role", etc.
        /// </summary>
        [StringLength(100)]
        public string? EntityName { get; set; }

        /// <summary>Unique identifier of the entity that was affected (nullable for non-entity actions).</summary>
        public Guid? EntityId { get; set; }

        /// <summary>Human-readable description of the action taken.</summary>
        [StringLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// JSON snapshot of the entity's values before the change.
        /// Populated for Update and Delete actions.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? OldValues { get; set; }

        /// <summary>
        /// JSON snapshot of the entity's values after the change.
        /// Populated for Create and Update actions.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? NewValues { get; set; }

        /// <summary>IP address from which the action was performed.</summary>
        [StringLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>User agent string of the client that performed the action.</summary>
        [StringLength(255)]
        public string? UserAgent { get; set; }

        /// <summary>Timestamp when the action was logged (UTC).</summary>
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
