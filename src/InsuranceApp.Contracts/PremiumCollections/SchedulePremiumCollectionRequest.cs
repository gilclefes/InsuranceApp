namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class SchedulePremiumCollectionRequest
{
    public DateTime DueDateUtc { get; set; }
    public decimal? Amount { get; set; }
    public string? Provider { get; set; }
    public string? PaymentChannel { get; set; }
}