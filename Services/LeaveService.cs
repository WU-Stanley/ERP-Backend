
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly IApprovalFlowRepository _approvalFlowRepo;
        private readonly IApprovalStepRepository _approvalStepRepo;
        private readonly ILeaveRequestApprovalRepository _approvalRepo;
        private readonly IAuthRepository _userRepo;
        private readonly IDepartmentRepository _departmentRepo;
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILeavePolicyRepository _leavePolicyRepo;
        private readonly ILeaveDateCalculator _leaveDateCalculator;
        private readonly ILeaveBalanceRepository _leaveBalanceRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WUIAMDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly INotifyService _notifyService;

        public LeaveService(
            ILeaveTypeRepository leaveTypeRepo,
            ILeaveRequestRepository leaveRequestRepo,
            IApprovalFlowRepository approvalFlowRepo,
            IApprovalStepRepository approvalStepRepo,
            ILeaveRequestApprovalRepository approvalRepo,
            IDepartmentRepository departmentRepo,
            IHttpContextAccessor httpContextAccessor,
            ILeaveRepository leaveRepository,
            ILeaveBalanceRepository leaveBalanceRepository,
            ILeaveDateCalculator leaveDateCalculator,
            ILeavePolicyRepository leavePolicyRepository,
            WUIAMDbContext dbContext,
            IAuthRepository userRepo,
            INotificationService notificationService,
            INotifyService notifyService)
        {
            _leaveTypeRepo = leaveTypeRepo;
            _leaveRequestRepo = leaveRequestRepo;
            _approvalFlowRepo = approvalFlowRepo;
            _approvalStepRepo = approvalStepRepo;
            _approvalRepo = approvalRepo;
            _userRepo = userRepo;
            _departmentRepo = departmentRepo;
            _leaveRepository = leaveRepository;
            _leaveBalanceRepo = leaveBalanceRepository;
            _leaveDateCalculator = leaveDateCalculator;
            _leavePolicyRepo = leavePolicyRepository;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _notificationService = notificationService;
            _notifyService = notifyService;
        }

        async Task<IEnumerable<LeaveType>> GetUserVisibleLeaveTypes()
        {
            var userClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(claimType: ClaimTypes.NameIdentifier);
            Guid.TryParse(userClaim, out Guid userId);
            var user =await _userRepo.FindUserByIdAsync(userId);
            var userLeaveVisibility = await _leaveTypeRepo.GetVisibleLeaveTypesForUser(user.Id);
            return userLeaveVisibility;
        }
        public async Task<ApiResponse<LeaveRequest>> ApplyForLeaveAsync(LeaveRequestCreateDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return ApiResponse<LeaveRequest>.Failure("Invalid or missing user identity.");

            var user = await _userRepo.FindUserByIdAsync(userId);
            if (user == null)
                return ApiResponse<LeaveRequest>.Failure("User not found.");

            var leaveType = await _leaveTypeRepo.GetByIdAsync(dto.LeaveTypeId);
            if (leaveType == null)
                return ApiResponse<LeaveRequest>.Failure("Leave type not found.");

            if (!await _leaveTypeRepo. MatchesVisibility(user, leaveType))
                return ApiResponse<LeaveRequest>.Failure("You are not eligible to request this leave type.");

            var policy = await _leavePolicyRepo.GetApplicablePolicyAsync(user, dto.LeaveTypeId);
            if (policy == null)
            {
                var hasActiveEmployment = user.Employee?.Employments.Any(e => e.IsActive) == true;
                return ApiResponse<LeaveRequest>.Failure(hasActiveEmployment
                    ? "No leave policy matches your employment type. Please contact HR."
                    : "Your employment profile is incomplete. Ask HR to add an active employment type before requesting leave.");
            }

            if (policy.DependentLeaveTypeId.HasValue)
            {
                var dependentBalance = await _leaveBalanceRepo.GetByUserAndTypeAsync(user.Id, policy.DependentLeaveTypeId.Value);
                double remainingDays = 0;
                if (dependentBalance != null)
                {
                    remainingDays = dependentBalance.RemainingDays;
                }
                else
                {
                    var dependentPolicy = await _leavePolicyRepo.GetApplicablePolicyAsync(user, policy.DependentLeaveTypeId.Value);
                    if (dependentPolicy != null)
                    {
                        remainingDays = dependentPolicy.AnnualEntitlement;
                    }
                }

                if (remainingDays > 0)
                {
                    return ApiResponse<LeaveRequest>.Failure($"This leave type requires you to exhaust your dependent leave type first. You have {remainingDays} days remaining of the dependent leave.");
                }
            }

            int requestedDays = await _leaveDateCalculator.CalculateWorkingDaysAsync(
                dto.StartDate,
                dto.EndDate,
                policy.IncludePublicHolidays);
            if (requestedDays <= 0)
                return ApiResponse<LeaveRequest>.Failure("Invalid leave duration.");

            if (requestedDays > policy.AnnualEntitlement && !policy.AllowNegativeBalance)
                return ApiResponse<LeaveRequest>.Failure("Requested days exceed your annual entitlement.");

            var balance = await _leaveBalanceRepo.GetByUserAndTypeAsync(user.Id, dto.LeaveTypeId);
            if (balance != null && requestedDays > balance.RemainingDays && !policy.AllowNegativeBalance)
                return ApiResponse<LeaveRequest>.Failure("Insufficient leave balance. Your leave balance is " + balance.RemainingDays + " days. ");

            var approvalFlow = await _approvalFlowRepo.GetByIdAsync(leaveType.ApprovalFlowId);
            if (approvalFlow == null)
                return ApiResponse<LeaveRequest>.Failure("Approval flow not configured for this leave type.");

            var steps =approvalFlow.Steps.Count>0?approvalFlow.Steps: await _approvalStepRepo.GetByFlowIdAsync(approvalFlow.Id);
            if (steps == null || steps.Count == 0)
                return ApiResponse<LeaveRequest>.Failure("Approval steps not defined for the selected flow.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var leaveRequest = new LeaveRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LeaveTypeId = dto.LeaveTypeId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Reason = dto.Reason,
                    Status = StatusConstants.Pending,
                    AppliedAt = DateTime.UtcNow,
                    TotalDays = requestedDays
                };

                await _leaveRequestRepo.AddAsync(leaveRequest);

                foreach (var step in steps.OrderBy(s => s.StepOrder))
                {
                    var approverId = await ResolveApprover(user, step);
                    if (!approverId.HasValue)
                    {
                        await transaction.RollbackAsync();
                        return ApiResponse<LeaveRequest>.Failure($"No approver is configured for the {step.ApproverType} approval step.");
                    }

                    await _approvalRepo.AddAsync(new LeaveRequestApproval
                    {
                        Id = Guid.NewGuid(),
                        LeaveRequestId = leaveRequest.Id,
                        ApprovalStepId = step.Id,
                        ApproverPersonId = approverId.Value,
                        Status = StatusConstants.Pending
                    });
                }

                await transaction.CommitAsync();

                var approvals = await _approvalRepo.GetByLeaveRequestIdAsync(leaveRequest.Id);
                foreach (var approval in approvals)
                {
                    var approver = await _userRepo.FindUserByIdAsync(approval.ApproverPersonId);
                    var approverName = approver?.FullName ?? "the assigned approver";

                    await _notificationService.NotifyUserAsync(
                        approval.ApproverPersonId,
                        "New leave request awaiting approval",
                        $"{user.FullName} submitted a {leaveType.Name} leave request from {dto.StartDate:dd MMM yyyy} to {dto.EndDate:dd MMM yyyy}. Please review it.",
                        "action_required",
                        "LeaveRequest",
                        leaveRequest.Id);
                }

                await _notificationService.NotifyUserAsync(
                    user.Id,
                    "Leave request submitted",
                    $"Your {leaveType.Name} leave request from {dto.StartDate:dd MMM yyyy} to {dto.EndDate:dd MMM yyyy} has been submitted and is pending approval.",
                    "info",
                    "LeaveRequest",
                    leaveRequest.Id);

                try
                {
                    await _notifyService.SendEmailAsync(
                        new List<EmailReceiver> { new() { Email = user.UserEmail, Name = user.FullName } },
                        "Leave request submitted",
                        EmailTemplateService.GenerateLeaveRequestSubmittedEmailHtml(
                            user.FullName,
                            leaveType.Name,
                            dto.StartDate.ToString("dd MMM yyyy"),
                            dto.EndDate.ToString("dd MMM yyyy"),
                            requestedDays));
                }
                catch
                {
                }

                foreach (var approval in approvals)
                {
                    var approver = await _userRepo.FindUserByIdAsync(approval.ApproverPersonId);
                    if (approver != null && !string.IsNullOrWhiteSpace(approver.UserEmail))
                    {
                        try
                        {
                            await _notifyService.SendEmailAsync(
                                new List<EmailReceiver> { new() { Email = approver.UserEmail, Name = approver.FullName } },
                                "New leave request awaiting your approval",
                                $@"<html><body><p>Hello {approver.FullName},</p><p>{user.FullName} submitted a {leaveType.Name} leave request from {dto.StartDate:dd MMM yyyy} to {dto.EndDate:dd MMM yyyy}.</p><p>Please review and act on it in the ERP system.</p></body></html>");
                        }
                        catch
                        {
                        }
                    }
                }

                return ApiResponse<LeaveRequest>.Success("Leave request submitted successfully.", leaveRequest);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log exception here
                return ApiResponse<LeaveRequest>.Failure("An error occurred while submitting your leave request.");
            }
        }

        private async Task<Guid?> ResolveApprover(User user, ApprovalStep step)
        {
            switch (step.ApproverType)
            {
                case "MANAGER":
                    var activeEmployment = user.Employee?.Employments.FirstOrDefault(e => e.IsActive)
                        ?? user.Employee?.Employments.FirstOrDefault();
                    if (activeEmployment == null)
                    {
                        return null;
                    }

                    var dept = await _departmentRepo.GetByIdAsync(activeEmployment.DepartmentId);
                    return dept?.Head?.UserId;

                case "USER":
                    return Guid.TryParse(step.ApproverValue, out var uid) ? uid : null;

                case "ROLE":
                    var usersWithRole = await _userRepo.GetUsersByRoleAsync(step.ApproverValue);
                    var firstUser = usersWithRole.FirstOrDefault();
                    return firstUser?.Id;

            }

            return null;


        }
        public async Task<bool> IsUserApprover(Guid userId, ApprovalStep step)
        {
            var now = DateTime.UtcNow;

            // First check if the user IS the actual approver
            if (await IsActualApprover(userId, step))
                return true;

            // Then check active delegations
            var delegations = await _dbContext.ApprovalDelegations
                .Where(d =>
                    d.StartDate <= now && d.EndDate >= now)
                .ToListAsync();

            var isDelegate = delegations.Any(d =>
                d.DelegatePersonId == userId &&
                (
                    d.ApprovalStepId == step.Id ||                      // Step-specific
                    (d.ApprovalStepId == null && d.ApprovalFlowId == step.ApprovalFlowId) || // Flow-level
                    (d.ApprovalStepId == null && d.ApprovalFlowId == null) // Global
                ));

            return isDelegate;
        }

        private async Task<bool> IsActualApprover(Guid userId, ApprovalStep step)
        {
            // Check if the user matches the approver criteria
            switch (step.ApproverType)
            {
                case "USER":
                    return Guid.TryParse(step.ApproverValue, out var uid) && uid == userId;

                case "ROLE":
                    var usersWithRole = await _userRepo.GetUsersByRoleAsync(step.ApproverValue);
                    return usersWithRole.Any(u => u.Id == userId);

                case "MANAGER":
                    // Check if this user is the head of any active department
                    var deptHeads = await _dbContext.EmploymentDetails
                        .Where(ed => ed.IsActive && ed.Department != null && ed.Department.HeadId == userId)
                        .Select(ed => ed.Employee!.UserId)
                        .Distinct()
                        .ToListAsync();
                    return deptHeads.Contains(userId);

                default:
                    return false;
            }
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByUserAsync(Guid userId)
        {
            return await _leaveRequestRepo.GetByUserIdAsync(userId);
        }

        public async Task<ApiResponse<LeaveRequestApproval>> ApproveOrRejectStepAsync(Guid approvalId, ApprovalDecisionDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return ApiResponse<LeaveRequestApproval>.Failure("Invalid or missing user identity.");

            var approval = await _approvalRepo.GetByIdAsync(approvalId);
            if (approval == null || approval.ApprovalStep == null)
                return ApiResponse<LeaveRequestApproval>.Failure("Not authorized or approval step not found.");
            var isDelegate = await IsUserApprover(userId, approval.ApprovalStep);
            if (approval.ApproverPersonId != userId && !isDelegate)
                return ApiResponse<LeaveRequestApproval>.Failure("Not authorized or approval step not found.");

            if (approval.Status != StatusConstants.Pending)
                return ApiResponse<LeaveRequestApproval>.Failure("This step has already been processed.");
            if (approval.ApprovalStep!.StepOrder != 1)
            {
                // Scope previous-step check to the SAME leave request, not just the same approval flow
                var requestApprovals = await _approvalRepo.GetByLeaveRequestIdAsync(approval.LeaveRequestId);
                var previousApproval = requestApprovals.FirstOrDefault(a =>
                    a.LeaveRequestId == approval.LeaveRequestId &&
                    a.ApprovalStep?.StepOrder == approval.ApprovalStep.StepOrder - 1);
                if (previousApproval == null || previousApproval.Status != StatusConstants.Approved)
                    return ApiResponse<LeaveRequestApproval>.Failure("Previous step must be approved before processing this step.");
            }
            var transaction = await _dbContext.Database.BeginTransactionAsync(); // Optional depending on your architecture

            try
            {
                approval.Status = dto.IsApproved ? StatusConstants.Approved : StatusConstants.Rejected;
                approval.Comment = dto.Comment;
                approval.ActedByUserId = userId;
                approval.DecisionAt = DateTime.UtcNow;

                await _approvalRepo.UpdateAsync(approval);

                var request = await _leaveRequestRepo.GetByIdAsync(approval.LeaveRequestId);
                if (request == null)
                    return ApiResponse<LeaveRequestApproval>.Failure("Associated leave request not found.");

                if (!dto.IsApproved)
                {
                    request.Status = StatusConstants.Rejected;
                }
                else
                {
                    var allSteps = await _approvalRepo.GetByLeaveRequestIdAsync(request.Id);
                    if (allSteps.All(s => s.Status == StatusConstants.Approved))
                    {
                        request.Status = StatusConstants.Approved;
                        // update leave balance
                        await _leaveRepository.CreateLeaveFromApprovedRequestAsync(request);
                        // Notify requester of approval
                    }
                }
                // notify requester of approval/rejection

                await _leaveRequestRepo.UpdateAsync(request);

                var requester = await _userRepo.FindUserByIdAsync(request.UserId);
                var requesterName = requester?.FullName ?? "the requester";
                var decisionText = dto.IsApproved ? "approved" : "rejected";
                var decisionTone = dto.IsApproved ? "success" : "warning";

                await _notificationService.NotifyUserAsync(
                    request.UserId,
                    $"Leave request {decisionText}",
                    $"Your {request.LeaveType?.Name ?? "leave"} request from {request.StartDate:dd MMM yyyy} to {request.EndDate:dd MMM yyyy} has been {decisionText}.",
                    decisionTone,
                    "LeaveRequest",
                    request.Id);

                if (requester != null && !string.IsNullOrWhiteSpace(requester.UserEmail))
                {
                    try
                    {
                        if (dto.IsApproved)
                        {
                            await _notifyService.SendEmailAsync(
                                new List<EmailReceiver> { new() { Email = requester.UserEmail, Name = requester.FullName } },
                                "Leave request approved",
                                EmailTemplateService.GenerateLeaveApprovedEmailHtml(
                                    requester.FullName,
                                    request.LeaveType?.Name ?? "Leave",
                                    request.StartDate.ToString("dd MMM yyyy"),
                                    request.EndDate.ToString("dd MMM yyyy"),
                                    request.TotalDays));
                        }
                        else
                        {
                            await _notifyService.SendEmailAsync(
                                new List<EmailReceiver> { new() { Email = requester.UserEmail, Name = requester.FullName } },
                                "Leave request rejected",
                                EmailTemplateService.GenerateLeaveRejectedEmailHtml(
                                    requester.FullName,
                                    request.LeaveType?.Name ?? "Leave",
                                    request.StartDate.ToString("dd MMM yyyy"),
                                    request.EndDate.ToString("dd MMM yyyy"),
                                    dto.Comment ?? "No reason provided."));
                        }
                    }
                    catch
                    {
                    }
                }

                if (dto.IsApproved && request.Status != StatusConstants.Approved)
                {
                    var pendingApprovals = await _approvalRepo.GetByLeaveRequestIdAsync(request.Id);
                    foreach (var nextApproval in pendingApprovals.Where(a => a.Status == StatusConstants.Pending && a.ApprovalStep != null && a.ApprovalStep.StepOrder > approval.ApprovalStep!.StepOrder).OrderBy(a => a.ApprovalStep!.StepOrder))
                    {
                        await _notificationService.NotifyUserAsync(
                            nextApproval.ApproverPersonId,
                            "Leave approval pending",
                            $"{requesterName}'s leave request is awaiting your approval.",
                            "action_required",
                            "LeaveRequest",
                            request.Id);
                    }
                }

                await _dbContext.Database.CommitTransactionAsync(); // Optional depending on your architecture

                return ApiResponse<LeaveRequestApproval>.Success("Step processed successfully.", approval);
            }
            catch (Exception ex)
            {
                await _dbContext.Database.RollbackTransactionAsync(); // Optional
                                                                      // Log ex
                return ApiResponse<LeaveRequestApproval>.Failure("An error occurred while processing the request." + ex);
            }
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync()
        {
            return await _leaveRequestRepo.GetAllAsync();
        }


        public async Task<LeaveType> CreateLeaveType(CreateLeaveTypeDto dto)
        {
            var leaveType = new LeaveType
            {
                Id = Guid.NewGuid(),
                IsPaid = dto.IsPaid,
                MaxDays = dto.MaxDays,
                Name = dto.Name,
            };

            await _leaveTypeRepo.AddAsync(leaveType);
            return leaveType;
        }
        public async Task<ApprovalFlow?> CreateApprovalFlow(CreateApprovalFlowDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return null;

            var approvalFlow = new ApprovalFlow
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                IsActive = dto.IsActive,
                CreatedBy = userId!,
                CreatedAt = DateTime.UtcNow,
                VisibilityJson = dto.VisibilityJson,
                Steps = new List<ApprovalStep>()
            };

            if (dto.Steps != null)
            {
                int order = 1;
                foreach (var stepDto in dto.Steps)
                {
                    ApprovalStep? step;
                    if (stepDto.StepOrder == null)
                    {
                        step = new ApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        ApprovalFlowId = approvalFlow.Id,
                        StepOrder = order++,
                        ApproverType = stepDto.ApproverType,
                        ApproverValue = stepDto.ApproverValue,
                        ConditionJson = stepDto.ConditionJson
                    };
                    }
                    else
                    {
                        step = new ApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        ApprovalFlowId = approvalFlow.Id,
                        StepOrder = stepDto.StepOrder,
                        ApproverType = stepDto.ApproverType,
                        ApproverValue = stepDto.ApproverValue,
                        ConditionJson = stepDto.ConditionJson
                    };
                    }

                    approvalFlow.Steps.Add(step);
                }
            }

            await _approvalFlowRepo.AddAsync(approvalFlow);
            return approvalFlow;
        }

        public async Task<ApiResponse<ApprovalDelegation>> DelegateApprovalAsync(ApprovalDelegationDto approvalDelegationDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return ApiResponse<ApprovalDelegation>.Failure("Invalid or missing user identity.");

            var approver = await _userRepo.FindUserByIdAsync((Guid)(approvalDelegationDto.ApproverPersonId != null ? approvalDelegationDto.ApproverPersonId : userId));
            if (approver == null)
                return ApiResponse<ApprovalDelegation>.Failure("Approver not found.");

            var delegatePerson = await _userRepo.FindUserByIdAsync(approvalDelegationDto.DelegatePersonId);
            if (delegatePerson == null)
                return ApiResponse<ApprovalDelegation>.Failure("Delegate person not found.");
            // var requestApproval = await _approvalRepo.GetByApprovalFlowIdAndApprovalPersonId(approvalDelegationDto.ApprovalFlowId, (Guid)(approvalDelegationDto.ApproverPersonId != null ? approvalDelegationDto.ApproverPersonId : userId));
            var delegation = new ApprovalDelegation
            {
                Id = approvalDelegationDto.Id ?? Guid.NewGuid(),
                ApproverPersonId = (Guid)(approvalDelegationDto.ApproverPersonId != null ? approvalDelegationDto.ApproverPersonId : userId),
                DelegatePersonId = approvalDelegationDto.DelegatePersonId,
                ApprovalFlowId = approvalDelegationDto.ApprovalFlowId,
                ApprovalStepId = approvalDelegationDto.ApprovalStepId,
                StartDate = approvalDelegationDto.StartDate,
                EndDate = approvalDelegationDto.EndDate,
                Notes = approvalDelegationDto.Notes,
            };

            _dbContext.ApprovalDelegations.Add(delegation);
            _dbContext.SaveChanges();

            return ApiResponse<ApprovalDelegation>.Success("Approval delegation created successfully.", delegation);
        }
        public async Task<ApiResponse<ApprovalDelegation>> RevokeApprovalDelegationAsync(Guid approvalDelegationId)
        {
            var delegation = await _dbContext.ApprovalDelegations.FindAsync(approvalDelegationId);
            if (delegation == null)
                return ApiResponse<ApprovalDelegation>.Failure("Delegation not found.");

            _dbContext.ApprovalDelegations.Remove(delegation);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<ApprovalDelegation>.Success("Approval delegation revoked successfully.", delegation);
        }
        public async Task<ApiResponse<LeaveRequest>> UpdateLeaveRequestAsync(Guid id, LeaveRequestCreateDto leaveRequestCreateDto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return ApiResponse<LeaveRequest>.Failure("Invalid or missing user identity.");

            var request = await _leaveRequestRepo.GetByIdAsync(id);
            if (request == null || request.UserId != userId)
                return ApiResponse<LeaveRequest>.Failure("Leave request not found or unauthorized.");
            if (request.Status != StatusConstants.Pending)
                return ApiResponse<LeaveRequest>.Failure("Only pending requests can be updated.");

            if (request.LeaveTypeId != leaveRequestCreateDto.LeaveTypeId)
                return ApiResponse<LeaveRequest>.Failure("The leave type cannot be changed after submission. Withdraw this request and create a new one instead.");

            var user = await _userRepo.FindUserByIdAsync(userId);
            if (user == null)
                return ApiResponse<LeaveRequest>.Failure("User not found.");

            var policy = await _leavePolicyRepo.GetApplicablePolicyAsync(user, request.LeaveTypeId);
            if (policy == null)
                return ApiResponse<LeaveRequest>.Failure("No applicable leave policy was found.");

            var requestedDays = await _leaveDateCalculator.CalculateWorkingDaysAsync(
                leaveRequestCreateDto.StartDate,
                leaveRequestCreateDto.EndDate,
                policy.IncludePublicHolidays);
            if (requestedDays <= 0)
                return ApiResponse<LeaveRequest>.Failure("Invalid leave duration.");

            var balance = await _leaveBalanceRepo.GetByUserAndTypeAsync(userId, request.LeaveTypeId);
            if (balance != null && requestedDays > balance.RemainingDays && !policy.AllowNegativeBalance)
                return ApiResponse<LeaveRequest>.Failure($"Insufficient leave balance. Your leave balance is {balance.RemainingDays} days.");

            request.LeaveTypeId = leaveRequestCreateDto.LeaveTypeId;
            request.StartDate = leaveRequestCreateDto.StartDate;
            request.EndDate = leaveRequestCreateDto.EndDate;
            request.Reason = leaveRequestCreateDto.Reason;
            request.TotalDays = requestedDays;

            await _leaveRequestRepo.UpdateAsync(request);
            return ApiResponse<LeaveRequest>.Success("Leave request updated successfully.", request);
        }

        public async Task<ApiResponse<LeaveRequest>> DeleteLeaveRequestAsync(Guid id)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return ApiResponse<LeaveRequest>.Failure("Invalid or missing user identity.");

            var request = await _leaveRequestRepo.GetByIdAsync(id);
            if (request == null || request.UserId != userId)
                return ApiResponse<LeaveRequest>.Failure("Leave request not found or unauthorized.");
            if (request.Status != StatusConstants.Pending)
                return ApiResponse<LeaveRequest>.Failure("Only pending requests can be withdrawn.");

            await _leaveRequestRepo.DeleteAsync(id);
            return ApiResponse<LeaveRequest>.Success("Leave request withdrawn successfully.", request);
        }

        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval>>> GetLeaveRequestApprovals(Guid leaveRequestId)
        {
            var approvals =await _approvalRepo.GetByLeaveRequestIdAsync(leaveRequestId);
            if(approvals.Count > 0)
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval>>.Success("Approvals found!", approvals);
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval>>.Failure("No Approvals found!");
        }
    }
}
