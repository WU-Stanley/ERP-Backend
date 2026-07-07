using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WUIAM.DTOs;

namespace WUIAM.Interfaces
{
    public interface IAttendanceService
    {
        Task<ApiResponse<AttendanceRecordDto>> CheckInAsync(CheckInRequestDto request);
        Task<ApiResponse<AttendanceRecordDto>> CheckOutAsync(CheckOutRequestDto request);
        Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetEmployeeAttendancesAsync(Guid employeeId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetAllAttendancesAsync(DateTime startDate, DateTime endDate);
        
        Task<ApiResponse<AttendanceSettingsDto>> GetSettingsAsync();
        Task<ApiResponse<AttendanceSettingsDto>> SaveSettingsAsync(AttendanceSettingsDto request);
    }
}
