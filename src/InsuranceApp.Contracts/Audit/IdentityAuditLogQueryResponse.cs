namespace InsuranceApp.Contracts.Audit;

public sealed class IdentityAuditLogQueryResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyCollection<IdentityAuditLogDto> Items { get; set; } = Array.Empty<IdentityAuditLogDto>();
}
