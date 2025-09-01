namespace IndasApp.API.Models.Entities
{
    public class Alert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AlertType { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime AlertTimestamp { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Status { get; set; }
    }
}