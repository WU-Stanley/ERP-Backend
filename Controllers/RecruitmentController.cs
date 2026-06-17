using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WUIAM.Attributes;
using WUIAM.DTOs;
using WUIAM.Enums;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Controllers
{
    /// <summary>
    /// API v1 - Recruitment management endpoints for job postings, applications, interviews, and offers.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _recruitmentService;
        private readonly INotifyService _notifyService;

        public RecruitmentController(IRecruitmentService recruitmentService, INotifyService notifyService)
        {
            _recruitmentService = recruitmentService;
            _notifyService = notifyService;
        }

        // ==================== Job Postings ====================

        /// <summary>
        /// Create a new job posting.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPost("job-postings")]
        public async Task<ActionResult<ApiResponse<JobPosting>>> CreateJobPosting([FromBody] CreateJobPostingDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return Unauthorized(ApiResponse<JobPosting>.Failure("User not authenticated."));

            var posting = await _recruitmentService.CreateJobPostingAsync(dto, userId);
            return Ok(ApiResponse<JobPosting>.Success("Job posting created successfully.", posting));
        }

        /// <summary>
        /// Update a job posting.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPatch("job-postings/{id}")]
        public async Task<ActionResult<ApiResponse<JobPosting>>> UpdateJobPosting(Guid id, [FromBody] UpdateJobPostingDto dto)
        {
            var posting = await _recruitmentService.UpdateJobPostingAsync(id, dto);
            if (posting == null)
                return NotFound(ApiResponse<JobPosting>.Failure("Job posting not found."));

            return Ok(ApiResponse<JobPosting>.Success("Job posting updated successfully.", posting));
        }

        /// <summary>
        /// Delete (archive) a job posting.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpDelete("job-postings/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteJobPosting(Guid id)
        {
            var success = await _recruitmentService.DeleteJobPostingAsync(id);
            if (!success)
                return NotFound(ApiResponse<string>.Failure("Job posting not found."));

            return Ok(ApiResponse<string>.Success("Job posting archived successfully.", "true"));
        }

        /// <summary>
        /// Get a job posting by ID.
        /// </summary>
        [HttpGet("job-postings/{id}")]
        public async Task<ActionResult<ApiResponse<JobPostingDto>>> GetJobPosting(Guid id)
        {
            var posting = await _recruitmentService.GetJobPostingByIdAsync(id);
            if (posting == null)
                return NotFound(ApiResponse<JobPostingDto>.Failure("Job posting not found."));

            var dto = new JobPostingDto
            {
                Id = posting.Id,
                Title = posting.Title,
                Description = posting.Description,
                Requirements = posting.Requirements,
                Location = posting.Location,
                EmploymentType = posting.EmploymentType,
                Status = posting.Status,
                DepartmentId = posting.DepartmentId,
                DepartmentName = posting.Department?.Name,
                CreatedBy = posting.CreatedBy,
                CreatedByName = posting.CreatedByUser?.UserName,
                Deadline = posting.Deadline ?? DateTime.UtcNow.AddDays(30),
                ApplicationsCount = posting.ApplicationsCount,
                CreatedAt = posting.CreatedAt,
                UpdatedAt = posting.UpdatedAt ?? DateTime.UtcNow
            };

            return Ok(ApiResponse<JobPostingDto>.Success("Job posting retrieved.", dto));
        }

        /// <summary>
        /// Get all job postings. Public can see Active ones, HR can see all.
        /// </summary>
        [HttpGet("job-postings")]
        public async Task<ActionResult<ApiResponse<List<JobPostingDto>>>> GetJobPostings([FromQuery] bool onlyActive = false)
        {
            var postings = await _recruitmentService.GetJobPostingsAsync(onlyActive);
            var dtos = postings.Select(p => new JobPostingDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Requirements = p.Requirements,
                Location = p.Location,
                EmploymentType = p.EmploymentType,
                Status = p.Status,
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department?.Name,
                CreatedBy = p.CreatedBy,
                CreatedByName = p.CreatedByUser?.UserName,
                Deadline = p.Deadline ?? p.CreatedAt.AddDays(30),
                ApplicationsCount = p.ApplicationsCount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt ?? DateTime.UtcNow
            }).ToList();

            return Ok(ApiResponse<List<JobPostingDto>>.Success("Job postings retrieved.", dtos));
        }

        // ==================== Applications ====================

        /// <summary>
        /// Public application endpoint - no authentication required.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("public/apply/{jobId}")]
        public async Task<ActionResult<ApiResponse<JobApplication>>> ApplyForJob(
            Guid jobId,
            [FromForm] CreateApplicationDto dto,
            [FromForm] IFormFile? resume)
        {
            try
            {
                var application = await _recruitmentService.CreateApplicationAsync(jobId, dto, resume);

                // Notify HR recruitment team
                var jobTitle = application.JobPosting?.Title ?? "Position";
                await _notifyService.NotifyRecruitmentTeamAsync(
                    "New Job Application",
                    $"New application for \"{jobTitle}\" by {application.ApplicantName} ({application.Email}).",
                    "info",
                    "JobApplication",
                    application.Id);

                return Ok(ApiResponse<JobApplication>.Success("Application submitted successfully.", application));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<JobApplication>.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Public application tracking endpoint. Requires the application ID and applicant email.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("public/applications/{id}/track")]
        public async Task<ActionResult<ApiResponse<ApplicantTrackingDto>>> TrackApplication(Guid id, [FromQuery] string email)
        {
            var tracking = await _recruitmentService.GetApplicantTrackingAsync(id, email);
            if (tracking == null)
                return NotFound(ApiResponse<ApplicantTrackingDto>.Failure("No application found for this reference and email."));

            return Ok(ApiResponse<ApplicantTrackingDto>.Success("Application tracking retrieved.", tracking));
        }

        /// <summary>
        /// Get application by ID.
        /// </summary>
        [HttpGet("applications/{id}")]
        public async Task<ActionResult<ApiResponse<ApplicationDto>>> GetApplication(Guid id)
        {
            var application = await _recruitmentService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse<ApplicationDto>.Failure("Application not found."));

            var dto = new ApplicationDto
            {
                Id = application.Id,
                JobPostingId = application.JobPostingId,
                JobTitle = application.JobPosting?.Title ?? "Unknown",
                ApplicantName = application.ApplicantName,
                Email = application.Email,
                Phone = application.Phone,
                ResumeFilePath = application.ResumeFilePath,
                CoverLetter = application.CoverLetter,
                Status = application.Status,
                AssignedTo = application.AssignedTo,
                AssignedToName = application.AssignedToUser?.UserName,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt ?? DateTime.UtcNow
            };

            // Attach latest score
            var latestScore = await _recruitmentService.GetLatestScoreAsync(id);
            if (latestScore != null)
            {
                dto.ScoredAt = latestScore.ScannedAt;
                dto.OverallMatch = latestScore.OverallMatch;
            }

            return Ok(ApiResponse<ApplicationDto>.Success("Application retrieved.", dto));
        }

        /// <summary>
        /// Get all applications with pagination and filters. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpGet("applications")]
        public async Task<ActionResult<ApiResponse<ApplicationListDto>>> GetApplications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? statusFilter = null,
            [FromQuery] string? search = null)
        {
            var result = await _recruitmentService.GetApplicationsAsync(pageNumber, pageSize, statusFilter, search);
            return Ok(ApiResponse<ApplicationListDto>.Success("Applications retrieved.", result));
        }

        /// <summary>
        /// Update application status. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPatch("applications/{id}/status")]
        public async Task<ActionResult<ApiResponse<JobApplication>>> UpdateApplicationStatus(Guid id, [FromBody] UpdateApplicationStatusDto dto)
        {
            var application = await _recruitmentService.UpdateApplicationStatusAsync(id, dto);
            if (application == null)
                return NotFound(ApiResponse<JobApplication>.Failure("Application not found."));

            return Ok(ApiResponse<JobApplication>.Success("Application status updated.", application));
        }

        // ==================== AI Resume Scanning ====================

        /// <summary>
        /// Trigger AI resume scanning for an application. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPost("applications/{id}/scan-resume")]
        public async Task<ActionResult<ApiResponse<ApplicationScoreDto>>> ScanResume(Guid id)
        {
            try
            {
                var score = await _recruitmentService.ScanResumeAsync(id);
                if (score == null)
                    return BadRequest(ApiResponse<ApplicationScoreDto>.Failure("Could not scan resume. File may be corrupted or unsupported."));

                var dto = MapToScoreDto(score);
                return Ok(ApiResponse<ApplicationScoreDto>.Success("Resume scanned successfully.", dto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ApplicationScoreDto>.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Get the latest AI score for an application.
        /// </summary>
        [HttpGet("applications/{id}/score")]
        public async Task<ActionResult<ApiResponse<ApplicationScoreDto>>> GetApplicationScore(Guid id)
        {
            var score = await _recruitmentService.GetLatestScoreAsync(id);
            if (score == null)
                return NotFound(ApiResponse<ApplicationScoreDto>.Failure("No AI score available for this application."));

            return Ok(ApiResponse<ApplicationScoreDto>.Success("Score retrieved.", MapToScoreDto(score)));
        }

        // ==================== Interviews ====================

        /// <summary>
        /// Schedule an interview for an application. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPost("applications/{id}/interview")]
        public async Task<ActionResult<ApiResponse<InterviewDto>>> ScheduleInterview(Guid id, [FromBody] CreateInterviewDto dto)
        {
            try
            {
                var interview = await _recruitmentService.CreateInterviewAsync(id, dto);

                var interviewDto = new InterviewDto
                {
                    Id = interview.Id,
                    ApplicationId = interview.ApplicationId,
                    ApplicantName = interview.Application?.ApplicantName ?? "Unknown",
                    Type = interview.Type,
                    ScheduledFor = interview.ScheduledFor,
                    MeetingLink = interview.MeetingLink,
                    TeamsMeetingId = interview.TeamsMeetingId,
                    Status = interview.Status,
                    Notes = interview.Notes,
                    CreatedAt = interview.CreatedAt
                };

                return Ok(ApiResponse<InterviewDto>.Success("Interview scheduled successfully.", interviewDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<InterviewDto>.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Get all interviews for an application.
        /// </summary>
        [HttpGet("applications/{id}/interviews")]
        public async Task<ActionResult<ApiResponse<List<InterviewDto>>>> GetInterviews(Guid id)
        {
            var interviews = await _recruitmentService.GetInterviewsByApplicationIdAsync(id);

            var dtos = interviews.Select(i => new InterviewDto
            {
                Id = i.Id,
                ApplicationId = i.ApplicationId,
                ApplicantName = i.Application?.ApplicantName ?? "Unknown",
                Type = i.Type,
                ScheduledFor = i.ScheduledFor,
                MeetingLink = i.MeetingLink,
                TeamsMeetingId = i.TeamsMeetingId,
                Status = i.Status,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<InterviewDto>>.Success("Interviews retrieved.", dtos));
        }

        /// <summary>
        /// Update interview status.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPatch("interviews/{id}/status")]
        public async Task<ActionResult<ApiResponse<InterviewDto>>> UpdateInterviewStatus(Guid id, [FromBody] UpdateInterviewStatusDto dto)
        {
            try
            {
                var interview = await _recruitmentService.UpdateInterviewStatusAsync(id, dto.Status, dto.Notes);

                var interviewDto = new InterviewDto
                {
                    Id = interview.Id,
                    ApplicationId = interview.ApplicationId,
                    ApplicantName = interview.Application?.ApplicantName ?? "Unknown",
                    Type = interview.Type,
                    ScheduledFor = interview.ScheduledFor,
                    MeetingLink = interview.MeetingLink,
                    TeamsMeetingId = interview.TeamsMeetingId,
                    Status = interview.Status,
                    Notes = interview.Notes,
                    CreatedAt = interview.CreatedAt
                };

                return Ok(ApiResponse<InterviewDto>.Success("Interview status updated.", interviewDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<InterviewDto>.Failure(ex.Message));
            }
        }

        // ==================== Offer Letters ====================

        /// <summary>
        /// Create an offer letter for an application. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPost("applications/{id}/offer-letter")]
        public async Task<ActionResult<ApiResponse<OfferLetterDto>>> CreateOfferLetter(Guid id, [FromBody] CreateOfferLetterDto dto)
        {
            try
            {
                var offer = await _recruitmentService.CreateOfferLetterAsync(id, dto);

                var offerDto = new OfferLetterDto
                {
                    Id = offer.Id,
                    ApplicationId = offer.ApplicationId,
                    ApplicantName = offer.Application?.ApplicantName ?? "Unknown",
                    CompanyName = offer.CompanyName,
                    Position = offer.Position,
                    Salary = offer.Salary,
                    StartDate = offer.StartDate,
                    Benefits = offer.Benefits ?? string.Empty,
                    Content = offer.Content,
                    Status = offer.Status,
                    SentAt = offer.SentAt,
                    ExpiresAt = offer.ExpiresAt,
                    SignedAt = offer.SignedAt,
                    CreatedAt = offer.CreatedAt
                };

                return Ok(ApiResponse<OfferLetterDto>.Success("Offer letter created successfully.", offerDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OfferLetterDto>.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Get offer letter for an application.
        /// </summary>
        [HttpGet("applications/{id}/offer-letter")]
        public async Task<ActionResult<ApiResponse<OfferLetterDto>>> GetOfferLetter(Guid id)
        {
            var offer = await _recruitmentService.GetOfferLetterByApplicationIdAsync(id);
            if (offer == null)
                return NotFound(ApiResponse<OfferLetterDto>.Failure("No offer letter found for this application."));

            var offerDto = new OfferLetterDto
            {
                Id = offer.Id,
                ApplicationId = offer.ApplicationId,
                ApplicantName = offer.Application?.ApplicantName ?? "Unknown",
                CompanyName = offer.CompanyName,
                Position = offer.Position,
                Salary = offer.Salary,
                StartDate = offer.StartDate,
                Benefits = offer.Benefits ?? string.Empty,
                Content = offer.Content,
                Status = offer.Status,
                SentAt = offer.SentAt,
                ExpiresAt = offer.ExpiresAt,
                SignedAt = offer.SignedAt,
                CreatedAt = offer.CreatedAt
            };

            return Ok(ApiResponse<OfferLetterDto>.Success("Offer letter retrieved.", offerDto));
        }

        /// <summary>
        /// Update offer letter status. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpPatch("offers/{id}/status")]
        public async Task<ActionResult<ApiResponse<OfferLetterDto>>> UpdateOfferStatus(Guid id, [FromBody] UpdateOfferStatusDto dto)
        {
            try
            {
                var offer = await _recruitmentService.UpdateOfferLetterStatusAsync(id, dto.Status);

                var offerDto = new OfferLetterDto
                {
                    Id = offer.Id,
                    ApplicationId = offer.ApplicationId,
                    ApplicantName = offer.Application?.ApplicantName ?? "Unknown",
                    CompanyName = offer.CompanyName,
                    Position = offer.Position,
                    Salary = offer.Salary,
                    StartDate = offer.StartDate,
                    Benefits = offer.Benefits ?? string.Empty,
                    Content = offer.Content,
                    Status = offer.Status,
                    SentAt = offer.SentAt,
                    ExpiresAt = offer.ExpiresAt,
                    SignedAt = offer.SignedAt,
                    CreatedAt = offer.CreatedAt
                };

                return Ok(ApiResponse<OfferLetterDto>.Success("Offer status updated.", offerDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OfferLetterDto>.Failure(ex.Message));
            }
        }

        /// <summary>
        /// Applicant responds to offer letter (accept/decline). Public endpoint.
        /// </summary>
        [AllowAnonymous]
        [HttpPatch("offers/{id}/respond")]
        public async Task<ActionResult<ApiResponse<OfferLetterDto>>> RespondToOffer(Guid id, [FromBody] OfferResponseDto dto)
        {
            try
            {
                var offer = await _recruitmentService.RespondToOfferAsync(id, dto.Response, dto.Comments);

                var offerDto = new OfferLetterDto
                {
                    Id = offer.Id,
                    ApplicationId = offer.ApplicationId,
                    ApplicantName = offer.Application?.ApplicantName ?? "Unknown",
                    CompanyName = offer.CompanyName,
                    Position = offer.Position,
                    Salary = offer.Salary,
                    StartDate = offer.StartDate,
                    Benefits = offer.Benefits ?? string.Empty,
                    Content = offer.Content,
                    Status = offer.Status,
                    SentAt = offer.SentAt,
                    ExpiresAt = offer.ExpiresAt,
                    SignedAt = offer.SignedAt,
                    CreatedAt = offer.CreatedAt
                };

                return Ok(ApiResponse<OfferLetterDto>.Success($"Offer {dto.Response.ToLower()} successfully.", offerDto));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OfferLetterDto>.Failure(ex.Message));
            }
        }

        // ==================== Queries ====================

        /// <summary>
        /// Create a query (from applicant or HR).
        /// </summary>
        [HttpPost("applications/{id}/query")]
        public async Task<ActionResult<ApiResponse<QueryDto>>> CreateQuery(Guid id, [FromBody] CreateQueryDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var fromType = userIdClaim != null ? "HR" : "Applicant";
            var fromUserId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var uid) ? (Guid?)uid : null;

            var query = await _recruitmentService.CreateQueryAsync(id, dto.Message, fromType, fromUserId);

            var queryDto = new QueryDto
            {
                Id = query.Id,
                ApplicationId = query.ApplicationId,
                Message = query.Message,
                MessageFrom = query.MessageFrom,
                FromUserName = query.FromUser?.UserName,
                CreatedAt = query.CreatedAt
            };

            return Ok(ApiResponse<QueryDto>.Success("Query sent successfully.", queryDto));
        }

        /// <summary>
        /// Get all queries for an application.
        /// </summary>
        [HttpGet("applications/{id}/queries")]
        public async Task<ActionResult<ApiResponse<List<QueryDto>>>> GetQueries(Guid id)
        {
            var queries = await _recruitmentService.GetQueriesByApplicationIdAsync(id);

            var dtos = queries.Select(q => new QueryDto
            {
                Id = q.Id,
                ApplicationId = q.ApplicationId,
                Message = q.Message,
                MessageFrom = q.MessageFrom,
                FromUserName = q.FromUser?.UserName,
                CreatedAt = q.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<QueryDto>>.Success("Queries retrieved.", dtos));
        }

        // ==================== Stats ====================

        /// <summary>
        /// Get recruitment statistics. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpGet("stats")]
        public async Task<ActionResult> GetRecruitmentStats()
        {
            var stats = await _recruitmentService.GetRecruitmentStatsAsync();
            return Ok(new
            {
                message = "Recruitment statistics retrieved.",
                status = true,
                data = stats
            });
        }

        /// <summary>
        /// Get public recruitment statistics.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("public/stats")]
        public async Task<ActionResult<ApiResponse<PublicRecruitmentStatsDto>>> GetPublicRecruitmentStats()
        {
            var stats = await _recruitmentService.GetPublicRecruitmentStatsAsync();
            return Ok(ApiResponse<PublicRecruitmentStatsDto>.Success("Public recruitment stats retrieved.", stats));
        }

        // ==================== Helpers ====================
        private ApplicationScoreDto MapToScoreDto(ApplicationScore score)
        {
            return new ApplicationScoreDto
            {
                Id = score.Id,
                ApplicationId = score.ApplicationId,
                TechnicalMatch = score.TechnicalMatch,
                CulturalFit = score.CulturalFit,
                EducationMatch = score.EducationMatch,
                ExperienceMatch = score.ExperienceMatch,
                OverallMatch = score.OverallMatch,
                ScannedResumeText = score.ScannedResumeText,
                AiAnalysisNotes = score.AiAnalysisNotes,
                ScannedAt = score.ScannedAt
            };
        }
    }

    // Helper DTO for interview status update
    public class UpdateInterviewStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    // Helper DTO for offer status update
    public class UpdateOfferStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}
