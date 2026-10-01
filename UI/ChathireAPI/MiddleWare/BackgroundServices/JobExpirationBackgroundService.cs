using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DataAccessLayer.Common;
using DataAccessLayer.Repository;

namespace MiddleWare.BackgroundServices
{
    public class JobExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobExpirationBackgroundService> _logger;

        public JobExpirationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<JobExpirationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JobExpirationBackgroundService started.");

            // Run an initial check on server startup (with small delay to allow DB connection to warm up)
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                await RunJobExpirationCheckAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial job expiration check.");
            }

            // Run periodically every 24 hours
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunJobExpirationCheckAsync(stoppingToken);
            }
        }

        private async Task RunJobExpirationCheckAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Running scheduled job expiration check...");

                using var scope = _serviceProvider.CreateScope();
                var managerFactory = scope.ServiceProvider.GetRequiredService<BusinessLayer.Common.IManagerFactory>();
                var jobOpeningManager = managerFactory.Get<BusinessLayer.Manager.IJobOpeningManager>();

                var expiredCount = await jobOpeningManager.ProcessExpiredJobOpeningsAsync(30);

                if (expiredCount > 0)
                {
                    _logger.LogInformation("Job expiration check completed: {Count} job opening(s) older than 30 days marked as expired and notification emails dispatched.", expiredCount);
                }
                else
                {
                    _logger.LogInformation("Job expiration check completed: No expired job openings found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the job expiration check.");
            }
        }
    }
}
