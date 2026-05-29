namespace InsuranceApp.Workers;

using InsuranceApp.Application.Interfaces;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Running scheduled premium collection retry cycle at: {time}", DateTimeOffset.UtcNow);
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var policyService = scope.ServiceProvider.GetRequiredService<IPolicyIssuanceService>();
                var result = await policyService.RunRenewalReminderCycleAsync(stoppingToken);
                logger.LogInformation("Renewal reminder cycle complete. Scanned={Scanned} Sent={Sent}", result.PoliciesScanned, result.RemindersSent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Renewal reminder cycle failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
