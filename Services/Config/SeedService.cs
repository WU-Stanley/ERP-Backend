using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WUIAM.Enums;
using WUIAM.Models;

namespace WUIAM.Services.Config.SeedService
{
    public class SeedService
    {
        private readonly WUIAMDbContext _context;

        public SeedService(WUIAMDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            // Seed Roles  
            if (!_context.Roles.Any())
            {
                var roleNames = Enum.GetNames(typeof(Roles));
                foreach (var role in roleNames)
                {
                    _context.Roles.Add(new()
                    {
                        Name = role,
                        Description = "The " + role + " role access"
                    });
                }
                _context.SaveChanges();
            }
            if (!_context.EmploymentTypes.Any())
            {
                var emtypes = Enum.GetNames(typeof(EmploymentTypes));
                foreach (var emtype in emtypes)
                {
                    _context.EmploymentTypes.Add(new()
                    {
                        Name = emtype,
                        Description = "The " + emtype + " employment type",
                        IsActive = true

                    });
                }
                _context.SaveChanges(true);
            }
            // Seed Departments  
            if (!_context.Departments.Any())
            {
                _context.Departments.Add(new()
                {
                    Name = "ICT",
                    Description = "Main ICT department",
                    HeadId = null,
                    DepartmentType = "NonAcademic"
                });

                _context.SaveChanges();
            }

            // Seed UserTypes  
            if (!_context.UserTypes.Any())
            {
                _context.UserTypes.AddRange(new List<UserType>
                   {
                       new() { Name = "Staff", Description = "Staff user" },
                       new() { Name = "Student", Description = "Regular student user" },
                       new() { Name = "Contract", Description = "Contract staff user" }
                   });
                _context.SaveChanges();
            }

            // Seed Permissions  
            if (!_context.Permissions.Any())
            {
                var permissionNames = Enum.GetNames(typeof(Permissions));
                foreach (var permissionName in permissionNames)
                {
                    _context.Permissions.Add(new()
                    {
                        Name = permissionName,
                        Description = "Permission to " + permissionName.ToLower()
                    });
                }
                _context.SaveChanges();
            }

            // Seed Admin User  
            if (!_context.Users.Any())
            {
                var adminUserTypeId = _context.UserTypes.FirstOrDefault()?.Id ?? Guid.NewGuid();
                var adminDeptId = _context.Departments.FirstOrDefault()?.Id ?? Guid.NewGuid();
                var employTypeId = _context.EmploymentTypes.FirstOrDefault()?.Id ?? Guid.NewGuid();

                var adminUser = new User
                {
                    UserName = "SuperAdmin",
                    UserEmail = "standevcode@gmail.com",
                    Password = PasswordUtilService.HashPassword("admin103#,"),
                    DateCreated = DateTime.Now,
                    IsDefault = true,
                    FirstName = "Administrator",
                    LastName = "WU",
                    UserTypeId = adminUserTypeId,
                    CreatedById = Guid.NewGuid(),
                    SingleSignOnEnabled = false,
                    SessionId = Guid.NewGuid().ToString(),
                    SessionTime = DateTime.Now,
                    TwoFactorEnabled = true,

                };
                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin Role to Admin User  
                var adminRole = _context.Roles.FirstOrDefault(r => r.Name == "Admin") ?? _context.Roles.First();
                _context.UserRoles.Add(new()
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    AssignedAt = DateTime.Now
                });
                _context.SaveChanges();

                // Assign AdminAccess Permission to Admin Role  
                var adminAccessPermission = _context.Permissions.FirstOrDefault(p => p.Name == Permissions.AdminAccess.ToString());
                if (adminAccessPermission != null && !_context.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == adminAccessPermission.Id))
                {
                    // _context.RolePermissions.Add(new()
                    // {
                    //     RoleId = adminRole.Id,
                    //     PermissionId = adminAccessPermission.Id,
                    //     GrantedAt = DateTime.Now
                    // });
                    // _context.UserPermissions.Add(new UserPermission { UserId = adminUser.Id, PermissionId = adminAccessPermission!.Id });
                    var permissions = _context.Permissions.ToList();
                    foreach (var item in permissions)
                    {
                        _context.RolePermissions.Add(new()
                        {
                            RoleId = adminRole.Id,
                            PermissionId = item.Id,
                            GrantedAt = DateTime.UtcNow
                        });
                    }
                    _context.SaveChanges();
                }

            }
        }
        public async Task SeedJobCategoriesAsync()
        {
            if (!_context.JobCategories.Any())
            {
                var jobCategories = new List<JobCategory>
                {
                    new() { Name = "Academic", Description = "Academic staff" },
                    new() { Name = "Non-Academic", Description = "Non-Academic staff" }
                };
                _context.JobCategories.AddRange(jobCategories);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SeedLeaveBalancesAsync()
        {
            var users = await _context.Users.ToListAsync();
            var policies = await _context.LeavePolicies.Include(lp => lp.LeaveType).ToListAsync();

            var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);

            foreach (var user in users)
            {
                foreach (var policy in policies)
                {
                    // Check if balance already exists
                    var existing = await _context.LeaveBalances.FirstOrDefaultAsync(lb =>
                        lb.UserId == user.Id &&
                        lb.LeaveTypeId == policy.LeaveTypeId &&
                        lb.ValidFrom == startOfYear);

                    if (existing != null) continue;

                    var balance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        LeaveTypeId = policy.LeaveTypeId,
                        TotalDays = policy.AnnualEntitlement,
                        UsedDays = 0,
                        RemainingDays = policy.AnnualEntitlement,
                        ValidFrom = startOfYear,
                        ValidTo = endOfYear,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.LeaveBalances.Add(balance);
                }
            }

            await _context.SaveChangesAsync();
        }

    }
}