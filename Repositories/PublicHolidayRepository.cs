using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Repositories
{
    public class PublicHolidayRepository: IPublicHolidayRepository
    {
        private readonly WUIAMDbContext _context;

        public PublicHolidayRepository(WUIAMDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PublicHoliday>> GetAllAsync()
        {
            return await _context.PublicHolidays.ToListAsync();
        }

        public async Task<PublicHoliday?> GetByIdAsync(Guid id)
        {
            return await _context.PublicHolidays.FindAsync(id);
        }

        public async Task<PublicHoliday> CreateAsync(PublicHolidayDto publicHolidayDto)
        {
            var publicHoliday = new PublicHoliday
            {
                Date = publicHolidayDto.Date,
                Name = publicHolidayDto.Name,
                IsRecurring = publicHolidayDto.IsRecurring,
                Description = publicHolidayDto.Description,
                CreatedAt = DateTime.UtcNow
            };
            _context.PublicHolidays.Add(publicHoliday);
            await _context.SaveChangesAsync();
            return publicHoliday;
        }

        public async Task<PublicHoliday> UpdateAsync( PublicHoliday publicHoliday)
        {
          
            _context.PublicHolidays.Update(publicHoliday);
            await _context.SaveChangesAsync();
            return publicHoliday;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var publicHoliday = await GetByIdAsync(id);
            if (publicHoliday == null) return false;

            _context.PublicHolidays.Remove(publicHoliday);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}