namespace IndasApp.API.Models.DTOs.Auth
{
    public class LoginRequestDto
    {
        // Data Annotations validation ke liye.
        // [Required] ka matlab hai ki yeh field khaali nahi ho sakti.
        // [EmailAddress] check karega ki format email jaisa hai ya nahi.
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public required string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public required string Password { get; set; }
               [System.ComponentModel.DataAnnotations.Required]

        public double Latitude { get; set; }
        [System.ComponentModel.DataAnnotations.Required]

      
        public double Longitude { get; set; }
    }
}