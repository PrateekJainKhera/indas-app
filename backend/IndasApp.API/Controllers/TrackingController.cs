using System.Security.Claims;
using IndasApp.API.Models.DTOs.Tracking;
using IndasApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All tracking endpoints require the user to be logged in.
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        // Endpoint: POST /api/tracking/ping
        // Frontend will call this repeatedly when the user is outside a geofence.
        [HttpPost("ping")]
        public async Task<IActionResult> PostLocationPing([FromBody] LocationPingDto pingDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _trackingService.LogLocationPingAsync(userId, pingDto);
            // For high-frequency endpoints, returning 204 No Content is efficient.
            // It tells the client the request was successful but there's no body to parse.
            return NoContent();
        }

        // Endpoint: POST /api/tracking/geofence-event
        // Frontend will call this when the user enters or exits a geofence.
        [HttpPost("geofence-event")]
        public async Task<IActionResult> PostGeofenceEvent([FromBody] GeofenceEventDto eventDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _trackingService.LogGeofenceEventAsync(userId, eventDto);
            return NoContent();
        }
    }
}