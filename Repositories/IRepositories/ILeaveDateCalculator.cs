
namespace WUIAM.Repositories.IRepositories
{
    public interface ILeaveDateCalculator
    {
        Task<int> CalculateWorkingDaysAsync(DateTime startDate, DateTime endDate, bool includePublicHolidays = false);
    }
}
