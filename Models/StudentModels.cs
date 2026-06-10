using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }
        public string MatricNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;

        // Academic Info
        public Guid ProgramId { get; set; }
        public AcademicProgram Program { get; set; } = null!;
        public int AdmissionYear { get; set; }
        public string Level { get; set; } = "100";   // 100, 200, 300...

        // Contact Info
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid CollegeId { get; set; }
        public College College { get; set; }
        // Status
        public string EnrollmentStatus { get; set; } = "Active"; // Active, Suspended, Graduated, Withdrawn
    }
    public class AcademicProgram
    {
        [Key]
        public Guid ProgramId { get; set; }
        public string Code { get; set; } = string.Empty;   // e.g. "BSC-PHY"
        public string Name { get; set; } = string.Empty;   // e.g. "B.Sc. Physics"
        public string Description { get; set; } = string.Empty;
        public int DurationYears { get; set; } = 4;

        public Guid CollegeId { get; set; }
        public College College { get; set; } = null!;

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
    public class College
    {
        [Key]
        public Guid CollegeId { get; set; }
        public string Code { get; set; } = string.Empty;   // e.g. "SCI", "ENG"
        public string Name { get; set; } = string.Empty;   // e.g. "College of Science"
        public string Description { get; set; } = string.Empty;

        // Leadership
        public Guid? DeanId { get; set; }
        public EmployeeDetails? Dean { get; set; }

        // Relations
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<AcademicProgram> Programs { get; set; } = new List<AcademicProgram>();
    }

}