using WUIAM.Repositories.IRepositories;
using WUIAM.Models;

public class LeaveDateCalculator : ILeaveDateCalculator
{
    private readonly IPublicHolidayRepository _publicHolidayRepository;

    public LeaveDateCalculator(IPublicHolidayRepository publicHolidayRepository)
    {
        _publicHolidayRepository = publicHolidayRepository;
    }

    public async Task<int> CalculateWorkingDaysAsync(DateTime startDate, DateTime endDate, bool includePublicHolidays = false)
    {
        if (endDate.Date < startDate.Date)
        {
            return 0;
        }

        var publicHolidays = includePublicHolidays
            ? await _publicHolidayRepository.GetAllAsync()
            : Enumerable.Empty<PublicHoliday>();

        int workingDays = 0;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                continue;
            }

            if (includePublicHolidays && IsPublicHoliday(date, publicHolidays))
            {
                continue;
            }

            workingDays++;
        }

        return workingDays;
    }

    private static bool IsPublicHoliday(DateTime date, IEnumerable<PublicHoliday> publicHolidays)
    {
        return publicHolidays.Any(holiday =>
            holiday.IsRecurring
                ? holiday.Date.Month == date.Month && holiday.Date.Day == date.Day
                : holiday.Date.Date == date.Date);
    }
}
