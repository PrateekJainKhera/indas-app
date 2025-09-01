using IndasApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // --- CHANGE 1: General authorization, any logged-in user can hit the controller ---
    public class ReportsController : ControllerBase
    {
        private readonly ISummaryService _summaryService;
        private readonly ISecurityService _securityService;

        public ReportsController(ISummaryService summaryService, ISecurityService securityService)
        {
            _summaryService = summaryService;
            _securityService = securityService;
        }

        [HttpGet("daily-summary")]
        public async Task<IActionResult> GetDailySummary([FromQuery] int userId, [FromQuery] DateTime date)
        {
            var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // --- CHANGE 2: New, more flexible security logic ---
            bool isAllowed = false;

            // Rule 1: Admins can see anyone's report.
            if (userRole == "Admin")
            {
                isAllowed = true;
            }
            // Rule 2: A user can always see their own report.
            else if (loggedInUserId == userId)
            {
                isAllowed = true;
            }
            // Rule 3: A TeamLead can see reports for their team members.
            else if (userRole == "TeamLead")
            {
                isAllowed = await _securityService.IsUserInLeadsTeamAsync(loggedInUserId, userId);
            }

            if (!isAllowed)
            {
                return Forbid(); // If none of the rules pass, deny access.
            }
            // --- END OF SECURITY LOGIC ---

            try
            {
                var summary = await _summaryService.GetOrCreateDailySummaryAsync(userId, date);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return StatusCode(500, "An error occurred while generating the summary.");
            }
        }
    }
}