using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Products;

public sealed class BulkUpsertProductRiskRulesRequest
{
    [Required]
    [Range(1, long.MaxValue)]
    public long ProductDefinitionId { get; set; }

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<BulkUpsertProductRiskRuleItem> Rules { get; set; } = Array.Empty<BulkUpsertProductRiskRuleItem>();
}
