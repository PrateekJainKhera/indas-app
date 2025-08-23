using IndasApp.API.Models.DTOs.Tracking;

namespace IndasApp.API.Services
{
    public interface ITrackingService
    {
        Task LogLocationPingAsync(int userId, LocationPingDto pingDto);
        Task LogGeofenceEventAsync(int userId, GeofenceEventDto eventDto);
    }
}