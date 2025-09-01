using IndasApp.API.Services;

namespace IndasApp.API.BackgroundServices
{
    // This class inherits from BackgroundService, the base class for long-running background tasks.
    public class DailySummaryJob : BackgroundService
    {
        private readonly ILogger<DailySummaryJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailySummaryJob(ILogger<DailySummaryJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Summary Job is starting.");

            // This loop ensures the service runs for the entire lifetime of the application.
            while (!stoppingToken.IsCancellationRequested)
            {
                // --- 1. Calculate the time until the next scheduled run ---
                var now = DateTime.UtcNow;
                // Schedule to run at 23:00 UTC (11 PM UTC). This is 4:30 AM in India Standard Time.
                // You can adjust this hour to whatever time you prefer for the nightly job.
                var nextRunTime = now.Date.AddHours(23); 

                if (now > nextRunTime)
                {
                    // If it's already past the scheduled time for today, schedule it for tomorrow.
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;
                _logger.LogInformation("Next summary calculation will run at: {runTime} (in {delay})", nextRunTime, delay);
                
                // --- 2. Wait (sleep) efficiently until the scheduled time ---
                await Task.Delay(delay, stoppingToken);

                // --- 3. Once the wait is over, run the job's logic ---
                _logger.LogInformation("Running nightly job: Daily Summary Calculation & Anomaly Detection...");

                // Create a 'scope' to safely use our 'scoped' services (like ISummaryService).
                using (var scope = _serviceProvider.CreateScope())
                {
                    var summaryService = scope.ServiceProvider.GetRequiredService<ISummaryService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var anomalyService = scope.ServiceProvider.GetRequiredService<IAnomalyDetectionService>();

                    try
                    {
                        // We process the data for the day that has just ended.
                        var dateToProcess = DateTime.UtcNow.Date.AddDays(-1); 
                        
                        var userIds = await userService.GetAllActiveUserIdsAsync();
                        _logger.LogInformation("Found {userCount} active users to process for date: {date}", userIds.Count, dateToProcess);

                        foreach (var userId in userIds)
                        {
                            _logger.LogInformation("Processing summary for UserId: {userId}", userId);
                            await summaryService.GetOrCreateDailySummaryAsync(userId, dateToProcess);
                        }

                        _logger.LogInformation("Checking for attendance anomalies for date: {date}", dateToProcess);
                        await anomalyService.CheckAttendanceAnomaliesAsync(dateToProcess);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the nightly job.");
                    }
                }
                
                _logger.LogInformation("Nightly job finished.");
            }
        }
    }
} 