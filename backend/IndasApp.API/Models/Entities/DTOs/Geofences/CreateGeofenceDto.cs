using System.ComponentModel.DataAnnotations;

namespace IndasApp.API.Models.DTOs.Geofences
{
    public class CreateGeofenceDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public string GeofenceType { get; set; } // "Office", "Home", "ClientSite"

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [Range(10, 1000)] // Radius 10 se 1000 meter ke beech ho sakta hai
        public int RadiusInMeters { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}