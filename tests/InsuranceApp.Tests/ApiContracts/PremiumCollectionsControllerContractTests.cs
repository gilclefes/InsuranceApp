using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.PremiumCollections;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class PremiumCollectionsControllerContractTests
{
    [Fact]
    public async Task CreateMandate_ShouldReturnBadRequest_WhenServiceThrows()
    {
        var controller = new PremiumCollectionsController(new StubPremiumCollectionService
        {
            CreateException = new InvalidOperationException("Policy not found.")
        });

        var result = await controller.CreateMandate(new CreatePremiumMandateRequest
        {
            PolicyNumber = "POL-404"
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ListByPolicy_ShouldReturnOk()
    {
        var controller = new PremiumCollectionsController(new StubPremiumCollectionService());
        var result = await controller.ListByPolicy("POL-1", CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ProcessWebhook_ShouldReturnBadRequest_WhenInvalid()
    {
        var controller = new PremiumCollectionsController(new StubPremiumCollectionService
        {
            WebhookException = new InvalidOperationException("Webhook signature is required.")
        });

        var result = await controller.ProcessWebhook(new PremiumCollectionWebhookRequest(), CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Reconciliation_ShouldReturnOk()
    {
        var controller = new PremiumCollectionsController(new StubPremiumCollectionService());
        var result = await controller.Reconciliation(null, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class StubPremiumCollectionService : IPremiumCollectionService
    {
        public Exception? CreateException { get; set; }
        public Exception? WebhookException { get; set; }

        public Task<PremiumCollectionItemResponse> CreateMandateAsync(CreatePremiumMandateRequest request, CancellationToken cancellationToken = default)
        {
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return Task.FromResult(new PremiumCollectionItemResponse());
        }

        public Task<PremiumCollectionItemResponse> ScheduleCollectionAsync(string policyNumber, SchedulePremiumCollectionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PremiumCollectionItemResponse());

        public Task<IReadOnlyCollection<PremiumCollectionItemResponse>> ListPolicyCollectionsAsync(string policyNumber, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PremiumCollectionItemResponse>>(Array.Empty<PremiumCollectionItemResponse>());

        public Task<PremiumCollectionRunResponse> RunDueCollectionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new PremiumCollectionRunResponse());

        public Task<PremiumCollectionRunResponse> RetryFailedCollectionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new PremiumCollectionRunResponse());

        public Task<PremiumCollectionWebhookResponse> ProcessWebhookAsync(PremiumCollectionWebhookRequest request, CancellationToken cancellationToken = default)
        {
            if (WebhookException is not null)
            {
                throw WebhookException;
            }

            return Task.FromResult(new PremiumCollectionWebhookResponse());
        }

        public Task<PremiumReconciliationSummaryResponse> GetReconciliationSummaryAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
            => Task.FromResult(new PremiumReconciliationSummaryResponse());
    }
}
