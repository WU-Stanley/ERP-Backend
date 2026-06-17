using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WUIAM.Models
{
    /// <summary>
    /// Represents a pending employee profile update request for admin review.
    /// </summary>
    public class EmployeeProfileUpdateRequest
    {
        /// <summary>Unique identifier for the update request.</summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>Foreign key to the employee whose profile is being updated.</summary>
        [Column("EmployeeID")]
        public Guid EmployeeId { get; set; }

        /// <summary>Foreign key to the user who submitted the request.</summary>
        [Column("RequestedByUserID")]
        public Guid RequestedByUserId { get; set; }

        /// <summary>JSON string of the employee's current profile values.</summary>
        [Column("CurrentValuesJson")]
        public string CurrentValuesJson { get; set; } = string.Empty;

        /// <summary>JSON string of the proposed new profile values.</summary>
        [Column("ProposedValuesJson")]
        public string ProposedValuesJson { get; set; } = string.Empty;

        /// <summary>Status: "Pending", "Approved", "Rejected".</summary>
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        /// <summary>Optional comment from the reviewer.</summary>
        [StringLength(500)]
        public string? Comment { get; set; }

        /// <summary>Foreign key to the user who reviewed the request.</summary>
        [Column("ReviewedByUserID")]
        public Guid? ReviewedByUserId { get; set; }

        /// <summary>Timestamp when the request was submitted.</summary>
        [Column("RequestedAt")]
        public DateTime RequestedAt { get; set; }

        /// <summary>Timestamp when the request was reviewed.</summary>
        [Column("ReviewedAt")]
        public DateTime? ReviewedAt { get; set; }

        /// <summary>Navigation to the employee.</summary>
        [ForeignKey("EmployeeId")]
        public EmployeeDetails? Employee { get; set; }
    }
}
