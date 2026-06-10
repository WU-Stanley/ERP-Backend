using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class LeaveApprovalService : ILeaveApprovalService
    {
        ILeaveRequestApprovalRepository _leaveReqApprovalRepo;
        IApprovalStepRepository _stepRepository;
        IHttpContextAccessor _httpContextAccessor;

        public LeaveApprovalService(ILeaveRequestApprovalRepository leaveRequestApprovalRepository, IApprovalStepRepository stepRepository, IHttpContextAccessor httpContextAccessor)
        {
            _leaveReqApprovalRepo = leaveRequestApprovalRepository;
            _stepRepository = stepRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetAllRequestApprovals()
        {
            var leaveRequestApprovals =await _leaveReqApprovalRepo.GetAllRequestApprovals();
            if (leaveRequestApprovals == null || !leaveRequestApprovals.Any())
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("No leave request approvals found.");
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Success("Leave requests found!", leaveRequestApprovals);
        }
        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApprovalFlowIdAndApprovalPersonId(Guid? approvalFlowId, Guid? approvalPersonId)
        {
            var leaveRequestApprovals = await _leaveReqApprovalRepo.GetByApprovalFlowIdAndApprovalPersonId(approvalFlowId, approvalPersonId);
            if (leaveRequestApprovals == null || !leaveRequestApprovals.Any())
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("No leave request approvals found.");
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Success("Leave requests found!", leaveRequestApprovals);
        }

        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApproverDelegationPersonId(Guid? approverDelegationPersonId)
        {
            if (!approverDelegationPersonId.HasValue)
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("Approver delegation person ID is required.");
            }

            var result = await _leaveReqApprovalRepo.GetByApproverDelegationPersonIdAsync(approverDelegationPersonId.Value);
            if (result == null || !result.Any())
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("No approval delegations found.");
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Success("Approval delegations found!", result);
        }

        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval?>>> GetByApproverPersonId(Guid? approverPersonId)
        {
            if (approverPersonId == null)
            {
                var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    approverPersonId = userId;
                }
            }

            if (!approverPersonId.HasValue)
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("Approver person ID is required.");
            }

            var requestApprovals = await _leaveReqApprovalRepo.GetByApproverPersonIdAsync(approverPersonId.Value);
            if (requestApprovals == null || !requestApprovals.Any())
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Failure("No leave request approvals found.");
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval?>>.Success("Leave requests found!", requestApprovals);
        }

        public Task<ApiResponse<LeaveRequestApproval>?> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<IEnumerable<LeaveRequestApproval>>> GetLeaveRequestApprovals(Guid leaveRequestId)
        {
            if (leaveRequestId == Guid.Empty)
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval>>.Failure("Invalid leave request ID.");
            }
            var leaveRequestApprovals = await _leaveReqApprovalRepo.GetByLeaveRequestIdAsync(leaveRequestId);
            if (leaveRequestApprovals == null || !leaveRequestApprovals.Any())
            {
                return ApiResponse<IEnumerable<LeaveRequestApproval>>.Failure("No leave request approvals found.");
            }
            return ApiResponse<IEnumerable<LeaveRequestApproval>>.Success("Leave request approvals found!", leaveRequestApprovals);
        }
    }
}