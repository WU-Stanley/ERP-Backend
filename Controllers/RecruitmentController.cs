using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly INotificationService _notificationService;
        private readonly WUIAMDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public RecruitmentController(
            IRecruitmentService recruitmentService, 
            INotifyService notifyService,
            INotificationService notificationService,
            WUIAMDbContext context,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _recruitmentService = recruitmentService;
            _notifyService = notifyService;
            _notificationService = notificationService;
            _context = context;
            _env = env;
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
        [AllowAnonymous]
        [HttpGet("job-postings/{id}")]
        public async Task<ActionResult<ApiResponse<JobPostingDto>>> GetJobPosting(Guid id)
        {
            var posting = await _recruitmentService.GetJobPostingByIdAsync(id);
            if (posting == null)
                return NotFound(ApiResponse<JobPostingDto>.Failure("Job posting not found."));

            // Anonymous users can only access active postings
            if ((!User.Identity?.IsAuthenticated ?? true) && posting.Status != "Active")
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
        [AllowAnonymous]
        [HttpGet("job-postings")]
        public async Task<ActionResult<ApiResponse<List<JobPostingDto>>>> GetJobPostings([FromQuery] bool onlyActive = false)
        {
            // Anonymous users are forced to view only active postings
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                onlyActive = true;
            }

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

                // Notify the applicant and provide tracking information
                await SendApplicationSubmittedEmailAsync(application);

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
        /// Get the resume file for an application. HR only.
        /// </summary>
        [HasPermission([Permissions.AdminAccess, Permissions.SuperAdminAccess, Permissions.ManageRecruitment])]
        [HttpGet("applications/{id}/resume")]
        public async Task<IActionResult> GetResume(Guid id, [FromQuery] bool download = false)
        {
            var application = await _recruitmentService.GetApplicationByIdAsync(id);
            if (application == null || string.IsNullOrEmpty(application.ResumeFilePath))
                return NotFound(ApiResponse<string>.Failure("Resume not found."));

            var filePath = System.IO.Path.Combine(_env.ContentRootPath, application.ResumeFilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound(ApiResponse<string>.Failure("Resume file not found on server."));

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            if (download)
            {
                return File(bytes, contentType, System.IO.Path.GetFileName(filePath));
            }
            else
            {
                return File(bytes, contentType);
            }
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

            // Notify the applicant of the status update
            await SendStatusUpdateEmailAsync(application);

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
                return Ok(new ApiResponse<ApplicationScoreDto>("No AI score available for this application.", true, null));

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
                    CreatedAt = interview.CreatedAt,
                    Interviewers = interview.Interviewers?.Select(ii => new InterviewerDto
                    {
                        Id = ii.Id,
                        EmployeeId = ii.EmployeeId,
                        EmployeeName = ii.Employee != null ? $"{ii.Employee.FirstName} {ii.Employee.LastName}" : null,
                        Email = ii.Email,
                        Name = ii.Name
                    }).ToList() ?? new List<InterviewerDto>()
                };

                // Notify the applicant and the interviewers of the scheduled interview
                if (interview.Application != null)
                {
                    await SendInterviewScheduledEmailAsync(interview.Application, interviewDto);

                    if (interviewDto.Interviewers != null)
                    {
                        foreach (var interviewer in interviewDto.Interviewers)
                        {
                            // Send email invite to the interviewer (internal or external)
                            await SendInterviewScheduledEmailToInterviewerAsync(
                                interviewer.Email,
                                interviewer.EmployeeName ?? interviewer.Name,
                                interview.Application,
                                interviewDto
                            );

                            // Send in-app notification to internal staff
                            Guid? interviewerUserId = null;
                            if (interviewer.EmployeeId.HasValue)
                            {
                                var emp = await _context.EmployeeDetails
                                    .FirstOrDefaultAsync(e => e.EmployeeId == interviewer.EmployeeId.Value);
                                if (emp != null)
                                {
                                    interviewerUserId = emp.UserId;
                                }
                            }
                            else
                            {
                                var usr = await _context.Users
                                    .FirstOrDefaultAsync(u => u.UserEmail.ToLower() == interviewer.Email.ToLower());
                                if (usr != null)
                                {
                                    interviewerUserId = usr.Id;
                                }
                            }

                            if (interviewerUserId.HasValue)
                            {
                                var notifyTitle = "New Interview Scheduled";
                                var notifyMsg = $"You have been scheduled as an interviewer for {interview.Application.ApplicantName} ({interview.Type}) on {interview.ScheduledFor:f}.";
                                await _notificationService.NotifyUserAsync(interviewerUserId.Value, notifyTitle, notifyMsg, "info", "InterviewSchedule", interview.Id);
                            }
                        }
                    }
                }

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
                CreatedAt = i.CreatedAt,
                Interviewers = i.Interviewers?.Select(ii => new InterviewerDto
                {
                    Id = ii.Id,
                    EmployeeId = ii.EmployeeId,
                    EmployeeName = ii.Employee != null ? $"{ii.Employee.FirstName} {ii.Employee.LastName}" : null,
                    Email = ii.Email,
                    Name = ii.Name
                }).ToList() ?? new List<InterviewerDto>()
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
                    CreatedAt = interview.CreatedAt,
                    Interviewers = interview.Interviewers?.Select(ii => new InterviewerDto
                    {
                        Id = ii.Id,
                        EmployeeId = ii.EmployeeId,
                        EmployeeName = ii.Employee != null ? $"{ii.Employee.FirstName} {ii.Employee.LastName}" : null,
                        Email = ii.Email,
                        Name = ii.Name
                    }).ToList() ?? new List<InterviewerDto>()
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
                return Ok(new ApiResponse<OfferLetterDto>("No offer letter found for this application.", true, null));

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

                // Notify the applicant when an offer letter is sent
                if (offer.Status == "Sent" && offer.Application != null)
                {
                    await SendOfferLetterSentEmailAsync(offer.Application);
                }

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
        private string GetTrackingUrl(JobApplication application)
        {
            var origin = Request.Headers["Origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = "http://localhost:4200"; // Fallback to frontend default
            }
            return $"{origin}/careers/track?applicationId={application.Id}&email={Uri.EscapeDataString(application.Email)}";
        }

        private async Task SendApplicantEmailAsync(JobApplication application, string subject, string htmlBody)
        {
            try
            {
                var receiver = new EmailReceiver
                {
                    Email = application.Email,
                    Name = application.ApplicantName
                };
                await _notifyService.SendEmailAsync(new List<EmailReceiver> { receiver }, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to send email to applicant {application.Email}: {ex.Message}");
            }
        }

        private async Task SendApplicationSubmittedEmailAsync(JobApplication application)
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var trackUrl = GetTrackingUrl(application);
            var subject = $"Application Received: {jobTitle} - Wigwe University";
            var body = $@"
<div style=""font-family: sans-serif; line-height: 1.5; color: #333;"">
  <p>Dear {application.ApplicantName},</p>
  <p>Thank you for applying for the position of <strong>{jobTitle}</strong> at Wigwe University.</p>
  <p>We have successfully received your application. You can track the status of your application at any time using the link below:</p>
  <p style=""margin: 20px 0;"">
    <a href=""{trackUrl}"" style=""background-color: #15803d; color: white; padding: 10px 16px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Track Your Application</a>
  </p>
  <p style=""margin-top: 20px;""><strong>Application Reference ID:</strong> <code style=""background-color: #f1f5f9; padding: 4px 6px; border-radius: 4px;"">{application.Id}</code></p>
  <p>Best regards,</p>
  <p><strong>Recruitment Team</strong><br/>Wigwe University</p>
</div>";

            await SendApplicantEmailAsync(application, subject, body);
        }

        private async Task SendStatusUpdateEmailAsync(JobApplication application)
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var trackUrl = GetTrackingUrl(application);
            var subject = $"Application Update: {jobTitle} - Wigwe University";

            string statusMessage;
            switch (application.Status.ToLower())
            {
                case "under review":
                    statusMessage = $"We wanted to let you know that your application for <strong>{jobTitle}</strong> is now under active review by our hiring team.";
                    break;
                case "shortlisted":
                    statusMessage = $"Great news! You have been shortlisted for the position of <strong>{jobTitle}</strong>. Our team will contact you shortly to arrange the next steps.";
                    break;
                case "rejected":
                    statusMessage = $"Thank you for your interest in the <strong>{jobTitle}</strong> role at Wigwe University and for taking the time to apply. After careful consideration, we have decided not to move forward with your application at this time.<br/><br/>We appreciate your interest in our institution and wish you the best in your professional endeavors.";
                    break;
                case "hired":
                    statusMessage = $"Congratulations! We are thrilled to officially welcome you to Wigwe University as a new team member for the role of <strong>{jobTitle}</strong>.";
                    break;
                default:
                    statusMessage = $"Your application status for <strong>{jobTitle}</strong> has been updated to: <strong>{application.Status}</strong>.";
                    break;
            }

            var body = $@"
<div style=""font-family: sans-serif; line-height: 1.5; color: #333;"">
  <p>Dear {application.ApplicantName},</p>
  <p>{statusMessage}</p>
  <p>To view your application progress or track any updates, please use the link below:</p>
  <p style=""margin: 20px 0;"">
    <a href=""{trackUrl}"" style=""background-color: #15803d; color: white; padding: 10px 16px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Track Your Application</a>
  </p>
  <p>Best regards,</p>
  <p><strong>Recruitment Team</strong><br/>Wigwe University</p>
</div>";

            await SendApplicantEmailAsync(application, subject, body);
        }

        private async Task SendInterviewScheduledEmailAsync(JobApplication application, InterviewDto interview)
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var subject = $"Interview Scheduled: {jobTitle} - Wigwe University";
            var notesBlock = !string.IsNullOrEmpty(interview.Notes) 
                ? $"<li><strong>Notes:</strong> {interview.Notes}</li>" 
                : "";

            var body = $@"
<div style=""font-family: sans-serif; line-height: 1.5; color: #333;"">
  <p>Dear {application.ApplicantName},</p>
  <p>An interview has been scheduled for your application for <strong>{jobTitle}</strong>.</p>
  <p><strong>Interview Details:</strong></p>
  <ul>
    <li><strong>Type:</strong> {interview.Type}</li>
    <li><strong>Date & Time:</strong> {interview.ScheduledFor:dddd, MMMM dd, yyyy} at {interview.ScheduledFor:hh:mm tt} (UTC)</li>
    {notesBlock}
  </ul>
  <p>You can join the interview meeting using the link below:</p>
  <p style=""margin: 20px 0;"">
    <a href=""{interview.MeetingLink}"" style=""background-color: #1d4ed8; color: white; padding: 10px 16px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Join Interview Meeting</a>
  </p>
  <p>If you have any questions or need to reschedule, please contact the HR recruitment team.</p>
  <p>Best regards,</p>
  <p><strong>Recruitment Team</strong><br/>Wigwe University</p>
</div>";

            await SendApplicantEmailAsync(application, subject, body);
        }

        private async Task SendInterviewScheduledEmailToInterviewerAsync(string interviewerEmail, string? interviewerName, JobApplication application, InterviewDto interview)
        {
            try
            {
                var jobTitle = application.JobPosting?.Title ?? "Position";
                var subject = $"Interview Assignment: {application.ApplicantName} - {jobTitle}";
                var notesBlock = !string.IsNullOrEmpty(interview.Notes) 
                    ? $"<li><strong>Notes:</strong> {interview.Notes}</li>" 
                    : "";

                var body = $@"
<div style=""font-family: sans-serif; line-height: 1.5; color: #333;"">
  <p>Dear {interviewerName ?? "Colleague"},</p>
  <p>You have been scheduled as an interviewer for <strong>{application.ApplicantName}</strong> who is applying for the position of <strong>{jobTitle}</strong>.</p>
  <p><strong>Interview Details:</strong></p>
  <ul>
    <li><strong>Candidate Name:</strong> {application.ApplicantName}</li>
    <li><strong>Type:</strong> {interview.Type}</li>
    <li><strong>Date & Time:</strong> {interview.ScheduledFor:dddd, MMMM dd, yyyy} at {interview.ScheduledFor:hh:mm tt} (UTC)</li>
    {notesBlock}
  </ul>
  <p>Please join the interview meeting using the link below:</p>
  <p style=""margin: 20px 0;"">
    <a href=""{interview.MeetingLink}"" style=""background-color: #1d4ed8; color: white; padding: 10px 16px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">Join Interview Meeting</a>
  </p>
  <p>If you have any conflicts or need to reschedule, please coordinate with the recruitment team.</p>
  <p>Best regards,</p>
  <p><strong>Recruitment System</strong><br/>Wigwe University</p>
</div>";

                var receiver = new EmailReceiver
                {
                    Email = interviewerEmail,
                    Name = interviewerName ?? "Interviewer"
                };
                await _notifyService.SendEmailAsync(new List<EmailReceiver> { receiver }, subject, body);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to send email to interviewer {interviewerEmail}: {ex.Message}");
            }
        }

        private async Task SendOfferLetterSentEmailAsync(JobApplication application)
        {
            var jobTitle = application.JobPosting?.Title ?? "Position";
            var trackUrl = GetTrackingUrl(application);
            var subject = $"Official Job Offer: {jobTitle} - Wigwe University";
            var body = $@"
<div style=""font-family: sans-serif; line-height: 1.5; color: #333;"">
  <p>Dear {application.ApplicantName},</p>
  <p>We are pleased to inform you that Wigwe University has extended an official job offer for the position of <strong>{jobTitle}</strong>!</p>
  <p>Please click the link below to view the offer letter details, check the terms, and submit your response (Accept/Decline):</p>
  <p style=""margin: 20px 0;"">
    <a href=""{trackUrl}"" style=""background-color: #15803d; color: white; padding: 10px 16px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;"">View Offer Details</a>
  </p>
  <p>If you have any questions or require additional details, please let us know.</p>
  <p>Best regards,</p>
  <p><strong>Recruitment Team</strong><br/>Wigwe University</p>
</div>";

            await SendApplicantEmailAsync(application, subject, body);
        }

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
