using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Models;
using WUIAM.Interfaces;

namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollegeController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegeController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<College>>> GetById(Guid id)
        {
            var result = await _collegeService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<College>>>> GetAll()
        {
            var result = await _collegeService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<College>>> Create([FromBody] College college)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<College>.Failure("Invalid college data."));

            var result = await _collegeService.CreateAsync(college);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<College>>> Update(Guid id, [FromBody] College college)
        {
            if (id != college.CollegeId)
                return BadRequest(ApiResponse<College>.Failure("College ID mismatch."));

            var result = await _collegeService.UpdateAsync(college);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<dynamic>>> Delete(Guid id)
        {
            var result = await _collegeService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
