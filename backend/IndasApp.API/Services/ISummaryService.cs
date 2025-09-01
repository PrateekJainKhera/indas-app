using IndasApp.API.Models.DTOs.Reports;

namespace IndasApp.API.Services
{
    public interface ISummaryService
    {
        Task<DailySummaryDto> GetOrCreateDailySummaryAsync(int userId, DateTime date);

    }

}