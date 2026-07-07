using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAuthRepository _authRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository, IAuthRepository authRepository)
        {
            _attendanceRepository = attendanceRepository;
            _authRepository = authRepository;
        }

        private bool IsIpAllowed(string? userIp, string? allowedIpsCsv)
        {
            if (string.IsNullOrWhiteSpace(allowedIpsCsv)) return true; // No restrictions
            if (string.IsNullOrWhiteSpace(userIp)) return false; // Missing IP when restrictions exist

            var allowedIps = allowedIpsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(ip => ip.Trim())
                                          .ToList();

            foreach (var allowedIp in allowedIps)
            {
                if (allowedIp.EndsWith("*"))
                {
                    var prefix = allowedIp.TrimEnd('*');
                    if (userIp.StartsWith(prefix)) return true;
                }
                else
                {
                    if (userIp == allowedIp) return true;
                }
            }

            return false;
        }

        public async Task<ApiResponse<AttendanceRecordDto>> CheckInAsync(CheckInRequestDto request)
        {
            var settings = await _attendanceRepository.GetSettingsAsync();
            if (settings != null && !settings.EnableWebAttendance)
            {
                return ApiResponse<AttendanceRecordDto>.Failure("Web attendance is currently disabled by administrators.");
            }

            if (settings != null && !string.IsNullOrWhiteSpace(settings.AllowedWifiIps))
            {
                if (!IsIpAllowed(request.IpAddress, settings.AllowedWifiIps))
                {
                    return ApiResponse<AttendanceRecordDto>.Failure("You must be connected to the company network to check in.");
                }
            }

            var employee = await _authRepository.FindUserByIdAsync(request.EmployeeId);
            if (employee == null) return ApiResponse<AttendanceRecordDto>.Failure("Employee not found.");

            var today = DateTime.UtcNow.Date;
            var existingRecord = await _attendanceRepository.GetAttendanceByDateAsync(request.EmployeeId, today);

            if (existingRecord != null && existingRecord.CheckInTime != null)
            {
                return ApiResponse<AttendanceRecordDto>.Failure("You have already checked in today.");
            }

            var checkInTime = DateTime.UtcNow;
            var status = "Present";

            if (settings?.StandardCheckInTime != null)
            {
                if (checkInTime.TimeOfDay > settings.StandardCheckInTime.Value)
                {
                    status = "Late";
                }
            }

            if (existingRecord == null)
            {
                var newRecord = new AttendanceRecord
                {
                    EmployeeId = request.EmployeeId,
                    Date = today,
                    CheckInTime = checkInTime,
                    Status = status,
                    Notes = request.Notes
                };
                await _attendanceRepository.AddAttendanceAsync(newRecord);
                return ApiResponse<AttendanceRecordDto>.Success("Checked in successfully", MapToDto(newRecord, employee.FullName));
            }
            else
            {
                existingRecord.CheckInTime = checkInTime;
                existingRecord.Status = status;
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    existingRecord.Notes += (string.IsNullOrEmpty(existingRecord.Notes) ? "" : " | ") + request.Notes;
                }
                await _attendanceRepository.UpdateAttendanceAsync(existingRecord);
                return ApiResponse<AttendanceRecordDto>.Success("Checked in successfully", MapToDto(existingRecord, employee.FullName));
            }
        }

        public async Task<ApiResponse<AttendanceRecordDto>> CheckOutAsync(CheckOutRequestDto request)
        {
            var settings = await _attendanceRepository.GetSettingsAsync();
            if (settings != null && !settings.EnableWebAttendance)
            {
                return ApiResponse<AttendanceRecordDto>.Failure("Web attendance is currently disabled by administrators.");
            }

            if (settings != null && !string.IsNullOrWhiteSpace(settings.AllowedWifiIps))
            {
                if (!IsIpAllowed(request.IpAddress, settings.AllowedWifiIps))
                {
                    return ApiResponse<AttendanceRecordDto>.Failure("You must be connected to the company network to check out.");
                }
            }

            var employee = await _authRepository.FindUserByIdAsync(request.EmployeeId);
            if (employee == null) return ApiResponse<AttendanceRecordDto>.Failure("Employee not found.");

            var today = DateTime.UtcNow.Date;
            var existingRecord = await _attendanceRepository.GetAttendanceByDateAsync(request.EmployeeId, today);

            if (existingRecord == null || existingRecord.CheckInTime == null)
            {
                return ApiResponse<AttendanceRecordDto>.Failure("You must check in before checking out.");
            }

            if (existingRecord.CheckOutTime != null)
            {
                return ApiResponse<AttendanceRecordDto>.Failure("You have already checked out today.");
            }

            existingRecord.CheckOutTime = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.Notes))
            {
                existingRecord.Notes += (string.IsNullOrEmpty(existingRecord.Notes) ? "" : " | ") + request.Notes;
            }

            await _attendanceRepository.UpdateAttendanceAsync(existingRecord);
            return ApiResponse<AttendanceRecordDto>.Success("Checked out successfully", MapToDto(existingRecord, employee.FullName));
        }

        public async Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetEmployeeAttendancesAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            var records = await _attendanceRepository.GetEmployeeAttendancesAsync(employeeId, startDate, endDate);
            var dtos = records.Select(r => MapToDto(r, r.Employee?.FullName));
            return ApiResponse<IEnumerable<AttendanceRecordDto>>.Success("Retrieved attendance records", dtos);
        }

        public async Task<ApiResponse<IEnumerable<AttendanceRecordDto>>> GetAllAttendancesAsync(DateTime startDate, DateTime endDate)
        {
            var records = await _attendanceRepository.GetAllAttendancesAsync(startDate, endDate);
            var dtos = records.Select(r => MapToDto(r, r.Employee?.FullName));
            return ApiResponse<IEnumerable<AttendanceRecordDto>>.Success("Retrieved all attendance records", dtos);
        }

        public async Task<ApiResponse<AttendanceSettingsDto>> GetSettingsAsync()
        {
            var settings = await _attendanceRepository.GetSettingsAsync();
            if (settings == null)
            {
                return ApiResponse<AttendanceSettingsDto>.Success("Settings retrieved", new AttendanceSettingsDto { EnableWebAttendance = true });
            }
            return ApiResponse<AttendanceSettingsDto>.Success("Settings retrieved", new AttendanceSettingsDto
            {
                Id = settings.Id,
                EnableWebAttendance = settings.EnableWebAttendance,
                AllowedWifiIps = settings.AllowedWifiIps,
                StandardCheckInTime = settings.StandardCheckInTime,
                StandardCheckOutTime = settings.StandardCheckOutTime
            });
        }

        public async Task<ApiResponse<AttendanceSettingsDto>> SaveSettingsAsync(AttendanceSettingsDto request)
        {
            var settings = new AttendanceSettings
            {
                Id = request.Id != Guid.Empty ? request.Id : Guid.NewGuid(),
                EnableWebAttendance = request.EnableWebAttendance,
                AllowedWifiIps = request.AllowedWifiIps,
                StandardCheckInTime = request.StandardCheckInTime,
                StandardCheckOutTime = request.StandardCheckOutTime
            };

            var saved = await _attendanceRepository.SaveSettingsAsync(settings);
            request.Id = saved.Id;
            return ApiResponse<AttendanceSettingsDto>.Success("Settings saved successfully", request);
        }

        private static AttendanceRecordDto MapToDto(AttendanceRecord record, string? employeeName)
        {
            return new AttendanceRecordDto
            {
                Id = record.Id,
                EmployeeId = record.EmployeeId,
                EmployeeName = employeeName,
                Date = record.Date,
                CheckInTime = record.CheckInTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                Notes = record.Notes
            };
        }
    }
}
