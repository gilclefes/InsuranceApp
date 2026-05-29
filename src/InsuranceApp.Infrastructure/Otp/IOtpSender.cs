namespace InsuranceApp.Infrastructure.Otp;

public interface IOtpSender
{
    Task SendAsync(string destination, string channel, string message, CancellationToken cancellationToken = default);
}
