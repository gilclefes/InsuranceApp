using InsuranceApp.Application.Interfaces;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;

namespace InsuranceApp.Infrastructure.Identity;

public class DbIdentityAuditLogger(InsuranceDbContext dbContext) : IIdentityAuditLogger
{
    public async Task LogAsync(
        string action,
        string outcome,
        string subjectId,
        string description,
        string correlationId,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        dbContext.Set<IdentityAuditLog>().Add(new IdentityAuditLog
        {
            Action = action,
            Outcome = outcome,
            SubjectId = subjectId,
            Description = description,
            CorrelationId = correlationId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
