using InsuranceApp.Contracts.Quotes;

namespace InsuranceApp.Application.Interfaces;

public interface IQuoteService
{
    Task<GenerateQuoteResponse> GenerateQuoteAsync(GenerateQuoteRequest request, CancellationToken cancellationToken = default);
    Task<QuoteRecordResponse> GetQuoteAsync(string quoteReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<QuoteRecordResponse>> ListQuotesAsync(string? productCode, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
    Task<GenerateQuoteResponse> RepriceQuoteAsync(string quoteReference, CancellationToken cancellationToken = default);
    Task<QuoteRecordResponse> ExpireQuoteAsync(string quoteReference, CancellationToken cancellationToken = default);
}
