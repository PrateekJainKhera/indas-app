using IndasApp.API.Models.DTOs.Reports;
using IndasApp.API.Models.Entities;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace IndasApp.API.Services
{
    internal class ProcessedEvent
    {
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string GeofenceType { get; set; } = string.Empty;
    }

    public class SummaryService : ISummaryService
    {
        private readonly IConfiguration _configuration;

        public SummaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DailySummaryDto> GetOrCreateDailySummaryAsync(int userId, DateTime date)
        {
            var events = await GetProcessedEventsAsync(userId, date);
            var (checkInTime, checkOutTime) = await GetDutyTimesAsync(userId, date);

            if (checkInTime == null)
            {
                var userNameForEmpty = await GetUserNameAsync(userId);
                return new DailySummaryDto { UserId = userId, FullName = userNameForEmpty, SummaryDate = date.Date };
            }

            var timeAtOffice = CalculateTimeInGeofence(events, "Office");
            var timeAtClientSites = CalculateTimeInGeofence(events, "ClientSite");
            var timeAtHome = CalculateTimeInGeofence(events, "Home");

            var dutyEndTime = checkOutTime ?? events.LastOrDefault()?.Timestamp ?? checkInTime.Value;
            var totalDutyDuration = dutyEndTime - checkInTime.Value;
            if (totalDutyDuration < TimeSpan.Zero) totalDutyDuration = TimeSpan.Zero;

            var totalInsideDuration = timeAtOffice + timeAtClientSites + timeAtHome;
            var timeOutside = totalDutyDuration - totalInsideDuration;
            if (timeOutside < TimeSpan.Zero) timeOutside = TimeSpan.Zero;

            var summaryToSave = new DailySummary
            {
                UserId = userId,
                SummaryDate = date.Date,
                TotalDutyHours = (decimal)totalDutyDuration.TotalHours,
                TimeAtOfficeHours = (decimal)timeAtOffice.TotalHours,
                TimeAtClientSitesHours = (decimal)timeAtClientSites.TotalHours,
                TimeAtHomeHours = (decimal)timeAtHome.TotalHours,
                TimeOutsideHours = (decimal)timeOutside.TotalHours
            };

            await SaveSummaryToTableAsync(summaryToSave);

            var userFullName = await GetUserNameAsync(userId);
            
            return new DailySummaryDto
            {
                UserId = summaryToSave.UserId,
                FullName = userFullName,
                SummaryDate = summaryToSave.SummaryDate,
                TimeAtOfficeHours = summaryToSave.TimeAtOfficeHours.Value,
                TimeAtClientSitesHours = summaryToSave.TimeAtClientSitesHours.Value,
                TimeAtHomeHours = summaryToSave.TimeAtHomeHours.Value,
                TimeOutsideHours = summaryToSave.TimeOutsideHours.Value,
                TotalDutyHours = summaryToSave.TotalDutyHours.Value
            };
        }

        // --- HELPER METHODS ---

        private async Task SaveSummaryToTableAsync(DailySummary summary)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            var query = @"
                MERGE DailySummaries AS target
                USING (SELECT @UserId AS UserId, @SummaryDate AS SummaryDate) AS source
                ON (target.UserId = source.UserId AND target.SummaryDate = source.SummaryDate)
                WHEN MATCHED THEN
                    UPDATE SET 
                        TotalDutyHours = @TotalDutyHours,
                        TimeAtOfficeHours = @TimeAtOfficeHours,
                        TimeAtClientSitesHours = @TimeAtClientSitesHours,
                        TimeAtHomeHours = @TimeAtHomeHours,
                        TimeOutsideHours = @TimeOutsideHours,
                        CalculatedAt = GETUTCDATE()
                WHEN NOT MATCHED THEN
                    INSERT (UserId, SummaryDate, TotalDutyHours, TimeAtOfficeHours, TimeAtClientSitesHours, TimeAtHomeHours, TimeOutsideHours)
                    VALUES (@UserId, @SummaryDate, @TotalDutyHours, @TimeAtOfficeHours, @TimeAtClientSitesHours, @TimeAtHomeHours, @TimeOutsideHours);
            ";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", summary.UserId);
            command.Parameters.AddWithValue("@SummaryDate", summary.SummaryDate);
            command.Parameters.AddWithValue("@TotalDutyHours", summary.TotalDutyHours ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimeAtOfficeHours", summary.TimeAtOfficeHours ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimeAtClientSitesHours", summary.TimeAtClientSitesHours ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimeAtHomeHours", summary.TimeAtHomeHours ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TimeOutsideHours", summary.TimeOutsideHours ?? (object)DBNull.Value);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task<List<ProcessedEvent>> GetProcessedEventsAsync(int userId, DateTime date)
        {
            var events = new List<ProcessedEvent>();
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);

            // --- THIS QUERY IS NOW MORE ROBUST ---
            var query = @"
                SELECT ae.EventType, ae.Timestamp, g.GeofenceType
                FROM AttendanceEvents ae
                JOIN Geofences g ON ae.GeofenceId = g.Id
                WHERE ae.UserId = @UserId 
                  AND ae.Timestamp >= @StartDate 
                  AND ae.Timestamp < @EndDate
                ORDER BY ae.Timestamp;";

            await using var command = new SqlCommand(query, connection);
            
            var startDate = date.Date; // e.g., 2025-08-25 00:00:00
            var endDate = startDate.AddDays(1); // e.g., 2025-08-26 00:00:00

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                events.Add(new ProcessedEvent
                {
                    EventType = reader.GetString(0),
                    Timestamp = reader.GetDateTime(1),
                    GeofenceType = reader.GetString(2)
                });
            }
            return events;
        }

        private TimeSpan CalculateTimeInGeofence(List<ProcessedEvent> events, string geofenceType)
        {
            TimeSpan totalDuration = TimeSpan.Zero;
            DateTime? enterTime = null;

            var filteredEvents = events.Where(e => e.GeofenceType == geofenceType).ToList();

            foreach (var ev in filteredEvents)
            {
                if (ev.EventType == "ENTER" && enterTime == null)
                {
                    enterTime = ev.Timestamp;
                }
                else if (ev.EventType == "EXIT" && enterTime != null)
                {
                    totalDuration += ev.Timestamp - enterTime.Value;
                    enterTime = null;
                }
            }
            return totalDuration;
        }

        private async Task<string> GetUserNameAsync(int userId)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            var query = "SELECT FullName FROM Users WHERE Id = @UserId";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "Unknown User";
        }

        private async Task<(DateTime? CheckIn, DateTime? CheckOut)> GetDutyTimesAsync(int userId, DateTime date)
        {
            var connectionString = _configuration.GetConnectionString("MyConn");
            await using var connection = new SqlConnection(connectionString);
            
            // --- THIS QUERY IS ALSO MORE ROBUST ---
            var query = @"
                SELECT MIN(CheckInTime), MAX(CheckOutTime) 
                FROM Attendance 
                WHERE UserId = @UserId AND AttendanceDate = @Date;"; // AttendanceDate is already just a DATE type

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Date", date.Date);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var checkIn = reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0);
                var checkOut = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                return (checkIn, checkOut);
            }

            return (null, null);
        }
    }
}