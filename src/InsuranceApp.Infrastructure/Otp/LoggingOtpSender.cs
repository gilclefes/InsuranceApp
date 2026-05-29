namespace InsuranceApp.Infrastructure.Otp;

using Microsoft.Extensions.Logging;

public class LoggingOtpSender(ILogger<LoggingOtpSender> logger) : IOtpSender
{
    public Task SendAsync(string destination, string channel, string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("OTP dispatched via {Channel} to {Destination}: {Message}", channel, destination, message);
        return Task.CompletedTask;
    }
}
