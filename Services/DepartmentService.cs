using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly WUIAMDbContext _dbContext;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public DepartmentService(IDepartmentRepository departmentRepository, WUIAMDbContext dbContext, IMemoryCache cache)
        {
            _departmentRepository = departmentRepository;
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
        {
            var cacheKey = "departments_all";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<DepartmentResponseDto>? cachedDepartments) && cachedDepartments != null)
                return cachedDepartments;

            var departments = await _departmentRepository.GetAllAsync();

            _cache.Set(cacheKey, departments, _cacheDuration);
            return departments;
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            return department;
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            var result = await _departmentRepository.AddAsync(department);
            _cache.Remove("departments_all");
            _cache.Remove("departments_academic");
            _cache.Remove("departments_nonacademic");
            return result;
        }

        public async Task<Department?> UpdateDepartmentAsync(Guid id, Department department)
        {
            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null) return null;
            department.Id = id;
            await _departmentRepository.UpdateAsync(department);
            _cache.Remove("departments_all");
            _cache.Remove("departments_academic");
            _cache.Remove("departments_nonacademic");
            return department;
        }

        public async Task<Department?> DeleteDepartmentAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            await _departmentRepository.DeleteAsync(id);
            _cache.Remove("departments_all");
            _cache.Remove("departments_academic");
            _cache.Remove("departments_nonacademic");
            return department;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetNonAcademicDepartmentsAsync()
        {
            var cacheKey = "departments_nonacademic";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<DepartmentResponseDto>? cachedDepartments) && cachedDepartments != null)
                return cachedDepartments;

            var departments = await _departmentRepository.GetNonAcademicDepartmentsAsync();

            _cache.Set(cacheKey, departments, _cacheDuration);
            return departments;
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetAcademicDepartmentsAsync()
        {
            var cacheKey = "departments_academic";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<DepartmentResponseDto>? cachedDepartments) && cachedDepartments != null)
                return cachedDepartments;

            var departments = await _departmentRepository.GetAcademicDepartmentsAsync();

            _cache.Set(cacheKey, departments, _cacheDuration);
            return departments;
        }

        public async Task<bool> EmployeeExistsAsync(Guid employeeId)
        {
            return await _dbContext.EmployeeDetails.AnyAsync(employee => employee.EmployeeId == employeeId);
        }
    }
}
