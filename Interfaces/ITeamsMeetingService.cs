namespace WUIAM.Interfaces
{
    public interface ITeamsMeetingService
    {
        Task<string?> CreateTeamsMeetingAsync(string title, DateTime startTime, DateTime endTime, string organizerEmail);
        Task<bool> CancelMeetingAsync(string teamsMeetingId);
        Task<string?> GetMeetingLinkAsync(string teamsMeetingId);
    }
}
