using System.Security.Claims;
using IndasApp.API.Hubs; // --- CHANGE 1: Import the LocationHub ---
using IndasApp.API.Models.DTOs.Tracking;
using IndasApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // --- CHANGE 2: Import SignalR's HubContext ---

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _trackingService;
        // --- CHANGE 3: Add a private variable for the HubContext ---
        private readonly IHubContext<LocationHub> _locationHubContext;
                private readonly ISecurityService _securityService;


        // --- CHANGE 4: Update the constructor to accept IHubContext<LocationHub> ---
        public TrackingController(ITrackingService trackingService, IHubContext<LocationHub> locationHubContext, ISecurityService securityService)
        {
            _trackingService = trackingService;
            _locationHubContext = locationHubContext; // Assign it\
                        _securityService = securityService; // Assign it

        }

        // Endpoint: POST /api/tracking/ping
        [HttpPost("ping")]
        public async Task<IActionResult> PostLocationPing([FromBody] LocationPingDto pingDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userFullName = User.FindFirstValue(ClaimTypes.Name); // Get user's name from their claims

            // Step 1: Save the location to the database (existing logic)
            await _trackingService.LogLocationPingAsync(userId, pingDto);

            // --- CHANGE 5: Broadcast the location update via SignalR ---
            // We are calling the "SendLocationUpdate" method that we defined in our LocationHub.
            // This will send the message to all connected clients.
            await _locationHubContext.Clients.All.SendAsync("ReceiveLocationUpdate", new
            {
                userId,
                userFullName,
                pingDto.Latitude,
                pingDto.Longitude
            });

            return NoContent();
        }

        // Endpoint: POST /api/tracking/geofence-event
        // (No changes needed here for now)
       // In Controllers/TrackingController.cs

[HttpPost("geofence-event")]
public async Task<IActionResult> PostGeofenceEvent([FromBody] GeofenceEventDto eventDto)
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    var userFullName = User.FindFirstValue(ClaimTypes.Name);

    // Step 1: Save the event to the database (existing logic)
    await _trackingService.LogGeofenceEventAsync(userId, eventDto);

    // --- NEW: Broadcast the geofence event via SignalR ---
    await _locationHubContext.Clients.All.SendAsync("ReceiveGeofenceUpdate", new 
    { 
        userId,
        userFullName,
        eventDto.GeofenceId,
        eventDto.EventType // "ENTER" or "EXIT"
    });

    return NoContent();
}
        // This code goes inside the TrackingController class, after the other endpoints.

// Endpoint: GET /api/tracking/path?userId=3&date=2025-08-25
// This is the updated method for your TrackingController.cs file

[HttpGet("path")]
//[Authorize(Roles = "Admin,TeamLead")] // Only Admins and TeamLeads can view path history.
// In TrackingController.cs

// --- CHANGE 1: Remove the specific role authorization from the attribute ---
public async Task<IActionResult> GetPathHistory([FromQuery] int userId, [FromQuery] DateTime date)
{
    var loggedInUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    var userRole = User.FindFirstValue(ClaimTypes.Role);

    // --- CHANGE 2: Add the same flexible security logic ---
    bool isAllowed = false;

    if (userRole == "Admin")
    {
        isAllowed = true;
    }
    else if (loggedInUserId == userId)
    {
        isAllowed = true;
    }
    else if (userRole == "TeamLead")
    {
        isAllowed = await _securityService.IsUserInLeadsTeamAsync(loggedInUserId, userId);
    }

    if (!isAllowed)
    {
        return Forbid();
    }
    // --- END OF SECURITY LOGIC ---

    try
    {
        var pathHistory = await _trackingService.GetPathHistoryAsync(userId, date);
        return Ok(pathHistory);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "An error occurred while fetching path history.");
    }
}
    }
}