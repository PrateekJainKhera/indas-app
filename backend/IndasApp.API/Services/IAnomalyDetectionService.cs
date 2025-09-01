namespace IndasApp.API.Services
{
    public interface IAnomalyDetectionService
    {
        // This method will check all attendance records for a given day for anomalies.
        Task CheckAttendanceAnomaliesAsync(DateTime date);
    }
}