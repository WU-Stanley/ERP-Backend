using WUIAM.DTOs;
using WUIAM.Models;

namespace WUIAM.Interfaces
{
    public interface IRecruitmentService
    {
        // Job Postings
        Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto, Guid createdBy);
        Task<JobPosting?> UpdateJobPostingAsync(Guid id, UpdateJobPostingDto dto);
        Task<bool> DeleteJobPostingAsync(Guid id);
        Task<JobPosting?> GetJobPostingByIdAsync(Guid id);
        Task<List<JobPosting>> GetJobPostingsAsync(bool onlyActive = false);

        // Applications
        Task<JobApplication> CreateApplicationAsync(Guid jobId, CreateApplicationDto dto, Microsoft.AspNetCore.Http.IFormFile? resume);
        Task<JobApplication?> GetApplicationByIdAsync(Guid id);
        Task<ApplicantTrackingDto?> GetApplicantTrackingAsync(string applicationReference, string email);
        Task<ApplicationListDto> GetApplicationsAsync(int pageNumber, int pageSize, string? statusFilter, string? search);
        Task<JobApplication> UpdateApplicationStatusAsync(Guid id, UpdateApplicationStatusDto dto);

        // AI Resume Scanning
        Task<ApplicationScore?> ScanResumeAsync(Guid applicationId);
        Task<ApplicationScore?> GetLatestScoreAsync(Guid applicationId);

        // Interviews
        Task<InterviewSchedule> CreateInterviewAsync(Guid applicationId, CreateInterviewDto dto);
        Task<List<InterviewSchedule>> GetInterviewsByApplicationIdAsync(Guid applicationId);
        Task<InterviewSchedule> UpdateInterviewStatusAsync(Guid id, string status, string? notes = null);
        Task<InterviewInterviewer> SubmitInterviewFeedbackAsync(Guid interviewerId, SubmitInterviewFeedbackDto dto);

        // Offer Letters
        Task<OfferLetter> CreateOfferLetterAsync(Guid applicationId, CreateOfferLetterDto dto);
        Task<OfferLetter?> GetOfferLetterByApplicationIdAsync(Guid applicationId);
        Task<OfferLetter> UpdateOfferLetterStatusAsync(Guid id, string status);
        Task<OfferLetter> RespondToOfferAsync(Guid id, string response, string? comments = null, string? signedName = null, string? signatureData = null);

        // Queries
        Task<ApplicantQuery> CreateQueryAsync(Guid applicationId, string message, string fromType, Guid? fromUserId = null);
        Task<List<ApplicantQuery>> GetQueriesByApplicationIdAsync(Guid applicationId);

        // Stats
        Task<RecruitmentStatsDto> GetRecruitmentStatsAsync();
        Task<PublicRecruitmentStatsDto> GetPublicRecruitmentStatsAsync();
    }
}
