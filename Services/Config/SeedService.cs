using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WUIAM.Enums;
using WUIAM.Models;

namespace WUIAM.Services.Config.SeedService
{
    public class SeedService
    {
        private const string SuperAdminEmail = "standevcode@gmail.com";
        private const string SuperAdminRoleName = "SuperAdmin";

        private readonly WUIAMDbContext _context;

        public SeedService(WUIAMDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            // Seed Roles  
            var roleNames = Enum.GetNames(typeof(Roles));
            foreach (var role in roleNames)
            {
                if (!_context.Roles.Any(existing => existing.Name == role))
                {
                    _context.Roles.Add(new Role
                    {
                        Name = role,
                        Description = "The " + role + " role access"
                    });
                }
            }
            _context.SaveChanges();

            if (!_context.EmploymentTypes.Any())
            {
                var emtypes = Enum.GetNames(typeof(EmploymentTypes));
                foreach (var emtype in emtypes)
                {
                    _context.EmploymentTypes.Add(new EmploymentType
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
                _context.Departments.Add(new Department
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
                    new UserType { Name = "Staff", Description = "Staff user" },
                    new UserType { Name = "Student", Description = "Regular student user" },
                    new UserType { Name = "Contract", Description = "Contract staff user" }
                });
                _context.SaveChanges();
            }

            // Seed Permissions  
            var permissionNames = Enum.GetNames(typeof(Permissions));
            foreach (var permissionName in permissionNames)
            {
                if (!_context.Permissions.Any(existing => existing.Name == permissionName))
                {
                    _context.Permissions.Add(new Permission
                    {
                        Name = permissionName,
                        Description = "Permission to " + permissionName.ToLower()
                    });
                }
            }
            _context.SaveChanges();

            EnsureSuperAdminAccess();
        }

        private void EnsureSuperAdminAccess()
        {
            var superAdminUserType = _context.UserTypes.FirstOrDefault(type =>
                type.Name == "Super Admin" || type.Name == "Staff") ?? _context.UserTypes.First();

            var superAdminRole = _context.Roles.FirstOrDefault(role => role.Name == SuperAdminRoleName)
                ?? _context.Roles.FirstOrDefault(role => role.Name == "Super Admin");

            if (superAdminRole == null)
            {
                superAdminRole = new Role
                {
                    Name = SuperAdminRoleName,
                    Description = "Full system access to all modules"
                };
                _context.Roles.Add(superAdminRole);
                _context.SaveChanges();
            }

            var adminUser = _context.Users.FirstOrDefault(user => user.UserEmail == SuperAdminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "SuperAdmin",
                    UserEmail = SuperAdminEmail,
                    Password = PasswordUtilService.HashPassword("admin103#,"),
                    DateCreated = DateTime.Now,
                    IsDefault = true,
                    IsActive = true,
                    FirstName = "Administrator",
                    LastName = "WU",
                    UserTypeId = superAdminUserType.Id,
                    CreatedById = Guid.NewGuid(),
                    SingleSignOnEnabled = false,
                    SessionId = Guid.NewGuid().ToString(),
                    SessionTime = DateTime.Now,
                    TwoFactorEnabled = true,
                };
                _context.Users.Add(adminUser);
                _context.SaveChanges();
            }
            else
            {
                adminUser.UserName = "SuperAdmin";
                adminUser.IsActive = true;
                adminUser.IsDeleted = false;
                adminUser.UserTypeId = superAdminUserType.Id;
                _context.Users.Update(adminUser);
                _context.SaveChanges();
            }

            if (!_context.UserRoles.Any(userRole => userRole.UserId == adminUser.Id && userRole.RoleId == superAdminRole.Id))
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id,
                    AssignedAt = DateTime.UtcNow
                });
                _context.SaveChanges();
            }

            var permissions = _context.Permissions.ToList();
            foreach (var permission in permissions)
            {
                if (!_context.RolePermissions.Any(rolePermission =>
                    rolePermission.RoleId == superAdminRole.Id &&
                    rolePermission.PermissionId == permission.Id))
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow
                    });
                }
            }

            var superAdminAccess = _context.Permissions.FirstOrDefault(permission =>
                permission.Name == Permissions.SuperAdminAccess.ToString());
            if (superAdminAccess != null &&
                !_context.UserPermissions.Any(userPermission =>
                    userPermission.UserId == adminUser.Id &&
                    userPermission.PermissionId == superAdminAccess.Id))
            {
                _context.UserPermissions.Add(new UserPermission
                {
                    UserId = adminUser.Id,
                    PermissionId = superAdminAccess.Id
                });
            }

            _context.SaveChanges();
        }
        public async Task SeedJobCategoriesAsync()
        {
            if (!_context.JobCategories.Any())
            {
                var jobCategories = new List<JobCategory>
                {
                    new JobCategory { Name = "Academic", Description = "Academic staff" },
                    new JobCategory { Name = "Non-Academic", Description = "Non-Academic staff" }
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
