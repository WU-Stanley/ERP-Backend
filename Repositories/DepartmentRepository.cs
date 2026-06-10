using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Enums;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{


    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly WUIAMDbContext _context;

        public DepartmentRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task<Department> AddAsync(Department department)
        {
            var added = _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return added.Result.Entity;
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Department>> GetAcademicDepartmentsAsync()
        {
            return await _context.Departments.Where(e => e.DepartmentType == DepartmentTypes.Academic.ToString()).ToListAsync();
        }
        public async Task<IEnumerable<Department>> GetNonAcademicDepartmentsAsync()
        {
            return await _context.Departments.Where(e => e.DepartmentType == DepartmentTypes.NonAcademic.ToString()).ToListAsync();
        }
    }
}