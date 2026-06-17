using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;

namespace WUIAM.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly WUIAMDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IAiResumeScanningService _aiService;
        private readonly ITeamsMeetingService _teamsMeetingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecruitmentService> _logger;

        public RecruitmentService(
            WUIAMDbContext context,
            IWebHostEnvironment env,
            IAiResumeScanningService aiService,
            ITeamsMeetingService teamsMeetingService,
            IConfiguration configuration,
            ILogger<RecruitmentService> logger)
        {
            _context = context;
            _env = env;
            _aiService = aiService;
            _teamsMeetingService = teamsMeetingService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<JobPosting> CreateJobPostingAsync(CreateJobPostingDto dto, Guid createdBy)
        {
            var creatorUserId = await ResolveCreatorUserIdAsync(createdBy);
            var posting = new JobPosting
            {
                Title = dto.Title, Description = dto.Description, Requirements = dto.Requirements ?? string.Empty,
                Location = dto.Location, EmploymentType = dto.EmploymentType, DepartmentId = dto.DepartmentId,
                CreatedBy = creatorUserId, Deadline = dto.Deadline, Status = "Draft"
            };
            _context.JobPostings.Add(posting);
            await _context.SaveChangesAsync();
            return posting;
        }

        private async Task<Guid> ResolveCreatorUserIdAsync(Guid createdBy)
        {
            if (createdBy != Guid.Empty && await _context.Users.AnyAsync(user => user.Id == createdBy))
            {
                return createdBy;
            }

            var superAdminUserId = await _context.Users
                .Where(user => user.UserRoles.Any(userRole =>
                    userRole.Role.Name == "SuperAdmin" ||
                    userRole.Role.Name == "Super Admin" ||
                    userRole.Role.Name == "Super Administrator"))
                .Select(user => (Guid?)user.Id)
                .FirstOrDefaultAsync();

            if (superAdminUserId.HasValue)
            {
                return superAdminUserId.Value;
            }

            var defaultUserId = await _context.Users
                .Where(user => user.IsDefault || user.IsActive)
                .OrderByDescending(user => user.IsDefault)
                .Select(user => (Guid?)user.Id)
                .FirstOrDefaultAsync();

            if (defaultUserId.HasValue)
            {
                return defaultUserId.Value;
            }

            throw new InvalidOperationException("A valid creator user is required to create a job posting.");
        }

        public async Task<JobPosting?> UpdateJobPostingAsync(Guid id, UpdateJobPostingDto dto)
        {
            var posting = await _context.JobPostings.FindAsync(id);
            if (posting == null) return null;
            if (dto.Title != null) posting.Title = dto.Title;
            if (dto.Description != null) posting.Description = dto.Description;
            if (dto.Requirements != null) posting.Requirements = dto.Requirements;
            if (dto.Location != null) posting.Location = dto.Location;
            if (dto.EmploymentType != null) posting.EmploymentType = dto.EmploymentType;
            if (dto.Status != null) posting.Status = dto.Status;
            if (dto.DepartmentId != null) posting.DepartmentId = dto.DepartmentId;
            if (dto.Deadline != null) posting.Deadline = dto.Deadline;
            posting.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return posting;
        }

        public async Task<bool> DeleteJobPostingAsync(Guid id)
        {
            var posting = await _context.JobPostings.FindAsync(id);
            if (posting == null) return false;
            posting.Status = "Archived";
            posting.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<JobPosting?> GetJobPostingByIdAsync(Guid id)
        {
            return await _context.JobPostings
                .Include(p => p.Department).Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<JobPosting>> GetJobPostingsAsync(bool onlyActive = false)
        {
            var query = _context.JobPostings
                .Include(p => p.Department).Include(p => p.CreatedByUser).AsQueryable();
            if (onlyActive) query = query.Where(p => p.Status == "Active");
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<JobApplication> CreateApplicationAsync(Guid jobId, CreateApplicationDto dto, Microsoft.AspNetCore.Http.IFormFile? resume)
        {
            var jobPosting = await _context.JobPostings.FindAsync(jobId)
                ?? throw new InvalidOperationException("Job posting not found or inactive.");
            if (jobPosting.Status != "Active")
                throw new InvalidOperationException("This position is not accepting applications.");
            if (jobPosting.Deadline.HasValue && jobPosting.Deadline.Value < DateTime.UtcNow)
                throw new InvalidOperationException("Application deadline has passed.");

            var app = new JobApplication
            {
                JobPostingId = jobId, ApplicantName = dto.ApplicantName, Email = dto.Email,
                Phone = dto.Phone, CoverLetter = dto.CoverLetter, Status = "New"
            };

            if (resume != null && resume.Length > 0)
            {
                ValidateResumeUpload(resume);

                var uploadDir = Path.Combine(_env.ContentRootPath, "uploads", "recruitment");
                Directory.CreateDirectory(uploadDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(resume.FileName).ToLowerInvariant()}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await resume.CopyToAsync(stream);
                app.ResumeFilePath = $"/uploads/recruitment/{fileName}";
            }

            _context.JobApplications.Add(app);
            jobPosting.ApplicationsCount++;
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task<JobApplication?> GetApplicationByIdAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(a => a.JobPosting).Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<ApplicantTrackingDto?> GetApplicantTrackingAsync(Guid applicationId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();
            var application = await _context.JobApplications
                .Include(a => a.JobPosting)
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Email.ToLower() == normalizedEmail);

            if (application == null)
            {
                return null;
            }

            var latestScore = await GetLatestScoreAsync(application.Id);
            var interviews = await _context.InterviewSchedules
                .Where(i => i.ApplicationId == application.Id)
                .OrderByDescending(i => i.ScheduledFor)
                .ToListAsync();
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.ApplicationId == application.Id);
            var queries = await _context.ApplicantQueries
                .Include(q => q.FromUser)
                .Where(q => q.ApplicationId == application.Id)
                .OrderBy(q => q.CreatedAt)
                .ToListAsync();

            return new ApplicantTrackingDto
            {
                Application = new ApplicationDto
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
                    UpdatedAt = application.UpdatedAt ?? application.CreatedAt,
                    OverallMatch = latestScore?.OverallMatch,
                    ScoredAt = latestScore?.ScannedAt
                },
                Interviews = interviews.Select(i => new InterviewDto
                {
                    Id = i.Id,
                    ApplicationId = i.ApplicationId,
                    ApplicantName = application.ApplicantName,
                    Type = i.Type,
                    ScheduledFor = i.ScheduledFor,
                    MeetingLink = i.MeetingLink,
                    TeamsMeetingId = i.TeamsMeetingId,
                    Status = i.Status,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt
                }).ToList(),
                OfferLetter = offer == null ? null : new OfferLetterDto
                {
                    Id = offer.Id,
                    ApplicationId = offer.ApplicationId,
                    ApplicantName = application.ApplicantName,
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
                },
                Queries = queries.Select(q => new QueryDto
                {
                    Id = q.Id,
                    ApplicationId = q.ApplicationId,
                    Message = q.Message,
                    MessageFrom = q.MessageFrom,
                    FromUserName = q.FromUser?.UserName,
                    CreatedAt = q.CreatedAt
                }).ToList()
            };
        }

        public async Task<ApplicationListDto> GetApplicationsAsync(int pageNumber, int pageSize, string? statusFilter, string? search)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.JobApplications
                .Include(a => a.JobPosting).Include(a => a.AssignedToUser).AsQueryable();
            if (!string.IsNullOrEmpty(statusFilter)) query = query.Where(a => a.Status == statusFilter);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(a => a.ApplicantName.Contains(search) || a.Email.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var applications = await query.OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var applicationIds = applications.Select(a => a.Id).ToList();
            var latestScores = await _context.ApplicationScores
                .Where(s => applicationIds.Contains(s.ApplicationId))
                .OrderByDescending(s => s.ScannedAt)
                .ToListAsync();
            var latestScoreByApplicationId = latestScores
                .GroupBy(s => s.ApplicationId)
                .ToDictionary(g => g.Key, g => g.First());

            var dtos = applications.Select(a =>
            {
                latestScoreByApplicationId.TryGetValue(a.Id, out var latestScore);

                return new ApplicationDto
                {
                    Id = a.Id,
                    JobPostingId = a.JobPostingId,
                    JobTitle = a.JobPosting?.Title ?? "Unknown",
                    ApplicantName = a.ApplicantName,
                    Email = a.Email,
                    Phone = a.Phone,
                    ResumeFilePath = a.ResumeFilePath,
                    CoverLetter = a.CoverLetter,
                    Status = a.Status,
                    AssignedTo = a.AssignedTo,
                    AssignedToName = a.AssignedToUser?.UserName,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt ?? DateTime.UtcNow,
                    OverallMatch = latestScore?.OverallMatch,
                    ScoredAt = latestScore?.ScannedAt
                };
            }).ToList();

            return new ApplicationListDto { Applications = dtos, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<JobApplication> UpdateApplicationStatusAsync(Guid id, UpdateApplicationStatusDto dto)
        {
            var app = await _context.JobApplications.FindAsync(id)
                ?? throw new InvalidOperationException("Application not found.");
            app.Status = dto.Status;
            if (dto.AssignedTo != null) app.AssignedTo = dto.AssignedTo;
            app.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task<ApplicationScore?> ScanResumeAsync(Guid applicationId)
        {
            var app = await _context.JobApplications.Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new InvalidOperationException("Application not found.");
            if (string.IsNullOrEmpty(app.ResumeFilePath))
                throw new InvalidOperationException("No resume file attached.");

            var fullPath = Path.Combine(_env.ContentRootPath, app.ResumeFilePath.TrimStart('/'));
            if (!File.Exists(fullPath))
                throw new InvalidOperationException("Resume file not found on server.");

            var resumeText = await ExtractTextFromFileAsync(fullPath);
            var score = await _aiService.ScanResumeAsync(resumeText, app.JobPosting?.Requirements ?? "");

            var scoreEntity = new ApplicationScore
            {
                ApplicationId = applicationId, TechnicalMatch = score.TechnicalMatch,
                CulturalFit = score.CulturalFit, EducationMatch = score.EducationMatch,
                ExperienceMatch = score.ExperienceMatch, OverallMatch = score.OverallMatch,
                ScannedResumeText = resumeText, AiAnalysisNotes = score.AiAnalysisNotes
            };
            _context.ApplicationScores.Add(scoreEntity);
            await _context.SaveChangesAsync();
            return scoreEntity;
        }

        public async Task<ApplicationScore?> GetLatestScoreAsync(Guid applicationId)
        {
            return await _context.ApplicationScores.Where(s => s.ApplicationId == applicationId)
                .OrderByDescending(s => s.ScannedAt).FirstOrDefaultAsync();
        }

        public async Task<InterviewSchedule> CreateInterviewAsync(Guid applicationId, CreateInterviewDto dto)
        {
            var app = await _context.JobApplications
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new InvalidOperationException("Application not found.");
            if (dto.ScheduledFor <= DateTime.UtcNow)
                throw new InvalidOperationException("Interview must be scheduled for a future date and time.");

            var endTime = dto.ScheduledFor.AddHours(1);
            var meetingLink = await _teamsMeetingService.CreateTeamsMeetingAsync(
                $"Interview: {app.ApplicantName} - {app.JobPosting?.Title ?? "Position"}",
                dto.ScheduledFor, endTime, "hr@wigweuniversity.edu.ng");

            var interview = new InterviewSchedule
            {
                ApplicationId = applicationId, Type = dto.Type, ScheduledFor = dto.ScheduledFor,
                MeetingLink = meetingLink, Notes = dto.Notes, Status = "Scheduled"
            };
            _context.InterviewSchedules.Add(interview);

            if (app.Status == "New" || app.Status == "Under Review")
            {
                app.Status = "Interviewing";
            }

            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<List<InterviewSchedule>> GetInterviewsByApplicationIdAsync(Guid applicationId)
        {
            return await _context.InterviewSchedules
                .Include(i => i.Application)
                .Where(i => i.ApplicationId == applicationId)
                .OrderBy(i => i.ScheduledFor).ToListAsync();
        }

        public async Task<InterviewSchedule> UpdateInterviewStatusAsync(Guid id, string status, string? notes = null)
        {
            var interview = await _context.InterviewSchedules
                .Include(i => i.Application)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new InvalidOperationException("Interview not found.");
            interview.Status = status;
            if (notes != null) interview.Notes = notes;
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<OfferLetter> CreateOfferLetterAsync(Guid applicationId, CreateOfferLetterDto dto)
        {
            var app = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new InvalidOperationException("Application not found.");
            if (dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= DateTime.UtcNow)
                throw new InvalidOperationException("Offer expiry must be in the future.");
            if (dto.StartDate <= DateTime.UtcNow.Date)
                throw new InvalidOperationException("Offer start date must be in the future.");

            var offer = new OfferLetter
            {
                ApplicationId = applicationId, CompanyName = dto.CompanyName, Position = dto.Position,
                Salary = ParseSalary(dto.Salary),
                Benefits = dto.Benefits, Content = dto.Content,
                Status = "Draft", StartDate = dto.StartDate, ExpiresAt = dto.ExpiresAt
            };
            _context.OfferLetters.Add(offer);
            app.Status = "Offer";
            await _context.SaveChangesAsync();
            offer.Application = app;
            return offer;
        }

        public async Task<OfferLetter?> GetOfferLetterByApplicationIdAsync(Guid applicationId)
        {
            return await _context.OfferLetters.Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.ApplicationId == applicationId);
        }

        public async Task<OfferLetter> UpdateOfferLetterStatusAsync(Guid id, string status)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new InvalidOperationException("Offer letter not found.");
            offer.Status = status;
            if (status == "Sent") offer.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return offer;
        }

        public async Task<OfferLetter> RespondToOfferAsync(Guid id, string response, string? comments = null)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new InvalidOperationException("Offer letter not found.");
            if (offer.ExpiresAt.HasValue && offer.ExpiresAt.Value < DateTime.UtcNow)
                throw new InvalidOperationException("This offer has expired.");

            offer.Status = response switch
            {
                "Accept" => "Accepted",
                "Decline" => "Declined",
                _ => throw new InvalidOperationException("Offer response must be Accept or Decline.")
            };
            if (response == "Accept") offer.SignedAt = DateTime.UtcNow;
            if (offer.Application != null)
                offer.Application.Status = response == "Accept" ? "Hired" : "Rejected";
            await _context.SaveChangesAsync();
            return offer;
        }

        public async Task<ApplicantQuery> CreateQueryAsync(Guid applicationId, string message, string fromType, Guid? fromUserId = null)
        {
            var applicationExists = await _context.JobApplications.AnyAsync(a => a.Id == applicationId);
            if (!applicationExists)
                throw new InvalidOperationException("Application not found.");

            var query = new ApplicantQuery { ApplicationId = applicationId, Message = message, MessageFrom = fromType, FromUserId = fromUserId };
            _context.ApplicantQueries.Add(query);
            await _context.SaveChangesAsync();
            return query;
        }

        public async Task<List<ApplicantQuery>> GetQueriesByApplicationIdAsync(Guid applicationId)
        {
            return await _context.ApplicantQueries.Include(q => q.FromUser)
                .Where(q => q.ApplicationId == applicationId).OrderBy(q => q.CreatedAt).ToListAsync();
        }

        public async Task<RecruitmentStatsDto> GetRecruitmentStatsAsync()
        {
            var statusBreakdown = new Dictionary<string, int>
            {
                { "New", await _context.JobApplications.CountAsync(a => a.Status == "New") },
                { "Under Review", await _context.JobApplications.CountAsync(a => a.Status == "Under Review") },
                { "Shortlisted", await _context.JobApplications.CountAsync(a => a.Status == "Shortlisted") },
                { "Interviewing", await _context.JobApplications.CountAsync(a => a.Status == "Interviewing") },
                { "Offer", await _context.JobApplications.CountAsync(a => a.Status == "Offer") },
                { "Hired", await _context.JobApplications.CountAsync(a => a.Status == "Hired") },
                { "Rejected", await _context.JobApplications.CountAsync(a => a.Status == "Rejected") }
            };

            var hasScores = await _context.ApplicationScores.AnyAsync();
            var averageMatchScore = hasScores
                ? Math.Round(await _context.ApplicationScores.AverageAsync(s => s.OverallMatch), 2)
                : 0;

            return new RecruitmentStatsDto
            {
                TotalJobPostings = await _context.JobPostings.CountAsync(),
                ActiveJobPostings = await _context.JobPostings.CountAsync(p => p.Status == "Active"),
                TotalApplications = await _context.JobApplications.CountAsync(),
                ApplicationsByStatus = statusBreakdown.Values.Sum(),
                StatusBreakdown = statusBreakdown,
                ShortlistedCount = await _context.JobApplications.CountAsync(a => a.Status == "Shortlisted"),
                InterviewCount = await _context.InterviewSchedules.CountAsync(i => i.Status == "Scheduled"),
                OfferCount = await _context.OfferLetters.CountAsync(),
                HiredCount = await _context.JobApplications.CountAsync(a => a.Status == "Hired"),
                RejectedCount = await _context.JobApplications.CountAsync(a => a.Status == "Rejected"),
                AverageMatchScore = averageMatchScore
            };
        }

        public async Task<PublicRecruitmentStatsDto> GetPublicRecruitmentStatsAsync()
        {
            var companyName = _configuration.GetValue<string>("CompanyName") ?? "Wigwe University";
            var activeJobs = await _context.JobPostings
                .Include(p => p.Department)
                .Where(p => p.Status == "Active" && (!p.Deadline.HasValue || p.Deadline.Value >= DateTime.UtcNow))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return new PublicRecruitmentStatsDto
            {
                OpenPositions = activeJobs.Count,
                TotalApplications = await _context.JobApplications.CountAsync(),
                FeaturedJobs = activeJobs.Take(10).Select(j => new PublicJobListingDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Location = j.Location,
                    EmploymentType = j.EmploymentType,
                    DepartmentName = j.Department?.Name,
                    Deadline = j.Deadline ?? DateTime.UtcNow.AddDays(30),
                    ApplicationsCount = j.ApplicationsCount
                }).ToList(),
                CompanyName = companyName
            };
        }

        private static void ValidateResumeUpload(Microsoft.AspNetCore.Http.IFormFile resume)
        {
            const long maxResumeSizeBytes = 5 * 1024 * 1024;
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".txt" };
            var extension = Path.GetExtension(resume.FileName);

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Unsupported resume format. Please upload PDF, DOCX, or TXT.");
            if (resume.Length > maxResumeSizeBytes)
                throw new InvalidOperationException("Resume file must be 5 MB or smaller.");
        }

        private static decimal ParseSalary(string salary)
        {
            var normalized = Regex.Replace(salary ?? string.Empty, @"[^\d.\-]", "");
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedSalary) || parsedSalary < 0)
                throw new InvalidOperationException("Salary must be a valid non-negative amount.");

            return parsedSalary;
        }

        private async Task<string> ExtractTextFromFileAsync(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".pdf" => await ExtractPdfTextAsync(filePath),
                ".docx" => await ExtractDocxTextAsync(filePath),
                ".txt" => await File.ReadAllTextAsync(filePath),
                _ => throw new InvalidOperationException("Unsupported file format. Please upload PDF, DOCX, or TXT.")
            };
        }

        private async Task<string> ExtractPdfTextAsync(string filePath)
        {
            try
            {
                // Fallback: read raw bytes as text (works for simple PDFs, or we can integrate a library later)
                var bytes = await File.ReadAllBytesAsync(filePath);
                var text = System.Text.Encoding.UTF8.GetString(bytes);
                // Extract text between /ByteString or from PDF content streams (simplified)
                var lines = text.Split('\n');
                var sb = new StringBuilder();
                var inContent = false;
                foreach (var line in lines)
                {
                    if (line.Contains("stream\r\n") || line.Contains("stream\n"))
                    {
                        inContent = true;
                        continue;
                    }
                    if (inContent && line.Trim() == "endstream")
                    {
                        inContent = false;
                        continue;
                    }
                    if (inContent)
                    {
                        // Try to extract readable text from PDF content
                        var cleaned = line.Replace("\\n", "\n").Replace("\\r", "\r");
                        sb.AppendLine(cleaned);
                    }
                }
                return sb.Length > 0 ? sb.ToString() : "[PDF content could not be fully extracted. Please use a text-based format for best results.]";
            }
            catch { return "[PDF extraction failed]"; }
        }

        private async Task<string> ExtractDocxTextAsync(string filePath)
        {
            try
            {
                // DOCX is a ZIP archive; extract and read the XML
                using var archive = System.IO.Compression.ZipFile.OpenRead(filePath);
                var part = archive.GetEntry("word/document.xml");
                if (part == null) return "[DOCX extraction failed: document.xml not found]";
                using var stream = part.Open();
                using var reader = new StreamReader(stream);
                var xmlContent = await reader.ReadToEndAsync();
                // Strip XML tags to get text content
                var text = System.Text.RegularExpressions.Regex.Replace(xmlContent, "<[^>]+>", " ");
                // Collapse multiple spaces
                text = System.Text.RegularExpressions.Regex.Replace(text, "  +", " ").Trim();
                return text;
            }
            catch { return "[DOCX extraction failed]"; }
        }
    }
}
