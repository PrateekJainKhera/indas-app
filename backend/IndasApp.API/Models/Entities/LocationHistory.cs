namespace IndasApp.API.Models.Entities
{
    public class LocationHistory
    {
        public long Id { get; set; } // long matches BIGINT
        public int UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}