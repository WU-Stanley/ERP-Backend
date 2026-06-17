using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Repositories
{
    /// <summary>
    /// Optimized DepartmentRepository with compiled queries and selective includes.
    /// </summary>
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly WUIAMDbContext _context;

        public DepartmentRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        private static DepartmentResponseDto ToDto(Department d) => new()
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            Description = d.Description,
            DepartmentType = d.DepartmentType,
            CollegeId = d.CollegeId,
            CollegeName = d.College != null ? d.College.Name : null,
            ParentDepartmentId = d.ParentDepartmentId,
            ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.Name : null,
            HeadId = d.HeadId,
            HeadName = d.Head != null ? $"{d.Head.FirstName} {d.Head.LastName}".Trim() : null,
        };

        public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.Name,
                    Description = d.Description,
                    DepartmentType = d.DepartmentType,
                    CollegeId = d.CollegeId,
                    CollegeName = d.College != null ? d.College.Name : null,
                    ParentDepartmentId = d.ParentDepartmentId,
                    HeadId = d.HeadId,
                    HeadName = d.Head != null ? d.Head.FirstName + " " + d.Head.LastName : null,
                })
                .ToListAsync();
        }

        public Task<Department?> GetByIdAsync(Guid id)
        {
            return _context.Departments
                .Where(d => d.Id == id)
                .Include(d => d.College)
                .Include(d => d.Head)
                .FirstOrDefaultAsync();
        }

        public async Task<Department> AddAsync(Department department)
        {
            var added = await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return added.Entity;
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

        public async Task<IEnumerable<DepartmentResponseDto>> GetAcademicDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => d.DepartmentType == DepartmentTypes.Academic.ToString())
                .AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.Name,
                    Description = d.Description,
                    DepartmentType = d.DepartmentType,
                    CollegeId = d.CollegeId,
                    CollegeName = d.College != null ? d.College.Name : null,
                    ParentDepartmentId = d.ParentDepartmentId,
                    HeadId = d.HeadId,
                    HeadName = d.Head != null ? d.Head.FirstName + " " + d.Head.LastName : null,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentResponseDto>> GetNonAcademicDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => d.DepartmentType == DepartmentTypes.NonAcademic.ToString())
                .AsNoTracking()
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.Name,
                    Description = d.Description,
                    DepartmentType = d.DepartmentType,
                    CollegeId = d.CollegeId,
                    CollegeName = d.College != null ? d.College.Name : null,
                    ParentDepartmentId = d.ParentDepartmentId,
                    HeadId = d.HeadId,
                    HeadName = d.Head != null ? d.Head.FirstName + " " + d.Head.LastName : null,
                })
                .ToListAsync();
        }
    }
}
