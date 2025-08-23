namespace IndasApp.API.Models.Entities
{
    public class AttendanceEvent
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GeofenceId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}