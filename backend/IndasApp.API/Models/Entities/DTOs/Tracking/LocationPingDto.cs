using System.ComponentModel.DataAnnotations;

namespace IndasApp.API.Models.DTOs.Tracking
{
    public class LocationPingDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}