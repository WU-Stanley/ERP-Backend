using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    /// <summary>
    /// Optimized EmployeeRepository with selective includes and compiled queries.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly WUIAMDbContext _context;

        // Compiled query: get by user ID
        private static readonly Func<WUIAMDbContext, Guid, Task<EmployeeDetails?>> _getByUserId =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid userId) =>
                db.EmployeeDetails
                    .Where(e => e.UserId == userId)
                    .Include(e => e.User)
                    .Include(e => e.Employments.Where(e => e.IsActive))
                        .ThenInclude(ed => ed.Department)
                    .FirstOrDefault());

        // Compiled query: get current employment
        private static readonly Func<WUIAMDbContext, Guid, Task<EmploymentDetails?>> _getCurrentEmployment =
            EF.CompileAsyncQuery((WUIAMDbContext db, Guid employeeId) =>
                db.EmploymentDetails
                    .Where(ed => ed.EmployeeId == employeeId && ed.IsActive)
                    .Include(ed => ed.Department)
                    .FirstOrDefault());

        public EmployeeRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        // -----------------------
        // EmployeeDetails
        // -----------------------

        public async Task<EmployeeDetails?> GetByIdAsync(Guid employeeId)
        {
            // Optimized: project to avoid loading full User graph
            return await _context.EmployeeDetails
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => new EmployeeDetails
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    MiddleName = e.MiddleName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    Address = e.Address,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    MaritalStatus = e.MaritalStatus,
                    Nationality = e.Nationality,
                    EmergencyContactName = e.EmergencyContactName,
                    EmergencyContactPhone = e.EmergencyContactPhone,
                    Relationship = e.Relationship,
                    BankName = e.BankName,
                    BankAccountNumber = e.BankAccountNumber,
                    ProfilePicture = e.ProfilePicture,
                    UserId = e.UserId,
                    User = e.User != null ? new User
                    {
                        Id = e.User.Id,
                        FirstName = e.User.FirstName,
                        LastName = e.User.LastName,
                        UserEmail = e.User.UserEmail,
                        UserName = e.User.UserName,
                        Password = e.User.Password,
                    } : null,
                    Employments = e.Employments
                        .Select(ed => new EmploymentDetails
                        {
                            EmploymentId = ed.EmploymentId,
                            EmployeeId = ed.EmployeeId,
                            DepartmentId = ed.DepartmentId,
                            JobTitle = ed.JobTitle,
                            JobCategory = ed.JobCategory,
                            EmploymentTypeId = ed.EmploymentTypeId,
                            EmploymentStatus = ed.EmploymentStatus,
                            GradeLevel = ed.GradeLevel,
                            DateOfHire = ed.DateOfHire,
                            ProbationEndDate = ed.ProbationEndDate,
                            ExitDate = ed.ExitDate,
                            SupervisorId = ed.SupervisorId,
                            SalaryStructureId = ed.SalaryStructureId,
                            Benefits = ed.Benefits,
                            PromotionHistory = ed.PromotionHistory,
                            TransferHistory = ed.TransferHistory,
                            StartDate = ed.StartDate,
                            EndDate = ed.EndDate,
                            IsActive = ed.IsActive,
                            JobCategoryId = ed.JobCategoryId,
                            Department = ed.Department != null ? new Department
                            {
                                Id = ed.Department.Id,
                                Name = ed.Department.Name,
                                Code = ed.Department.Code,
                                DepartmentType = ed.Department.DepartmentType,
                            } : null,
                            EmploymentType = ed.EmploymentType != null ? new EmploymentType
                            {
                                Id = ed.EmploymentType.Id,
                                Name = ed.EmploymentType.Name,
                            } : null,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EmployeeDetails>> GetAllAsync()
        {
            // Optimized: project to DTO instead of loading full entity graph
            return await _context.EmployeeDetails
                .Select(e => new EmployeeDetails
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    MiddleName = e.MiddleName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    Address = e.Address,
                    DateOfBirth = e.DateOfBirth,
                    Gender = e.Gender,
                    MaritalStatus = e.MaritalStatus,
                    User = e.User != null ? new User
                    {
                        Id = e.User.Id,
                        FirstName = e.User.FirstName,
                        LastName = e.User.LastName,
                        UserEmail = e.User.UserEmail,
                        UserName = e.User.UserName,
                        Password = e.User.Password,
                    } : null,
                    Employments = e.Employments
                        .Where(ed => ed.IsActive)
                        .Select(ed => new EmploymentDetails
                        {
                            EmploymentId = ed.EmploymentId,
                            EmployeeId = ed.EmployeeId,
                            DepartmentId = ed.DepartmentId,
                            SupervisorId = ed.SupervisorId,
                            EmploymentTypeId = ed.EmploymentTypeId,
                            JobTitle = ed.JobTitle,
                            DateOfHire = ed.DateOfHire,
                            EmploymentStatus = ed.EmploymentStatus,
                            IsActive = ed.IsActive,
                            Department = ed.Department != null ? new Department
                            {
                                Id = ed.Department.Id,
                                Name = ed.Department.Name,
                                Code = ed.Department.Code,
                            } : null,
                            EmploymentType = ed.EmploymentType != null ? new EmploymentType
                            {
                                Id = ed.EmploymentType.Id,
                                Name = ed.EmploymentType.Name,
                            } : null,
                        })
                        .ToList(),
                })
                .ToListAsync();
        }

        public async Task<EmployeeDetails> AddAsync(EmployeeDetails employee)
        {
            _context.EmployeeDetails.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<EmployeeDetails> UpdateAsync(EmployeeDetails employee)
        {
            var existing = await _context.EmployeeDetails
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Employee with ID {employee.EmployeeId} not found");
            }

            // Copy Personal Information
            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.MiddleName = employee.MiddleName;
            existing.DateOfBirth = employee.DateOfBirth;
            existing.Gender = employee.Gender;
            existing.MaritalStatus = employee.MaritalStatus;
            existing.Nationality = employee.Nationality;

            // Copy Contact Information
            existing.Address = employee.Address;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.Email = employee.Email;

            // Copy Emergency Contact
            existing.EmergencyContactName = employee.EmergencyContactName;
            existing.EmergencyContactPhone = employee.EmergencyContactPhone;
            existing.Relationship = employee.Relationship;

            // Copy Financial Information
            existing.BankName = employee.BankName;
            existing.BankAccountNumber = employee.BankAccountNumber;
            existing.ProfilePicture = employee.ProfilePicture;
            existing.UpdatedAt = DateTime.UtcNow;

            // Sync User details
            if (existing.User != null)
            {
                existing.User.FirstName = employee.FirstName;
                existing.User.LastName = employee.LastName;
                existing.User.UserEmail = employee.Email;
                existing.User.UserName = employee.Email;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<EmployeeDetails> DeleteAsync(Guid employeeId)
        {
            var employee = await _context.EmployeeDetails.FindAsync(employeeId);
            if (employee != null)
            {
                _context.EmployeeDetails.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return employee!;
        }

        // -----------------------
        // EmploymentDetails
        // -----------------------

        public async Task<IEnumerable<EmploymentDetails>> GetEmploymentsAsync(Guid employeeId)
        {
            return await _context.EmploymentDetails
                .Where(ed => ed.EmployeeId == employeeId)
                .Select(ed => new EmploymentDetails
                {
                    EmploymentId = ed.EmploymentId,
                    EmployeeId = ed.EmployeeId,
                    DepartmentId = ed.DepartmentId,
                    SupervisorId = ed.SupervisorId,
                    EmploymentTypeId = ed.EmploymentTypeId,
                    Department = ed.Department != null ? new Department
                    {
                        Id = ed.Department.Id,
                        Name = ed.Department.Name,
                        Code = ed.Department.Code,
                    } : null,
                    EmploymentType = ed.EmploymentType != null ? new EmploymentType
                    {
                        Id = ed.EmploymentType.Id,
                        Name = ed.EmploymentType.Name,
                    } : null,
                })
                .ToListAsync();
        }

        public Task<EmploymentDetails?> GetCurrentEmploymentAsync(Guid employeeId)
        {
            return _getCurrentEmployment(_context, employeeId);
        }

        // -----------------------
        // With User
        // -----------------------

        public async Task<EmployeeDetails?> GetByUserIdAsync(Guid userId)
        {
            return await _context.EmployeeDetails
                .Where(e => e.UserId == userId)
                .Include(e => e.User)
                .Include(e => e.Employments)
                    .ThenInclude(ed => ed.Department)
                .Include(e => e.Employments)
                    .ThenInclude(ed => ed.EmploymentType)
                .FirstOrDefaultAsync();
        }

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
        {
            return await _context.JobCategories.ToListAsync();
        }
    }
}
