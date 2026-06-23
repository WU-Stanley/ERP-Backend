using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Enums;

namespace WUIAM.SeedData
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(WUIAMDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Permissions (ensure all enum values exist)
            if (!context.Permissions.Any())
            {
                foreach (var perm in Enum.GetValues<Permissions>())
                {
                    var name = perm.ToString();
                    context.Permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = $"Permission to {name.Replace("Manage", "Manage ").Replace("View", "View ").Replace("Create", "Create ").Replace("Update", "Update ").Replace("Approve", "Approve ").Replace("Reject", "Reject ").Replace("Delete", "Delete ").Replace("Assign", "Assign ").Replace("Revoke", "Revoke ").Replace("Send", "Send ").Replace("Receive", "Receive ").Replace("Generate", "Generate ").Replace("Export", "Export ").Replace("Upload", "Upload ").Replace("Archive", "Archive ").Replace("Publish", "Publish ").Replace("Schedule", "Schedule ").Replace("Process", "Process ").Replace("Initiate", "Initiate ").Replace("Complete", "Complete ").Replace("Edit", "Edit ").Replace("Delete", "Delete ").Replace("Reset", "Reset ").Replace("Activate", "Activate ")}"
                    });
                }
                await context.SaveChangesAsync();
            }
            // Seed User Types
            if (!context.UserTypes.Any())
            {
                var userTypes = new (string Name, string Description)[]
                {
                    ("Super Admin", "Full system administrator"),
                    ("HR Admin", "Human Resources administrator"),
                    ("Department Head", "Head of a department"),
                    ("Staff", "Regular staff member"),
                    ("Student", "Enrolled student"),
                    ("Guest", "Guest/external user")
                };
                foreach (var (name, desc) in userTypes)
                {
                    context.UserTypes.Add(new UserType
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = desc,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }

            // Seed Employment Types
            if (!context.EmploymentTypes.Any())
            {
                var empTypes = new (string Name, string Description)[]
                {
                    ("FullTime", "Full-time permanent staff"),
                    ("Contract", "Contract-based employment"),
                    ("PartTime", "Part-time employment"),
                    ("Adjunct", "Adjunct/visiting staff"),
                    ("Intern", "Intern/trainee"),
                    ("Retired", "Retired")
                };
                foreach (var (name, desc) in empTypes)
                {
                    context.EmploymentTypes.Add(new EmploymentType
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = desc,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }

            // Seed Job Categories
            if (!context.JobCategories.Any())
            {
                var categories = new (string Name, string Description)[]
                {
                    ("Academic", "Teaching and research staff"),
                    ("Administrative", "Office and admin staff"),
                    ("Technical", "Technical and lab staff"),
                    ("Support Staff", "Maintenance and facility staff"),
                    ("Management", "Senior management and executives"),
                    ("Contractor", "External contractors"),
                    ("Intern", "Interns and trainees")
                };
                foreach (var (name, desc) in categories)
                {
                    context.JobCategories.Add(new JobCategory
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = desc
                    });
                }
                await context.SaveChangesAsync();
            }

            // Seed Roles (only if none exist)
            if (!context.Roles.Any())
            {
                var roles = new (string Name, string Description)[]
                {
                    ("Super Admin", "Full system access to all modules"),
                    ("HR Admin", "Human resources management access"),
                    ("Department Head", "Department-level management access"),
                    ("Staff", "Standard staff member access"),
                    ("Finance Officer", "Finance and payroll access"),
                    ("Registry Officer", "Registry and records access"),
                    ("Lecturer", "Academic staff access"),
                    ("Student", "Student portal access")
                };
                foreach (var (name, desc) in roles)
                {
                    context.Roles.Add(new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = desc
                    });
                }
                await context.SaveChangesAsync();
            }

            // Backfill employee codes if any are null/empty
            var employeesWithoutCode = await context.EmployeeDetails
                .Where(e => string.IsNullOrEmpty(e.EmployeeCode))
                .ToListAsync();

            if (employeesWithoutCode.Any())
            {
                var existingCodes = await context.EmployeeDetails
                    .Where(e => !string.IsNullOrEmpty(e.EmployeeCode) && e.EmployeeCode.StartsWith("WU-"))
                    .Select(e => e.EmployeeCode)
                    .ToListAsync();

                var maxNum = 0;
                foreach (var code in existingCodes)
                {
                    var numStr = code.Substring(3);
                    if (int.TryParse(numStr, out var val))
                    {
                        if (val > maxNum)
                        {
                            maxNum = val;
                        }
                    }
                }

                foreach (var employee in employeesWithoutCode)
                {
                    maxNum++;
                    employee.EmployeeCode = $"WU-{maxNum:D4}";
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
