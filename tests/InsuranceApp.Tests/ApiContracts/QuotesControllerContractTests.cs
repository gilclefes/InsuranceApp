using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class QuotesControllerContractTests
{
    [Fact]
    public async Task GetByReference_ShouldReturnNotFound_WhenMissing()
    {
        var controller = new QuotesController(new StubQuoteService
        {
            GetException = new InvalidOperationException("Quote reference not found.")
        });

        var result = await controller.GetByReference("Q-UNKNOWN", CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Reprice_ShouldReturnBadRequest_OnInvalidState()
    {
        var controller = new QuotesController(new StubQuoteService
        {
            RepriceException = new InvalidOperationException("Issued quotes cannot be repriced.")
        });

        var result = await controller.Reprice("Q-1", CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class StubQuoteService : IQuoteService
    {
        public Exception? GetException { get; set; }
        public Exception? RepriceException { get; set; }

        public Task<GenerateQuoteResponse> GenerateQuoteAsync(GenerateQuoteRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new GenerateQuoteResponse());

        public Task<QuoteRecordResponse> GetQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
        {
            if (GetException is not null)
            {
                throw GetException;
            }

            return Task.FromResult(new QuoteRecordResponse());
        }

        public Task<IReadOnlyCollection<QuoteRecordResponse>> ListQuotesAsync(string? productCode, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<QuoteRecordResponse>>(Array.Empty<QuoteRecordResponse>());

        public Task<GenerateQuoteResponse> RepriceQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
        {
            if (RepriceException is not null)
            {
                throw RepriceException;
            }

            return Task.FromResult(new GenerateQuoteResponse());
        }

        public Task<QuoteRecordResponse> ExpireQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
            => Task.FromResult(new QuoteRecordResponse());
    }
}
