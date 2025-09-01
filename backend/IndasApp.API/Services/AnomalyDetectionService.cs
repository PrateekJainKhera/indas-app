using IndasApp.API.Models.Entities;
using Microsoft.Data.SqlClient;

namespace IndasApp.API.Services
{
    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly IConfiguration _configuration;

        public AnomalyDetectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }



// Replace the existing CheckAttendanceAnomaliesAsync method in AnomalyDetectionService.cs

public async Task CheckAttendanceAnomaliesAsync(DateTime date)
{
    // Step 1: Get the rules from the database.
    var settings = await GetSystemSettingsAsync();
    
    TimeSpan officialStartTime;
    try
    {
        var timeParts = settings["OfficialStartTime"].Split(':');
        officialStartTime = new TimeSpan(int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
    }
    catch (Exception) { officialStartTime = new TimeSpan(9,0,0); } // Fallback

    // Get the rule for Early Check-Out
    TimeSpan officialEndTime;
    try
    {
        var timeParts = settings["OfficialEndTime"].Split(':');
        officialEndTime = new TimeSpan(int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
    }
    catch (Exception) { officialEndTime = new TimeSpan(18,0,0); } // Fallback


    // Step 2: Get all of today's attendance records using your existing helper method.
    var attendanceRecords = await GetAttendanceRecordsForDateAsync(date);

    Console.WriteLine($"[DEBUG] Rules: Start={officialStartTime}, End={officialEndTime}. Checking {attendanceRecords.Count} records...");

    // Step 3: Loop through each record and check for anomalies.
    foreach (var record in attendanceRecords)
    {
        // --- Rule 1: Check for Late Check-In (Existing Logic) ---
        if (record.CheckInTime.TimeOfDay > officialStartTime)
        {
            var description = $"User checked in at {record.CheckInTime:HH:mm:ss}, which is after the official start time of {officialStartTime:hh\\:mm}.";
            await CreateAlertAsync(new Alert
            {
                UserId = record.UserId,
                AlertType = "LateCheckIn",
                Description = description,
                Priority = "Medium",
                ActivityDate = date.Date,
            });
            Console.WriteLine($"!!!!!! ANOMALY DETECTED (Late Check-In) for UserId: {record.UserId} !!!!!!");
        }

        // --- NEW: Rule 2: Check for Early Check-Out ---
        // We only check this if a CheckOutTime actually exists.
        if (record.CheckOutTime.HasValue && record.CheckOutTime.Value.TimeOfDay < officialEndTime)
        {
            var description = $"User checked out at {record.CheckOutTime.Value:HH:mm:ss}, which is before the official end time of {officialEndTime:hh\\:mm}.";
            await CreateAlertAsync(new Alert
            {
                UserId = record.UserId,
                AlertType = "EarlyCheckOut",
                Description = description,
                Priority = "Medium",
                ActivityDate = date.Date,
            });
            Console.WriteLine($"!!!!!! ANOMALY DETECTED (Early Check-Out) for UserId: {record.UserId} !!!!!!");
        }
    }
}       // --- Helper Methods ---

        private async Task<Dictionary<string, string>> GetSystemSettingsAsync()
        {
            var settings = new Dictionary<string, string>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            var query = "SELECT SettingKey, SettingValue FROM SystemSettings;";
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                settings[reader.GetString(0)] = reader.GetString(1);
            }
            return settings;
        }

        private async Task<List<Attendance>> GetAttendanceRecordsForDateAsync(DateTime date)
        {
            var records = new List<Attendance>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            // We need the full Attendance object, not just a few properties
            var query = "SELECT Id, UserId, CheckInTime, CheckOutTime, Latitude, Longitude, AttendanceMode, AttendanceDate FROM Attendance WHERE AttendanceDate = @Date;";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Date", date.Date);
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                records.Add(new Attendance 
                { 
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1), 
                    CheckInTime = reader.GetDateTime(2),
                    CheckOutTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    Latitude = reader.GetDecimal(4),
                    Longitude = reader.GetDecimal(5),
                    AttendanceMode = reader.GetString(6),
                    AttendanceDate = reader.GetDateTime(7)
                });
            }
            return records;
        }

        private async Task CreateAlertAsync(Alert alert)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            var query = @"
                INSERT INTO Alerts (UserId, AlertType, Description, Priority, ActivityDate, Status)
                VALUES (@UserId, @AlertType, @Description, @Priority, @ActivityDate, 'Open');";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", alert.UserId);
            command.Parameters.AddWithValue("@AlertType", alert.AlertType);
            command.Parameters.AddWithValue("@Description", alert.Description);
            command.Parameters.AddWithValue("@Priority", alert.Priority);
            command.Parameters.AddWithValue("@ActivityDate", alert.ActivityDate);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
    }
}