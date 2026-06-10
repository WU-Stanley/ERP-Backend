using WUIAM.DTOs;
using WUIAM.Models;

public interface IPublicHolidayRepository
{
    Task<IEnumerable<PublicHoliday>> GetAllAsync();
    Task<PublicHoliday> GetByIdAsync(Guid id);
    Task<PublicHoliday> CreateAsync(PublicHolidayDto publicHoliday);
    Task<PublicHoliday> UpdateAsync(PublicHoliday publicHoliday);
    Task<bool> DeleteAsync(Guid id);
}