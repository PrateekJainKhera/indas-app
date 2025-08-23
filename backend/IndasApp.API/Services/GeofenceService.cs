using System.Security.Claims;
using IndasApp.API.Models.DTOs.Geofences;
using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class GeofenceService : IGeofenceService
    {
        private readonly IConfiguration _configuration;

        public GeofenceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GeofenceDto> CreateGeofenceAsync(CreateGeofenceDto geofenceDto, int createdByUserId)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            var query = @"
                INSERT INTO Geofences (Name, GeofenceType, Latitude, Longitude, RadiusInMeters, AssignedToUserId, CreatedByUserId)
                OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.GeofenceType, INSERTED.Latitude, INSERTED.Longitude, INSERTED.RadiusInMeters, INSERTED.AssignedToUserId
                VALUES (@Name, @GeofenceType, @Latitude, @Longitude, @RadiusInMeters, @AssignedToUserId, @CreatedByUserId);";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", geofenceDto.Name);
            command.Parameters.AddWithValue("@GeofenceType", geofenceDto.GeofenceType);
            command.Parameters.AddWithValue("@Latitude", geofenceDto.Latitude);
            command.Parameters.AddWithValue("@Longitude", geofenceDto.Longitude);
            command.Parameters.AddWithValue("@RadiusInMeters", geofenceDto.RadiusInMeters);
            
            // Handle nullable AssignedToUserId
            if (geofenceDto.AssignedToUserId.HasValue)
            {
                command.Parameters.AddWithValue("@AssignedToUserId", geofenceDto.AssignedToUserId.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@AssignedToUserId", DBNull.Value);
            }
            
            command.Parameters.AddWithValue("@CreatedByUserId", createdByUserId);

            await connection.OpenAsync();
            
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new GeofenceDto
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    GeofenceType = reader.GetString(2),
                    Latitude = (double)reader.GetDecimal(3),
                    Longitude = (double)reader.GetDecimal(4),
                    RadiusInMeters = reader.GetInt32(5),
                    AssignedToUserId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                };
            }
            
            throw new Exception("Failed to create geofence.");
        }

        public async Task<IEnumerable<GeofenceDto>> GetGeofencesForUserAsync(int userId)
        {
            var geofences = new List<GeofenceDto>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            // This query gets all global geofences (like offices) AND the specific user's personal geofences (like their home)
            var query = @"
                SELECT Id, Name, GeofenceType, Latitude, Longitude, RadiusInMeters, AssignedToUserId
                FROM Geofences
                WHERE IsActive = 1 AND (AssignedToUserId IS NULL OR AssignedToUserId = @UserId);";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                geofences.Add(new GeofenceDto
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    GeofenceType = reader.GetString(2),
                    Latitude = (double)reader.GetDecimal(3),
                    Longitude = (double)reader.GetDecimal(4),
                    RadiusInMeters = reader.GetInt32(5),
                    AssignedToUserId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                });
            }
            return geofences;
        }
    }
}