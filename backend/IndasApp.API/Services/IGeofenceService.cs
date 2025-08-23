using IndasApp.API.Models.DTOs.Geofences;

namespace IndasApp.API.Services
{
    public interface IGeofenceService
    {
        Task<GeofenceDto> CreateGeofenceAsync(CreateGeofenceDto geofenceDto, int createdByUserId);
        Task<IEnumerable<GeofenceDto>> GetGeofencesForUserAsync(int userId);
    }
}