using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalFlowController : ControllerBase
    {
        private readonly IApprovalFlowService _approvalFlowService;
        private readonly IAuthRepository _userService;

        public ApprovalFlowController(IApprovalFlowService approvalFlowService, IAuthRepository userService)
        {
            _approvalFlowService = approvalFlowService;
            _userService = userService;
        }

        // POST: api/approvalflow/create
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<ApprovalFlow>>> CreateApprovalFlow([FromBody] CreateApprovalFlowDto createApprovalFlowDto)
        {
            var result = await _approvalFlowService.CreateApprovalFlow(createApprovalFlowDto);
            if (result != null)
            {
                return ApiResponse<ApprovalFlow>.Success("Approval flow created", result);
            }
            return ApiResponse<ApprovalFlow>.Failure("Failed to create approval flow");
        }

        // GET: api/approvalflow/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ApprovalFlow>>> GetApprovalFlow(Guid id)
        {
            var result = await _approvalFlowService.GetApprovalFlow(id);
            if (result != null)
            {
                return ApiResponse<ApprovalFlow>.Success("Approval flow retrieved", result);
            }
            return ApiResponse<ApprovalFlow>.Failure("Approval flow not found");
        }

        // GET: api/approvalflow/all
        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ApprovalFlow>>>> GetAllApprovalFlows()
        {
            var result = await _approvalFlowService.GetAllApprovalFlow();
            return ApiResponse<IEnumerable<ApprovalFlow>>.Success("Approval flows retrieved", result);
        }

        // PUT: api/approvalflow/update/{id}
        [HttpPut("update/{id:guid}")]
        public async Task<ActionResult<ApiResponse<ApprovalFlow>>> UpdateApprovalFlow(Guid id, [FromBody] ApprovalFlow approvalFlow)
        {
            if (id != approvalFlow.Id)
            {
                return ApiResponse<ApprovalFlow>.Failure("ID mismatch");
            }
            var result = await _approvalFlowService.UpdateApprovalFlow(approvalFlow);
            if (result != null)
            {
                return ApiResponse<ApprovalFlow>.Success("Approval flow updated", result);
            }
            return ApiResponse<ApprovalFlow>.Failure("Failed to update approval flow");
        }

        // DELETE: api/approvalflow/delete/{id}
        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult<ApiResponse<ApprovalFlow>>> DeleteApprovalFlow(Guid id)
        {
            var result = await _approvalFlowService.DeleteApprovalFlow(id);
            if (result != null)
            {
                return ApiResponse<ApprovalFlow>.Success("Approval flow deleted", result);
            }
            return ApiResponse<ApprovalFlow>.Failure("Failed to delete approval flow");
        }
    }
}
