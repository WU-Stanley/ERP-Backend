using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Models;
using WUIAM.Interfaces;

namespace WUIAM.Services
{
    public class ExportService : IExportService
    {
        private readonly WUIAMDbContext _dbContext;

        public ExportService(WUIAMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<byte[]> ExportEmployeesCsvAsync()
        {
            var employees = await _dbContext.EmployeeDetails
                .Include(e => e.User)
                .Include(e => e.Employments)
                    .ThenInclude(emp => emp.Department)
                .AsNoTracking()
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Code,First Name,Last Name,Email,Gender,Job Title,Department,Employment Status");

            foreach (var emp in employees)
            {
                var activeEmployment = emp.Employments.FirstOrDefault(e => e.EmploymentStatus == "Active") ?? emp.Employments.FirstOrDefault();
                var deptName = activeEmployment?.Department?.Name;
                var jobTitle = activeEmployment?.JobTitle;
                var status = activeEmployment?.EmploymentStatus;

                sb.AppendLine($"{EscapeCsv(emp.EmployeeCode)},{EscapeCsv(emp.User?.FirstName)},{EscapeCsv(emp.User?.LastName)},{EscapeCsv(emp.Email)},{EscapeCsv(emp.Gender.ToString())},{EscapeCsv(jobTitle)},{EscapeCsv(deptName)},{EscapeCsv(status)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportDepartmentsCsvAsync()
        {
            var departments = await _dbContext.Departments
                .AsNoTracking()
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Department Code,Department Name,Department Type,Description");

            foreach (var dept in departments)
            {
                sb.AppendLine($"{EscapeCsv(dept.Code)},{EscapeCsv(dept.Name)},{EscapeCsv(dept.DepartmentType)},{EscapeCsv(dept.Description)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportLeaveRequestsCsvAsync()
        {
            var leaves = await _dbContext.LeaveRequests
                .Include(l => l.User)
                .Include(l => l.LeaveType)
                .AsNoTracking()
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Name,Leave Type,Start Date,End Date,Total Days,Status,Reason");

            foreach (var leave in leaves)
            {
                var employeeName = leave.User != null ? $"{leave.User.FirstName} {leave.User.LastName}" : "Unknown";
                var leaveTypeName = leave.LeaveType != null ? leave.LeaveType.Name : "Unknown";
                
                sb.AppendLine($"{EscapeCsv(employeeName)},{EscapeCsv(leaveTypeName)},{leave.StartDate:yyyy-MM-dd},{leave.EndDate:yyyy-MM-dd},{leave.TotalDays},{EscapeCsv(leave.Status)},{EscapeCsv(leave.Reason)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            
            // If field contains comma, quote, or newline, escape it
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
