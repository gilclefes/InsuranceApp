namespace InsuranceApp.Application.Interfaces;

public interface IIdentityAuditLogger
{
    Task LogAsync(
        string action,
        string outcome,
        string subjectId,
        string description,
        string correlationId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);
}
