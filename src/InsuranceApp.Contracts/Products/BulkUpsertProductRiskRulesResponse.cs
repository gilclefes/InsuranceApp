namespace InsuranceApp.Contracts.Products;

public sealed class BulkUpsertProductRiskRulesResponse
{
    public int TotalProcessed { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public IReadOnlyCollection<ProductRiskRuleResponse> Items { get; set; } = Array.Empty<ProductRiskRuleResponse>();
}
