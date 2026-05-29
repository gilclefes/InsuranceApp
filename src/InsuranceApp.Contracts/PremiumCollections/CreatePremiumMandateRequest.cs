namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class CreatePremiumMandateRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = "MoMo";
    public string ExternalReference { get; set; } = string.Empty;
    public DateTime? FirstDueDateUtc { get; set; }
}