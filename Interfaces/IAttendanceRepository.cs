using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<AttendanceRecord?> GetAttendanceAsync(Guid id);
        Task<AttendanceRecord?> GetAttendanceByDateAsync(Guid employeeId, DateTime date);
        Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendancesAsync(Guid employeeId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<AttendanceRecord>> GetAllAttendancesAsync(DateTime startDate, DateTime endDate);
        Task<AttendanceRecord> AddAttendanceAsync(AttendanceRecord record);
        Task<AttendanceRecord> UpdateAttendanceAsync(AttendanceRecord record);

        Task<AttendanceSettings?> GetSettingsAsync();
        Task<AttendanceSettings> SaveSettingsAsync(AttendanceSettings settings);
    }
}
