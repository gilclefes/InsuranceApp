namespace InsuranceApp.Contracts.Products;

public sealed class BulkUpsertProductRiskRulesRequest
{
    public long ProductDefinitionId { get; set; }
    public IReadOnlyCollection<BulkUpsertProductRiskRuleItem> Rules { get; set; } = Array.Empty<BulkUpsertProductRiskRuleItem>();
}
