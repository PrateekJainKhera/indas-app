using IndasApp.API.Models.DTOs.Auth; // Yeh line add karein
using IndasApp.API.Models.DTOs.Users; // Yeh line add karein

namespace IndasApp.API.Services
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginRequestDto loginRequest, HttpContext httpContext);
    }
}