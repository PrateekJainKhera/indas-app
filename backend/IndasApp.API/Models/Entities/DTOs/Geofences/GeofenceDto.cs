namespace IndasApp.API.Models.DTOs.Geofences
{
    public class GeofenceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GeofenceType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}