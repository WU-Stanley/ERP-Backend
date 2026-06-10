using System.ComponentModel.DataAnnotations;
using WUIAM.Enums;

namespace WUIAM.Models
{
    public class JobCategory
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. Academic, NonAcademic
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
    public class Department
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;   // e.g. "PHY", "ICT"
        public string Name { get; set; } = string.Empty;   // e.g. "Physics", "ICT"
        public string Description { get; set; } = string.Empty;

        // Type: Academic (Physics, Chemistry) or NonAcademic (Registry, ICT)
        public string DepartmentType { get; set; } = DepartmentTypes.Academic.ToString();
        // For Academic departments only
        public Guid? CollegeId { get; set; }
        public College? College { get; set; }

        // Hierarchy: Parent Department → SubDepartments
        public Guid? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }
        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        // Head of Department
        public Guid? HeadId { get; set; }
        public EmployeeDetails? Head { get; set; }

        // Employees
        public ICollection<EmploymentDetails> Employees { get; set; } = new List<EmploymentDetails>();
    }
    public class EmployeeDetails
    {
        [Key]
        public Guid EmployeeId { get; set; }

        // Personal Information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;

        // Contact Information
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Emergency Contact
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;

        // Financial Information
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;

        // Optional
        public string? ProfilePicture { get; set; }
        public Guid UserId { get; set; }

        // Navigation  
        public User? User { get; set; }
        public ICollection<EmploymentDetails> Employments { get; set; } = new List<EmploymentDetails>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }

    public class EmploymentDetails
    {

        [Key] public Guid EmploymentId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid DepartmentId { get; set; }

        // Job Info
        public string JobTitle { get; set; } = string.Empty;
        public string JobCategory { get; set; } = string.Empty; // Academic | NonAcademic
        public Guid EmploymentTypeId { get; set; }  // FullTime, Contract, Adjunct
        public string EmploymentStatus { get; set; } = "Active";   // Active, Suspended, Retired, Terminated
        public string GradeLevel { get; set; } = string.Empty;     // e.g., Lecturer II, Senior Officer

        // Dates
        public DateTime DateOfHire { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? ExitDate { get; set; }

        // Organization
        public Guid? SupervisorId { get; set; }
        public EmployeeDetails? Supervisor { get; set; }


        // Compensation
        public Guid SalaryStructureId { get; set; }
        public string Benefits { get; set; } = string.Empty;

        // HR History
        public string PromotionHistory { get; set; } = string.Empty;
        public string TransferHistory { get; set; } = string.Empty;

        // Navigation
        public EmployeeDetails Employee { get; set; } = null!;
        public Department Department { get; set; } = null!;
        public EmploymentType EmploymentType { get; set; } = null!;
        // Lifecycle
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public Guid JobCategoryId { get; set; }
    }

}