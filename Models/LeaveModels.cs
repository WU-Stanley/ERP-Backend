namespace WUIAM.Models
{
    public class LeaveType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // e.g., Annual Leave
        public int MaxDays { get; set; }
        public bool IsPaid { get; set; }
        public string? Description { get; set; }
        public string? ColorTag { get; set; }
        public bool IsActive { get; set; } = true;
        public bool? RequireDocument { get; set; }
        public Guid ApprovalFlowId { get; set; }
        public string? Gender { get; set; }
        public ApprovalFlow ApprovalFlow { get; set; } 
        public List<LeaveTypeVisibility> VisibilityRules { get; set; } = new();
    }

    public class ApprovalFlow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? VisibilityJson { get; set; } = string.Empty; // Optional: if approval flow has visibility scope
        public List<ApprovalStep> Steps { get; set; } = new();
    }

    public class LeaveTypeVisibility
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string VisibilityType { get; set; } // "ROLE", "DEPARTMENT", "EMPLOYMENT_TYPE"
        public string Value { get; set; } // e.g., "Academic", "HR", "FullTime"

        public LeaveType? LeaveType { get; set; }
    }

    public class ApprovalStep
    {
        public Guid Id { get; set; }
        public Guid ApprovalFlowId { get; set; }
        public int StepOrder { get; set; } // Step number
        public string ApproverType { get; set; } // MANAGER, ROLE, USER
        public string ApproverValue { get; set; } // Role name, UserId, etc.
        public string? ConditionJson { get; set; } // Optional dynamic conditions (JSON)

        public ApprovalFlow? ApprovalFlow { get; set; }
    }

    public class LeaveRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SupportDocument { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime AppliedAt { get; set; }

        public LeaveType? LeaveType { get; set; }
        public User? User { get; set; }
        public int TotalDays { get; set; }
    }

    public class LeaveRequestApproval
    {
        public Guid Id { get; set; }
        public Guid LeaveRequestId { get; set; }
        public Guid ApprovalStepId { get; set; }
        public Guid ApproverPersonId { get; set; }
        public string Status { get; set; } = "Pending";// Pending, Approved, Rejected
        public Guid? ActedByUserId { get; set; }
        public string? Comment { get; set; }
        public DateTime? DecisionAt { get; set; }
        public LeaveRequest? LeaveRequest { get; set; }
        public ApprovalStep? ApprovalStep { get; set; }
        public User? ApproverPerson { get; set; }
    }

    public class Leave
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public Guid LeaveRequestId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsCancelled { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public LeaveType? LeaveType { get; set; }
        public LeaveRequest? LeaveRequest { get; set; }
    }

    public class PublicHoliday
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } // e.g. New Year’s Day every Jan 1st
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LeaveBalance
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }

        public int TotalDays { get; set; }         // e.g. 20 days/year
        public int UsedDays { get; set; }          // Automatically synced
        public int RemainingDays { get; set; }     // Total - Used

        public DateTime ValidFrom { get; set; }    // Start of cycle
        public DateTime ValidTo { get; set; }      // End of cycle

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public LeaveType? LeaveType { get; set; }
    }
    public class ApprovalDelegation
    {
        public Guid Id { get; set; }

        public Guid ApproverPersonId { get; set; }  // 👈 updated name
        public Guid DelegatePersonId { get; set; }  // 👈 for clarity

        public Guid? ApprovalFlowId { get; set; }
        public Guid? ApprovalStepId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

        // Navigation
        public User? ApproverPerson { get; set; }
        public User? DelegatePerson { get; set; }
        public ApprovalFlow? ApprovalFlow { get; set; }
        public ApprovalStep? ApprovalStep { get; set; }
    }

    public class LeavePolicy
    {
        public Guid Id { get; set; }

        public Guid LeaveTypeId { get; set; }
        public LeaveType? LeaveType { get; set; }

        public Guid? EmploymentTypeId { get; set; } // e.g., "FullTime", "Contract" — optional
        public string? RoleName { get; set; }       // Optional: if some roles have custom entitlement

        public int AnnualEntitlement { get; set; } = 20;
        public bool IsAccrualBased { get; set; } = false;
        public double AccrualRatePerMonth { get; set; } = 1.67;

        public int MaxCarryOverDays { get; set; } = 0;
        public bool AllowNegativeBalance { get; set; } = false;
        public bool IncludePublicHolidays { get; set; } = false;
        public bool AllowBackdatedRequest { get; set; } = false;
        public int MaxDaysPerRequest { get; set; }
        public EmploymentType EmploymentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class StatusConstants
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

}
