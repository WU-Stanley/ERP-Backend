using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IPublicHolidayService
    {
        Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync();
        Task<PublicHoliday> GetPublicHolidayByIdAsync(Guid id);
        Task<PublicHoliday> CreatePublicHolidayAsync(PublicHolidayDto publicHoliday);
        Task<PublicHoliday> UpdatePublicHolidayAsync(Guid id, PublicHolidayDto publicHoliday);
        Task<bool> DeletePublicHolidayAsync(Guid id);
    }
}
