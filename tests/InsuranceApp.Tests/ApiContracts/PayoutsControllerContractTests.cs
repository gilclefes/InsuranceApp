using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Payouts;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class PayoutsControllerContractTests
{
    [Fact]
    public async Task Initiate_ShouldReturnBadRequest_WhenInvalid()
    {
        var controller = new PayoutsController(new StubPayoutService
        {
            InitiateException = new InvalidOperationException("Only approved claims can be paid out.")
        });

        var result = await controller.Initiate(new InitiatePayoutRequest { ClaimNumber = "CLM-1" }, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetByReference_ShouldReturnNotFound_WhenMissing()
    {
        var controller = new PayoutsController(new StubPayoutService
        {
            GetException = new InvalidOperationException("Payout not found.")
        });

        var result = await controller.GetByReference("PO-404", CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task List_ShouldReturnOk()
    {
        var controller = new PayoutsController(new StubPayoutService());
        var result = await controller.List(null, null, null, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class StubPayoutService : IPayoutService
    {
        public Exception? InitiateException { get; set; }
        public Exception? GetException { get; set; }

        public Task<PayoutResponse> InitiatePayoutAsync(InitiatePayoutRequest request, CancellationToken cancellationToken = default)
        {
            if (InitiateException is not null)
            {
                throw InitiateException;
            }

            return Task.FromResult(new PayoutResponse());
        }

        public Task<PayoutResponse> GetPayoutAsync(string payoutReference, CancellationToken cancellationToken = default)
        {
            if (GetException is not null)
            {
                throw GetException;
            }

            return Task.FromResult(new PayoutResponse());
        }

        public Task<IReadOnlyCollection<PayoutResponse>> ListPayoutsAsync(string? status, string? claimNumber, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PayoutResponse>>(Array.Empty<PayoutResponse>());

        public Task<PayoutReconciliationRunResponse> RunReconciliationAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new PayoutReconciliationRunResponse());

        public Task<PayoutWebhookResponse> ProcessWebhookAsync(PayoutWebhookRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PayoutWebhookResponse());
    }
}