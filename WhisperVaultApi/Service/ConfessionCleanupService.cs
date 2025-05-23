using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WhisperVaultApi.Data;

namespace WhisperVaultApi.Services
{
    public class ConfessionCleanupService : BackgroundService
    {
        private readonly ILogger<ConfessionCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        public ConfessionCleanupService(ILogger<ConfessionCleanupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Confession cleanup service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var cutoff = DateTime.UtcNow.AddHours(-24);
                        var oldConfessions = await dbContext.Confessions
                            .Where(c => c.SubmittedAt <= cutoff)
                            .ToListAsync(stoppingToken);

                        if (oldConfessions.Count > 0)
                        {
                            dbContext.Confessions.RemoveRange(oldConfessions);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation($"Deleted {oldConfessions.Count} confessions older than 24 hours.");
                        }
                        else
                        {
                            _logger.LogInformation("No old confessions found for cleanup.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up confessions.");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Confession cleanup service is stopping.");
        }
    }
}
