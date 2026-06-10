using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WUIAM.Models;

namespace WUIAM.DTOs
{
    public class CreateUserDto
    {
        [StringLength(90)]
        [Unicode(false)]
        public string? UserName { get; set; }

        [StringLength(60)]
        [Unicode(false)]
        public string? FullName { get; set; }

        [StringLength(150)]
        [Unicode(false)]
        public required string UserEmail { get; set; }

        [Unicode(false)]
        public required string Password { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        public Guid? DepartmentId { get; set; }
        public Guid JobCategoryId { get; set; }
        public Guid UserTypeId { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required Guid EmploymentTypeId { get; set; }
        public bool SingleSignOnEnabled { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid UserTypeId { get; set; }
        public Guid DepartmentId { get; set; }
    }
    public class UserTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class EmploymentTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

