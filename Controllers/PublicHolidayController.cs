using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WUIAM.DTOs;
using WUIAM.Interfaces;
using WUIAM.Models;
using WUIAM.Services;

namespace WUIAM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicHolidayController : ControllerBase
    {
        IPublicHolidayService _publicHolidayService;
        public PublicHolidayController(IPublicHolidayService publicHolidayService)
        {
            _publicHolidayService = publicHolidayService;
        }

        // GET: api/PublicHoliday
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PublicHoliday>>>> GetPublicHolidays()
        {
            var publicHolidays = await _publicHolidayService.GetPublicHolidaysAsync();
            return Ok(ApiResponse<IEnumerable<PublicHoliday>>.Success(
                publicHolidays.Count() <= 0 ? "No public holidays found" : "Public holidays found",
                publicHolidays));
        }
        // GET: api/PublicHoliday/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PublicHoliday>>> GetPublicHolidayById(Guid id)
        {
            var publicHoliday = await _publicHolidayService.GetPublicHolidayByIdAsync(id);
            if (publicHoliday == null)
            {
                return NotFound(ApiResponse<PublicHoliday>.Failure("Public holiday not found"));
            }
            return Ok(ApiResponse<PublicHoliday>.Success("Public holiday found", publicHoliday));
        }

        // POST: api/PublicHoliday  
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PublicHoliday>>> CreatePublicHoliday(PublicHolidayDto publicHolidayDto)
        {
            var publicHoliday = await _publicHolidayService.CreatePublicHolidayAsync(publicHolidayDto);
            return CreatedAtAction(nameof(GetPublicHolidayById), new { id = publicHoliday.Id }, ApiResponse<PublicHoliday>.Success("Public holiday created", publicHoliday));
        }

        // PUT: api/PublicHoliday/5 
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PublicHoliday>>> UpdatePublicHoliday(Guid id, PublicHolidayDto publicHolidayDto)
        {
            var publicHoliday = await _publicHolidayService.UpdatePublicHolidayAsync(id, publicHolidayDto);
            if (publicHoliday == null)
            {
                return NotFound(ApiResponse<PublicHoliday>.Failure("Public holiday not found"));
            }
            return Ok(ApiResponse<PublicHoliday>.Success("Public holiday updated", publicHoliday));
        }

        // DELETE: api/PublicHoliday/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<dynamic>>> DeletePublicHoliday(Guid id)
        {
            var result = await _publicHolidayService.DeletePublicHolidayAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<dynamic>.Failure("Public holiday not found"));
            }
            return Ok(ApiResponse<dynamic>.Success("Public holiday deleted", true));
        }
    }
}
