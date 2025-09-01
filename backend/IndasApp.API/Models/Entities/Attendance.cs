namespace IndasApp.API.Models.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInTime { get; set; }
        
        // --- ADD THIS NEW PROPERTY ---
        public DateTime? CheckOutTime { get; set; } // Nullable DateTime
        
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string AttendanceMode { get; set; }
        public DateTime AttendanceDate { get; set; }
    }
}