using System.Security.Claims;
using IndasApp.API.Models.DTOs.Auth;
using IndasApp.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndasApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Is controller ka base URL hoga: /api/auth
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Constructor: .NET automatically IAuthService ko yahan inject karega.
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint: POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                // Hum login logic ko AuthService me delegate kar rahe hain.
                // Hum HttpContext ko pass kar rahe hain taaki service cookie set kar sake.
                var userDto = await _authService.LoginAsync(loginRequest, HttpContext);

                // Agar login successful hota hai, to 200 OK response ke saath user ki details bhejo.
                return Ok(userDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Agar AuthService ne "UnauthorizedAccessException" throw kiya (galat password/email),
                // to 401 Unauthorized response bhejo.
                Console.WriteLine("error : ",ex);
                return Unauthorized(new { message = ex.Message });
                
            }
            catch (Exception ex)
            {
                // Kisi aur unexpected error ke liye, 500 Internal Server Error bhejo.
                // Production me, is error ko log karna zaroori hai.
                Console.WriteLine("error : ",ex);
                return StatusCode(500, new { message = "unauthorized" });
            }
        }

        // Endpoint: POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize] // Yeh endpoint sirf logged-in users hi access kar sakte hain.
        public async Task<IActionResult> Logout()
        {
            // ASP.NET Core se user ko sign out karne ko kehte hain.
            // Yeh browser se authentication cookie ko clear kar dega.
            await HttpContext.SignOutAsync();
            
            return Ok(new { message = "Logged out successfully." });
        }

        // Endpoint: GET /api/auth/me
        [HttpGet("me")]
        [Authorize] // Yeh endpoint bhi sirf logged-in users hi access kar sakte hain.
        public IActionResult GetCurrentUser()
        {
            // Jab user logged in hota hai, to uski saari details (Claims) HttpContext.User me available hoti hain.
            // Hum in claims se user ki details nikal kar DTO banakar bhej sakte hain.
            var userClaims = HttpContext.User.Claims;

            var currentUser = new
            {
                Id = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                FullName = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Role = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            };

            if (currentUser.Id == null)
            {
                return Unauthorized();
            }

            return Ok(currentUser);
        }
    }
}