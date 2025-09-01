namespace IndasApp.API.Models.DTOs.Alerts
{
    public class AlertDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } // Include name for UI
        public string AlertType { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime AlertTimestamp { get; set; }
        public string Status { get; set; }
    }
}