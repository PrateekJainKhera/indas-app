using System.Security.Claims;
using IndasApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All attendance actions require a user to be logged in.
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // Endpoint: POST /api/attendance/end-duty
        [HttpPost("end-duty")]
        public async Task<IActionResult> EndDuty()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            try
            {
                // We use today's date in UTC.
                await _attendanceService.EndDutyAsync(userId, DateTime.UtcNow);
                return Ok(new { message = "Duty ended successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while ending duty.");
            }
        }
    }
}