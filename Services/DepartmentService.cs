using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _departmentRepository.GetAllAsync();
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _departmentRepository.GetByIdAsync(id);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            return await _departmentRepository.AddAsync(department);
        }

        public async Task<Department?> UpdateDepartmentAsync(Guid id, Department department)
        {
            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null) return null;
            department.Id = id; // ensure consistency
            await _departmentRepository.UpdateAsync(department);
            return department;
        }

        public async Task<Department?> DeleteDepartmentAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            await _departmentRepository.DeleteAsync(id);
            return department;
        }


        public async Task<IEnumerable<Department>> GetNonAcademicDepartmentsAsync()
        {
            return await _departmentRepository.GetNonAcademicDepartmentsAsync();
        }

        public async Task<IEnumerable<Department>> GetAcademicDepartmentsAsync()
        {
            return await _departmentRepository.GetAcademicDepartmentsAsync();
        }
    }
}
