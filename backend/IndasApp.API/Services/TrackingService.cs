using IndasApp.API.Models.DTOs.Tracking;
using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IConfiguration _configuration;

        public TrackingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task LogLocationPingAsync(int userId, LocationPingDto pingDto)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            var query = "INSERT INTO LocationHistory (UserId, Latitude, Longitude) VALUES (@UserId, @Latitude, @Longitude);";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Latitude", pingDto.Latitude);
            command.Parameters.AddWithValue("@Longitude", pingDto.Longitude);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task LogGeofenceEventAsync(int userId, GeofenceEventDto eventDto)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            var query = "INSERT INTO AttendanceEvents (UserId, GeofenceId, EventType) VALUES (@UserId, @GeofenceId, @EventType);";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@GeofenceId", eventDto.GeofenceId);
            command.Parameters.AddWithValue("@EventType", eventDto.EventType);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        // This code goes inside the TrackingService class, after the other methods.

        public async Task<IEnumerable<LocationPointDto>> GetPathHistoryAsync(int userId, DateTime date)
        {
            var pathPoints = new List<LocationPointDto>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            var query = @"
        SELECT Latitude, Longitude, Timestamp 
        FROM LocationHistory
        WHERE UserId = @UserId AND CAST(Timestamp AS DATE) = @Date
        ORDER BY Timestamp;";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pathPoints.Add(new LocationPointDto
                {
                    // We are reading from the database as decimal and converting to double for the DTO
                    Latitude = (double)reader.GetDecimal(0),
                    Longitude = (double)reader.GetDecimal(1),
                    Timestamp = reader.GetDateTime(2)
                });
            }

            return pathPoints;
        }
    }
}