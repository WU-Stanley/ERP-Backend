using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly WUIAMDbContext _context;

        public AttendanceRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceRecord?> GetAttendanceAsync(Guid id)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<AttendanceRecord?> GetAttendanceByDateAsync(Guid employeeId, DateTime date)
        {
            var targetDate = date.Date;
            return await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == targetDate && !a.IsDeleted);
        }

        public async Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendancesAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAllAttendancesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<AttendanceRecord> AddAttendanceAsync(AttendanceRecord record)
        {
            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<AttendanceRecord> UpdateAttendanceAsync(AttendanceRecord record)
        {
            _context.AttendanceRecords.Update(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<AttendanceSettings?> GetSettingsAsync()
        {
            return await _context.AttendanceSettings.FirstOrDefaultAsync(s => !s.IsDeleted);
        }

        public async Task<AttendanceSettings> SaveSettingsAsync(AttendanceSettings settings)
        {
            var existing = await _context.AttendanceSettings.FirstOrDefaultAsync(s => !s.IsDeleted);
            if (existing == null)
            {
                _context.AttendanceSettings.Add(settings);
            }
            else
            {
                existing.EnableWebAttendance = settings.EnableWebAttendance;
                existing.AllowedWifiIps = settings.AllowedWifiIps;
                existing.StandardCheckInTime = settings.StandardCheckInTime;
                existing.StandardCheckOutTime = settings.StandardCheckOutTime;
                _context.AttendanceSettings.Update(existing);
            }
            await _context.SaveChangesAsync();
            return existing ?? settings;
        }
    }
}
