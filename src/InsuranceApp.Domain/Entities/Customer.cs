using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public long Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string GhanaCardNumberHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool KycVerified { get; set; }
    public bool ConsentAccepted { get; set; }
    public DateTime? ConsentAcceptedAtUtc { get; set; }
    public bool RegisteredByAgent { get; set; }
    public string RegisteredByAgentId { get; set; } = string.Empty;

    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
