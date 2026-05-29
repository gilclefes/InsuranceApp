using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class OtpChallenge : BaseAuditableEntity
{
    public long Id { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int MaxAttempts { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public DateTime LastSentAtUtc { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public bool IsVerified => VerifiedAtUtc is not null;
    public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;
    public bool IsLocked => LockedUntilUtc is not null && LockedUntilUtc > DateTime.UtcNow;
}
