namespace IndasApp.API.Models.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public  required string FullName { get; set; }
        public  required string Email { get; set; }
        public string? PhoneNumber { get; set; } // ? indicates that it can be null
        public required string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}