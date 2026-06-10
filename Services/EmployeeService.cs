using System;
using Microsoft.AspNetCore.Identity;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;
namespace WUIAM.Services
{

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAuthService _authService;
        private readonly IAuthRepository _authRepository;
        private readonly WUIAMDbContext _context;
        public EmployeeService(WUIAMDbContext wUIAMDbContext, IEmployeeRepository employeeRepo, IAuthService authService, IAuthRepository authRepository)
        {
            _employeeRepo = employeeRepo;
            _authService = authService;
            _authRepository = authRepository;
            _context = wUIAMDbContext;
        }

        public async Task<EmployeeDetails?> GetEmployeeProfileAsync(Guid employeeId)
            => await _employeeRepo.GetByIdAsync(employeeId);

        public async Task<EmployeeDetails?> GetEmployeeByUserIdAsync(Guid userId)
            => await _employeeRepo.GetByUserIdAsync(userId);

        public async Task<EmployeeDetails> CreateEmployeeAsync(CreateUserDto userDto)
        {
            if (userDto.DepartmentId == null)
                throw new ArgumentException("DepartmentId is required", nameof(userDto.DepartmentId));

            var employee = new EmployeeDetails
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.UserEmail,
                CreatedAt = DateTime.UtcNow
                // UserId will be set after user creation
            };

            var employment = new EmploymentDetails
            {
                DepartmentId = userDto.DepartmentId.Value,
                EmploymentTypeId = userDto.EmploymentTypeId,
                DateOfHire = DateTime.UtcNow,
                IsActive = true,
                JobTitle = userDto.JobTitle,
                JobCategoryId = userDto.JobCategoryId,
                Employee = employee // Establish relationship
            };
            // Use a transaction to ensure both user and employee are created successfully
            // If either fails, the transaction is rolled back

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Get Staff UserType
                var userTypes = await _authRepository.getUserTypes();
                var staff = userTypes.First(a => a.Name == "Staff");

                // Step 2: Create user account
                var foundUser = await _authService.getUserByEmailAsync(employee.Email);
                if (foundUser == null)
                {
                    var user = await _authService.RegisterAsync(new CreateUserDto
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        UserEmail = employee.Email,
                        UserTypeId = staff!.Id,
                        EmploymentTypeId = userDto.EmploymentTypeId,
                        Password = employee.FirstName, // ⚠️ You may want to improve this logic
                        SingleSignOnEnabled = userDto.SingleSignOnEnabled
                    });

                    // Step 3: Link employee with user
                    employee.UserId = user.Id;
                    employee.ProfilePicture = "default.png";
                }
                else
                {
                    // Step 3: Link employee with user
                    employee.UserId = foundUser.Id;
                    employee.ProfilePicture = "default.png";
                }               // Step 4: Persist employee
                var createdEmployee = await _employeeRepo.AddAsync(employee);

                // Step 5: Commit transaction
                await transaction.CommitAsync();

                return createdEmployee;
            }
            catch
            {
                // Rollback if anything fails
                await transaction.RollbackAsync();
                throw; // let the controller handle the error
            }
        }

        public async Task<EmployeeDetails> UpdateEmployeeAsync(EmployeeDetails employee)
            => await _employeeRepo.UpdateAsync(employee);

        public async Task<List<JobCategory>> GetJobCategoriesAsync()
            => await _employeeRepo.GetJobCategoriesAsync();
    }
}
