using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
namespace WUIAM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramController : ControllerBase
    {
        private readonly IAcademicProgramService _programService;

        public ProgramController(IAcademicProgramService programService)
        {
            _programService = programService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AcademicProgram>>> GetById(Guid id)
        {
            var result = await _programService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AcademicProgram>>>> GetAll()
        {
            var result = await _programService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("college/{collegeId:guid}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AcademicProgram>>>> GetByCollege(Guid collegeId)
        {
            var result = await _programService.GetByCollegeAsync(collegeId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AcademicProgram>>> Create([FromBody] AcademicProgram program)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AcademicProgram>.Failure("Invalid academic program data."));

            var result = await _programService.CreateAsync(program);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AcademicProgram>>> Update(Guid id, [FromBody] AcademicProgram program)
        {
            if (id != program.ProgramId)
                return BadRequest(ApiResponse<AcademicProgram>.Failure("Program ID mismatch."));

            var result = await _programService.UpdateAsync(program);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<dynamic>>> Delete(Guid id)
        {
            var result = await _programService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
