using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using WUIAM.Repositories.IRepositories; 
using WUIAM.Models;
using WUIAM.DTOs;

[ApiController]
[Route("api/[controller]")]
public class LeavePolicyController : ControllerBase
{
    private readonly ILeavePolicyRepository _policyRepo;

    public LeavePolicyController(ILeavePolicyRepository policyRepo)
    {
        _policyRepo = policyRepo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeavePolicy>>>> GetAll()
    {
        var policies = await _policyRepo.GetAllAsync();
        return Ok( ApiResponse<IEnumerable<LeavePolicy>>.Success("Leave policies retrieved successfully.", policies));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeavePolicy>> Get(Guid id)
    {
        var policy = await _policyRepo.GetByIdAsync(id);
        if (policy == null)
            return NotFound();
        return Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeavePolicy>>> Create([FromBody] LeavePolicyDto policy)
    {
        if (policy == null)
            return BadRequest("Policy data is required");
        var result = await _policyRepo.AddAsync(policy);
      if(result == null)
            return BadRequest(ApiResponse<LeavePolicy>.Failure("Failed to create leave policy")); 
        return Ok(ApiResponse<LeavePolicy>.Success("Leave policy created successfully.", result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] LeavePolicy policy)
    {
        if (id != policy.Id)
            return BadRequest("ID mismatch");

        var existing = await _policyRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _policyRepo.UpdateAsync(policy);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _policyRepo.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        await _policyRepo.DeleteAsync(id);
        return NoContent();
    }
}
