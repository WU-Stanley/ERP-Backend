
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
            IAuthRepository userRepo)
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
                return ApiResponse<LeaveRequest>.Failure("No leave policy found for this leave type.");

            int requestedDays = await _leaveDateCalculator.CalculateWorkingDaysAsync(dto.StartDate, dto.EndDate); // Optional holiday exclusion
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
                    if (approverId.HasValue)
                    {
                        await _approvalRepo.AddAsync(new LeaveRequestApproval
                        {
                            Id = Guid.NewGuid(),
                            LeaveRequestId = leaveRequest.Id,
                            ApprovalStepId = step.Id,
                            ApproverPersonId = approverId.Value,
                            Status = StatusConstants.Pending
                        });
                    }
                }

                await transaction.CommitAsync();

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
                    var dept = await _departmentRepo.GetByIdAsync(user.Employee!.Employments.FirstOrDefault()!.DepartmentId);
                    return dept?.HeadId;

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


            // Check active delegations
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
            var isDelegate = await IsUserApprover(userId, approval!.ApprovalStep!);
            if ((approval == null))
                return ApiResponse<LeaveRequestApproval>.Failure("Not authorized or approval step not found.");
            if (approval.ApproverPersonId != userId && !isDelegate)
                return ApiResponse<LeaveRequestApproval>.Failure("Not authorized or approval step not found.");

            if (approval.Status != StatusConstants.Pending)
                return ApiResponse<LeaveRequestApproval>.Failure("This step has already been processed.");
            if (approval.ApprovalStep!.StepOrder != 1)
            {
                var previousApproval = await _approvalRepo.GetByStepOrderAndApprovalFlowIdAsync(approval.ApprovalStep.ApprovalFlowId, approval.ApprovalStep.StepOrder - 1);
                // var previousApproval = await _approvalStepRepo.GetByStepOrderAndApprovalFlowIdAsync(approval.ApprovalStep.ApprovalFlowId, approval.ApprovalStep.StepOrder - 1);
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

            request.LeaveTypeId = leaveRequestCreateDto.LeaveTypeId;
            request.StartDate = leaveRequestCreateDto.StartDate;
            request.EndDate = leaveRequestCreateDto.EndDate;
            request.Reason = leaveRequestCreateDto.Reason;

            await _leaveRequestRepo.UpdateAsync(request);
            return ApiResponse<LeaveRequest>.Success("Leave request updated successfully.", request);
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