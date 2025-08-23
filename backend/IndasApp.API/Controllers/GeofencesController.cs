using System.Security.Claims;
using IndasApp.API.Models.DTOs.Geofences;
using IndasApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // IMPORTANT: Is controller ke saare endpoints ke liye user ka login hona zaroori hai.
    public class GeofencesController : ControllerBase
    {
        private readonly IGeofenceService _geofenceService;

        public GeofencesController(IGeofenceService geofenceService)
        {
            _geofenceService = geofenceService;
        }

        // Endpoint: POST /api/geofences
        // Naya geofence (location) create karne ke liye.
        [HttpPost]
        [Authorize(Roles = "Admin,TeamLead")] // Sirf Admin aur TeamLead hi is endpoint ko call kar sakte hain.
        public async Task<IActionResult> CreateGeofence([FromBody] CreateGeofenceDto geofenceDto)
        {
            // Logged-in user ki ID hum cookie (Claims) se nikalenge.
            // Yeh secure hai kyunki user ise badal nahi sakta.
            var createdByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var createdGeofence = await _geofenceService.CreateGeofenceAsync(geofenceDto, createdByUserId);
                // 201 Created response bhejna best practice hai jab kuch naya create hota hai.
                // Hum naye bane hue object ko bhi response me bhejte hain.
                return CreatedAtAction(nameof(GetMyGeofences), new { }, createdGeofence);
            }
            catch (Exception ex)
            {
                // In case of an error
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint: GET /api/geofences/my
        // Logged-in user ke liye saare relevant geofences fetch karne ke liye.
        [HttpGet("my")]
        // Is endpoint ko koi bhi logged-in user (TeamMember, TeamLead, Admin) access kar sakta hai.
        public async Task<IActionResult> GetMyGeofences()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var geofences = await _geofenceService.GetGeofencesForUserAsync(userId);
                return Ok(geofences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}