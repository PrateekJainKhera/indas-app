namespace IndasApp.API.Models.DTOs.Reports
{
    public class DailySummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } // We'll also include the user's name
        public DateTime SummaryDate { get; set; }
        public decimal TotalDutyHours { get; set; }
        public decimal TimeAtOfficeHours { get; set; }
        public decimal TimeAtClientSitesHours { get; set; }
        public decimal TimeAtHomeHours { get; set; }
        public decimal TimeOutsideHours { get; set; }
    }
}