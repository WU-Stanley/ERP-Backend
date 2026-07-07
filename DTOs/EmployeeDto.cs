using System;
using System.Collections.Generic;
using WUIAM.Enums;

namespace WUIAM.DTOs
{
    /// <summary>
    /// Editable employee profile fields. Server-managed fields such as
    /// EmployeeCode, UserId and audit timestamps are intentionally excluded.
    /// </summary>
    public class EmployeeUpdateDto
    {
        public Guid EmployeeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
    }

    /// <summary>
    /// DTO for row-level errors during bulk staff upload.
    /// </summary>
    public class BulkStaffUploadRowErrorDto
    {
        public int RowNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for bulk staff upload result summary.
    /// </summary>
    public class BulkStaffUploadResultDto
    {
        public int TotalRows { get; set; }
        public int CreatedRows { get; set; }
        public int FailedRows { get; set; }
        public List<BulkStaffUploadRowErrorDto> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO for employee directory search results.
    /// </summary>
    public class EmployeeDirectoryDto
    {
        public Guid EmployeeId { get; set; }
        public Guid UserId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string EmploymentTypeName { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public DateTime? DateOfHire { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for employee self-service profile updates.
    /// </summary>
    public class EmployeeSelfServiceUpdateDto
    {
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string? CvUrl { get; set; }
        public string? IdentificationUrl { get; set; }
        public string? CertificateUrl { get; set; }
    }

    /// <summary>
    /// DTO for pending employee profile update requests.
    /// </summary>
    public class EmployeeProfileUpdateRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public EmployeeSelfServiceUpdateDto CurrentValues { get; set; } = new();
        public EmployeeSelfServiceUpdateDto ProposedValues { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    /// <summary>
    /// DTO for approving or rejecting a profile update request.
    /// </summary>
    public class ProfileUpdateDecisionDto
    {
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for assigning employment details to an employee.
    /// </summary>
    public class EmploymentAssignmentDto
    {
        public Guid DepartmentId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public Guid EmploymentTypeId { get; set; }
        public string EmploymentStatus { get; set; } = "Active";
        public string GradeLevel { get; set; } = string.Empty;
        public DateTime DateOfHire { get; set; } = DateTime.UtcNow;
        public DateTime? ProbationEndDate { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public string Benefits { get; set; } = string.Empty;
        public string PromotionHistory { get; set; } = string.Empty;
        public string TransferHistory { get; set; } = string.Empty;
        public Guid? JobCategoryId { get; set; }
    }
}
