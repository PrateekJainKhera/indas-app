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
        // Yeh private variables hain jo doosri services ko hold karenge.
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        // Yeh Constructor hai. Jab bhi AuthService ka object banega,
        // .NET automatically IConfiguration aur IPasswordHasher ke objects isme "inject" kar dega.
        public AuthService(IConfiguration configuration, IPasswordHasher passwordHasher)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest, HttpContext httpContext)
        {
            // Step 1: Database se connection string lo.
            var connectionString = _configuration.GetConnectionString("MyConn");
            
            UserDto userDto = null;
            string storedPasswordHash = null;

            // Step 2: Database se user ko email ke basis par dhoondo.
            // Hum RoleName ko bhi saath me fetch kar rahe hain.
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
                            // Agar user milta hai, to uski details DTO me daalo.
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
                // Agar user nahi mila ya active nahi hai, to error throw karo.
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Step 4: Password ko verify karo.
            var isPasswordValid = _passwordHasher.Verify(storedPasswordHash, loginRequest.Password);
            if (!isPasswordValid)
            {
                // Agar password galat hai, to error throw karo.
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Step 5: User ki identity (Claims) create karo.
            // Claims user ke baare me "facts" hote hain (jaise ID, email, role).
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Email, userDto.Email),
                new Claim(ClaimTypes.Name, userDto.FullName),
                new Claim(ClaimTypes.Role, userDto.RoleName) // Sabse important claim: Role
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                // Isse cookie browser band hone ke baad bhi persist karegi (agar user chahe).
                IsPersistent = true
            };

            // Step 6: User ko sign in karo.
            // Yeh method automatically ek secure, encrypted cookie banakar browser ko bhej dega.
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Step 7: User ki details (bina password ke) return karo.
            return userDto;
        }
    }
}