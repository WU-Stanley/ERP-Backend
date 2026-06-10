using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Repositories.IRepositories;

namespace WUIAM.Services
{
    public class ApprovalFlowService : IApprovalFlowService
    {
        private IApprovalFlowRepository _approvalFlowRepo;
        private IHttpContextAccessor _context;

        public ApprovalFlowService(IApprovalFlowRepository approvalFlowRepository, IHttpContextAccessor httpContextAccessor) {
        _approvalFlowRepo = approvalFlowRepository;
            _context = httpContextAccessor;
        }
        public async Task<ApprovalFlow> CreateApprovalFlow(CreateApprovalFlowDto dto)
        {
            var userIdClaim = _context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid createdBy))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
            }

            var approvalFlow = new ApprovalFlow
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                IsActive = dto.IsActive,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                VisibilityJson = dto.VisibilityJson,
                Steps = new List<ApprovalStep>()
            };

            if (dto.Steps != null)
            {
                int order = 1;
                foreach (var stepDto in dto.Steps)
                {
                    var step = new ApprovalStep
                    {
                        Id = Guid.NewGuid(),
                        ApprovalFlowId = approvalFlow.Id,
                        StepOrder = order++,
                        ApproverType = stepDto.ApproverType,
                        ApproverValue = stepDto.ApproverValue,
                        ConditionJson = stepDto.ConditionJson
                    };
                    approvalFlow.Steps.Add(step);
                }
            }

            await _approvalFlowRepo.AddAsync(approvalFlow);
            return approvalFlow;
        }

        public async Task<ApprovalFlow> DeleteApprovalFlow(Guid id)
        {
            var approvalFlow = await _approvalFlowRepo.GetByIdAsync(id);
            if (approvalFlow == null)
            {
                throw new KeyNotFoundException($"ApprovalFlow with id {id} not found.");
            }
            await _approvalFlowRepo.DeleteAsync(id.ToString());
            return approvalFlow;
        }

        public async Task<IEnumerable<ApprovalFlow>> GetAllApprovalFlow()
        {
            var approvalFlows = await _approvalFlowRepo.GetAllApprovalFlow();
            return approvalFlows.Where(flow => flow != null).Select(flow => flow!);
        }

        public async Task<ApprovalFlow> GetApprovalFlow(Guid id)
        {
            var approvalFlow = await _approvalFlowRepo.GetByIdAsync(id);
            if (approvalFlow == null)
            {
                throw new KeyNotFoundException($"ApprovalFlow with id {id} not found.");
            }
            return approvalFlow;
        }

        public async Task<ApprovalFlow> UpdateApprovalFlow(ApprovalFlow approvalFlow)
        {
            var existing = await _approvalFlowRepo.GetByIdAsync(approvalFlow.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"ApprovalFlow with id {approvalFlow.Id} not found.");
            }
            var updated = await _approvalFlowRepo.UpdateAsync(approvalFlow);
            return updated;
        }
    }
}
