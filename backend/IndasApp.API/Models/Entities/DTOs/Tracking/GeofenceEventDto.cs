using System.ComponentModel.DataAnnotations;

namespace IndasApp.API.Models.DTOs.Tracking
{
    public class GeofenceEventDto
    {
        [Required]
        public int GeofenceId { get; set; }

        [Required]
        public string EventType { get; set; } // "ENTER" or "EXIT"
    }
}