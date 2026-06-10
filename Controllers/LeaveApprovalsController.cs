using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Services;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveApprovalsController : ControllerBase
    {
        ILeaveApprovalService _leaveApprovalService;
        ILeaveService _leaveService;
        public LeaveApprovalsController(ILeaveApprovalService leaveApprovalService,ILeaveService leaveService)
        {
            _leaveApprovalService = leaveApprovalService;
            _leaveService = leaveService;
        }

        // GET: /api/leaveapprovals/get-all
        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestApproval>>>> GetLeaveRequestApprovals()
        {
            var result = await _leaveApprovalService.GetAllRequestApprovals();
           
                return Ok(result); 
        }

        // GET: /api/leaveapprovals/get-leave-request-approvals
        [HttpGet("{leaveRequestId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestApproval>>>> GetLeaveRequestApprovals(Guid leaveRequestId)
        {
            var result = await _leaveApprovalService.GetLeaveRequestApprovals(leaveRequestId);
            
                return Ok(result);
          
        }

        // GET: /api/leave/get-leave-request-approvals-by-approver
        [HttpGet("get-by-approver-person/{approverId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestApproval>>>> GetLeaveRequestApprovalsByApprover(Guid approverId)
        {
            var result = await _leaveApprovalService.GetByApproverPersonId(approverId);
            
                return Ok(result);
            
        }

        // GET: /api/leave/get-by-approver-delegation-person/{delegationId}
        [HttpGet("get-by-approver-delegation-person/{delegationId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestApproval>>>> GetLeaveRequestApprovalsByDelegation(Guid delegationId)
        {
            var result = await _leaveApprovalService.GetByApproverDelegationPersonId(delegationId);
           
                return Ok(result); 
        }
        // GET: /api/leave/get-by-approval-flow-and-step
        [HttpGet("get-by-approval-flow-and-step/{approvalFlowId}/{stepOrder}")]
        public async Task<ActionResult<ApiResponse<LeaveRequestApproval>>> GetByStepOrderAndApprovalFlowIdAsync(Guid approvalFlowId, int stepOrder)
        {
            var result = await _leaveApprovalService.GetByStepOrderAndApprovalFlowIdAsync(approvalFlowId, stepOrder);
            
                return Ok(result); 
        }
        // GET: /api/leave/get-by-approval-flow-and-person
        [HttpGet("get-by-approval-flow-and-person/{approvalFlowId}/{personId}")]
        public async Task<ActionResult<ApiResponse<LeaveRequestApproval>>> GetByApprovalFlowAndPersonIdAsync(Guid approvalFlowId, Guid personId)
        {
            var result = await _leaveApprovalService.GetByApprovalFlowIdAndApprovalPersonId(approvalFlowId, personId);
            
                return Ok(result); 
        }  
        //POST: api/leave/approve-leave
        [HttpPost("approve-reject-leave/{approvalId}")]
        public async Task<ActionResult<ApiResponse<LeaveRequest>>> ApproveRejectLeave([FromBody] ApprovalDecisionDto approvalDecisionDto, Guid approvalId)
        {
            var approval = await _leaveService.ApproveOrRejectStepAsync(approvalId, approvalDecisionDto);
            return Ok(approval);
        }
        //POST: /api/leaveApprovals/approve-delegate
        [HttpPost("delegate-approval")]
        public async Task<ActionResult<ApiResponse<LeaveRequest>>> DelegateApproval([FromBody] ApprovalDelegationDto approvalDelegationDto)
        {
            var approval = await _leaveService.DelegateApprovalAsync(approvalDelegationDto);
            return Ok(approval);
        }
        //DELETE: /api/LeaveApprovals/revoke-delegate
        [HttpDelete("revoke-delegate/{delegationId}")]
        public async Task<ActionResult<ApiResponse<ApprovalDelegation>>> RevokeApproval(Guid delegationId)
        {
            var result = await _leaveService.RevokeApprovalDelegationAsync(delegationId);

            return Ok(result);
        }
    }
}
