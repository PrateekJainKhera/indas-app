using System.Security.Claims;
using IndasApp.API.Models.DTOs.Auth;
using IndasApp.API.Models.DTOs.Users;
using IndasApp.API.Services.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;
        // --- CHANGE 1: Add a private variable for IAttendanceService ---
        private readonly IAttendanceService _attendanceService;

        // --- CHANGE 2: Update the constructor to accept IAttendanceService ---
        public AuthService(IConfiguration configuration, IPasswordHasher passwordHasher, IAttendanceService attendanceService)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _attendanceService = attendanceService; // Assign it
        }

        // --- CHANGE 3: Update LoginAsync to accept the full DTO ---
        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest, HttpContext httpContext)
        {
            // Step 1: Database se connection string lo.
            var connectionString = _configuration.GetConnectionString("MyConn");
            
            UserDto userDto = null;
            string storedPasswordHash = null;

            // Step 2: Database se user ko email ke basis par dhoondo.
            await using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT u.Id, u.FullName, u.Email, u.PhoneNumber, u.PasswordHash, u.IsActive, r.RoleName 
                              FROM Users u 
                              JOIN Roles r ON u.RoleId = r.Id 
                              WHERE u.Email = @Email";
                
                await using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", loginRequest.Email);
                    await connection.OpenAsync();

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userDto = new UserDto
                            {
                                Id = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Email = reader.GetString(2),
                                PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                                IsActive = reader.GetBoolean(5),
                                RoleName = reader.GetString(6)
                            };
                            storedPasswordHash = reader.GetString(4);
                        }
                    }
                }
            }

            // Step 3: Check karo ki user mila ya nahi.
            if (userDto == null || !userDto.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            
            if (string.IsNullOrEmpty(storedPasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Step 4: Password ko verify karo.
            var isPasswordValid = _passwordHasher.Verify(storedPasswordHash, loginRequest.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // --- CHANGE 4: Call the AttendanceService to mark attendance ---
            // This happens AFTER password is verified but BEFORE we sign the user in.
            await _attendanceService.MarkLoginAttendanceAsync(userDto.Id, loginRequest.Latitude, loginRequest.Longitude);


            // Step 5: User ki identity (Claims) create karo.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Email, userDto.Email),
                new Claim(ClaimTypes.Name, userDto.FullName),
                new Claim(ClaimTypes.Role, userDto.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            // Step 6: User ko sign in karo.
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Step 7: User ki details return karo.
            return userDto;
        }
    }
}