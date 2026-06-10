using System.ComponentModel.DataAnnotations;
namespace WUIAM.Enums
{
    public enum Roles
    {
        [Display(Name = "Super Administrator")]
        SuperAdmin,

        [Display(Name = "IT Administrator")]
        ITAdmin,
        [Display(Name = "Staff")]
        Staff,

        [Display(Name = "HR Manager")]
        HRManager,

        [Display(Name = "HR Director")]
        HRDirector,

        [Display(Name = "HR Officer")]
        HROfficer,

        [Display(Name = "Recruitment Officer")]
        Recruiter,

        [Display(Name = "Payroll Officer")]
        PayrollOfficer,

        [Display(Name = "Head of Department")]
        DepartmentHead,

        [Display(Name = "Line Manager")]
        LineManager,

        [Display(Name = "Employee")]
        Employee,

        [Display(Name = "Intern")]
        Intern,

        [Display(Name = "Contract Staff")]
        Contractor

    }

    public enum EmploymentTypes
    {
        [Display(Name = "Full Time")]
        FullTime,

        [Display(Name = "Contract")]
        Contract,

        [Display(Name = "Part Time")]
        PartTime,

        [Display(Name = "Internship")]
        Internship,

        [Display(Name = "Temporary")]
        Temporary
    }

    public enum EmploymentStatus
    {
        [Display(Name = "Active")]
        Active,

        [Display(Name = "On Leave")]
        OnLeave,

        [Display(Name = "Suspended")]
        Suspended,

        [Display(Name = "Terminated")]
        Terminated,

        [Display(Name = "Resigned")]
        Resigned,

        [Display(Name = "Retired")]
        Retired,

        [Display(Name = "On Probation")]
        OnProbation
    }
}