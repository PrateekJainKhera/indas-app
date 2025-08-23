namespace IndasApp.API.Models.Entities
{
    public class Geofence
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GeofenceType { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusInMeters { get; set; }
        public int? AssignedToUserId { get; set; } // Nullable int
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
    }
}