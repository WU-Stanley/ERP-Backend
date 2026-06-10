using WUIAM.Models;

namespace WUIAM.DTOs
{
    public class LeaveRequestCreateDto
    {
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
    }


    public class ApprovalDecisionDto
    {
        public bool IsApproved { get; set; }
        public string Comment { get; set; }
    }

    public class CreateLeaveTypeDto
    {
        public required string Name { get; set; } // e.g., Annual Leave
        public int MaxDays { get; set; }
        public bool IsPaid { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Gender {  get; set; }
        public Guid ApprovalFlowId { get; set; } 
        public string? ColorTag { get;  set; }
        public bool? RequireDocument { get;  set; }
        public List<LeaveTypeVisibilityDto> VisibilityRules { get; set; } = [];
    }
public class LeaveTypeVisibilityDto
{
    public string VisibilityType { get; set; } = default!;
    public string Value { get; set; } = default!;
}

    public class CreateApprovalFlowDto
    {
        public required string Name { get; set; }
        // public Guid LeaveTypeId { get; set; }
        public bool IsActive { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ApprovalStep>? Steps { get; set; }
        public string? VisibilityJson { get; set; }
    }
    public class PublicHolidayDto
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } // e.g. New Year’s Day every Jan 1st
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

      public class ApprovalDelegationDto
    {
        public Guid? Id { get; set; }

        public Guid? ApproverPersonId { get; set; }  
        public Guid DelegatePersonId { get; set; } 

        public Guid? ApprovalFlowId { get; set; }
        public Guid? ApprovalStepId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

         
    }
public class LeavePolicyDto
{ 
    
    public Guid LeaveTypeId { get; set; }
        public Guid EmploymentTypeId { get; set; } // e.g., "FullTime", "Contract" — optional
    public string? RoleName { get; set; }       // Optional: if some roles have custom entitlement

    public int AnnualEntitlement { get; set; } = 20;
    public bool IsAccrualBased { get; set; } = false;
    public double AccrualRatePerMonth { get; set; } = 1.67;

    public int MaxCarryOverDays { get; set; } = 0;
    public bool AllowNegativeBalance { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
    public class LeaveRequestDto
    {
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SupportDocument { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime AppliedAt { get; set; }
       public int? TotalDays { get; set; }
        public LeaveType? LeaveType { get; set; }
        public User? User { get; set; }

        public Guid? Id { get; set; }
    }

}
