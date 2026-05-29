namespace InsuranceApp.Workers;

using InsuranceApp.Application.Interfaces;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private DateTime _lastRetentionRunUtc = DateTime.MinValue;

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
                var premiumService = scope.ServiceProvider.GetRequiredService<IPremiumCollectionService>();
                var policyService = scope.ServiceProvider.GetRequiredService<IPolicyIssuanceService>();
                var retentionService = scope.ServiceProvider.GetRequiredService<IDataRetentionService>();

                var dueResult = await premiumService.RunDueCollectionsAsync(stoppingToken);
                logger.LogInformation("Due premium cycle complete. Scanned={Scanned} Success={Success} Failed={Failed}", dueResult.ItemsScanned, dueResult.ItemsSucceeded, dueResult.ItemsFailed);

                var retryResult = await premiumService.RetryFailedCollectionsAsync(stoppingToken);
                logger.LogInformation("Retry premium cycle complete. Scanned={Scanned} Success={Success}", retryResult.ItemsScanned, retryResult.ItemsSucceeded);

                var result = await policyService.RunRenewalReminderCycleAsync(stoppingToken);
                logger.LogInformation("Renewal reminder cycle complete. Scanned={Scanned} Sent={Sent}", result.PoliciesScanned, result.RemindersSent);

                if (_lastRetentionRunUtc == DateTime.MinValue || (DateTime.UtcNow - _lastRetentionRunUtc) >= TimeSpan.FromHours(24))
                {
                    var retentionResult = await retentionService.RunRetentionAsync(stoppingToken);
                    _lastRetentionRunUtc = DateTime.UtcNow;

                    logger.LogInformation(
                        "Retention cycle complete. OtpDeleted={OtpDeleted} RefreshDeleted={RefreshDeleted} AuditAnonymized={AuditAnonymized} ClaimNotifAnonymized={ClaimNotifAnonymized} PremiumPayloadRedacted={PremiumPayloadRedacted} PayoutPayloadRedacted={PayoutPayloadRedacted}",
                        retentionResult.DeletedOtpChallenges,
                        retentionResult.DeletedRefreshTokens,
                        retentionResult.AnonymizedIdentityAuditRows,
                        retentionResult.AnonymizedClaimNotifications,
                        retentionResult.RedactedPremiumWebhookPayloads,
                        retentionResult.RedactedPayoutWebhookPayloads);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Renewal reminder cycle failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
