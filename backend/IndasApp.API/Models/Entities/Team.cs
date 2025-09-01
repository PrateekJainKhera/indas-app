namespace IndasApp.API.Models.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public int? TeamLeadId { get; set; }
        public bool IsActive { get; set; }
    }
}