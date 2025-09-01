// File Location: Models/Entities/DailySummary.cs

namespace IndasApp.API.Models.Entities
{
    public class DailySummary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime SummaryDate { get; set; }
        public decimal? TotalDutyHours { get; set; }
        public decimal? TimeAtOfficeHours { get; set; }
        public decimal? TimeAtClientSitesHours { get; set; }
        public decimal? TimeAtHomeHours { get; set; }
        public decimal? TimeOutsideHours { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}