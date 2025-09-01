using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IConfiguration _configuration;

        // Constructor
        public AttendanceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task MarkLoginAttendanceAsync(int userId, double latitude, double longitude)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");

            await using (var connection = new SqlConnection(connectionString))
            {
                // SQL query to insert a new attendance record
                var query = @"INSERT INTO Attendance 
                                (UserId, CheckInTime, Latitude, Longitude, AttendanceMode, AttendanceDate) 
                              VALUES 
                                (@UserId, @CheckInTime, @Latitude, @Longitude, @AttendanceMode, @AttendanceDate)";

                await using (var command = new SqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@CheckInTime", DateTime.UtcNow); // Use UTC time
                    command.Parameters.AddWithValue("@Latitude", latitude);
                    command.Parameters.AddWithValue("@Longitude", longitude);
                    command.Parameters.AddWithValue("@AttendanceMode", "LoginCheckIn"); // A specific mode for login
                    command.Parameters.AddWithValue("@AttendanceDate", DateTime.UtcNow.Date); // Just the date part

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync(); // Execute the insert command
                }
            }
        }
          public async Task EndDutyAsync(int userId, DateTime date)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            // This query finds the user's attendance record for today and updates the CheckOutTime.
            var query = @"
                UPDATE Attendance 
                SET CheckOutTime = @CheckOutTime 
                WHERE UserId = @UserId AND AttendanceDate = @AttendanceDate AND CheckOutTime IS NULL;
            ";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CheckOutTime", DateTime.UtcNow);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@AttendanceDate", date.Date);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}