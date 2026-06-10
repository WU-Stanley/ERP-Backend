using WUIAM.Repositories.IRepositories;

public class LeaveDateCalculator : ILeaveDateCalculator
{
    public async Task<int> CalculateWorkingDaysAsync(DateTime startDate, DateTime endDate)
    {
        // Implementation for calculating working days between two dates
        // This is a placeholder implementation and should be replaced with actual logic
        int workingDays = 0;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
        }

        return await Task.FromResult(workingDays > 0 ? workingDays : 1);
    }

}
