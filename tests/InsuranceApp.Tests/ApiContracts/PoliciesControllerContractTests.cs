using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Policies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class PoliciesControllerContractTests
{
    [Fact]
    public async Task IssueFromQuote_ShouldReturnBadRequest_WhenQuoteInvalid()
    {
        var controller = new PoliciesController(new StubPolicyIssuanceService
        {
            ExceptionToThrow = new InvalidOperationException("Quote reference not found.")
        });

        var result = await controller.IssueFromQuote(new IssuePolicyFromQuoteRequest
        {
            QuoteReference = "Q-INVALID"
        }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Quote reference not found.", ReadMessage(badRequest.Value));
    }

    [Fact]
    public async Task GetDocument_ShouldReturnNotFound_WhenPolicyMissing()
    {
        var controller = new PoliciesController(new StubPolicyIssuanceService
        {
            DocumentExceptionToThrow = new InvalidOperationException("Policy not found.")
        });

        var result = await controller.GetDocument("POL-UNKNOWN", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Dashboard_ShouldReturnOk()
    {
        var controller = new PoliciesController(new StubPolicyIssuanceService());
        var result = await controller.Dashboard(null, null, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Cancel_ShouldReturnBadRequest_WhenInvalid()
    {
        var controller = new PoliciesController(new StubPolicyIssuanceService
        {
            CancelExceptionToThrow = new InvalidOperationException("Policy is already cancelled.")
        });

        var result = await controller.Cancel("POL-1", new CancelPolicyRequest { Reason = "Duplicate" }, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static string ReadMessage(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var property = value.GetType().GetProperty("message");
        return property?.GetValue(value)?.ToString() ?? string.Empty;
    }

    private sealed class StubPolicyIssuanceService : IPolicyIssuanceService
    {
        public Exception? ExceptionToThrow { get; set; }
        public Exception? DocumentExceptionToThrow { get; set; }
        public Exception? EndorseExceptionToThrow { get; set; }
        public Exception? CancelExceptionToThrow { get; set; }

        public Task<IssuePolicyFromQuoteResponse> IssueFromQuoteAsync(IssuePolicyFromQuoteRequest request, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(new IssuePolicyFromQuoteResponse());
        }

        public Task<PolicyDocumentResponse> GetPolicyDocumentAsync(string policyNumber, CancellationToken cancellationToken = default)
        {
            if (DocumentExceptionToThrow is not null)
            {
                throw DocumentExceptionToThrow;
            }

            return Task.FromResult(new PolicyDocumentResponse
            {
                PolicyNumber = policyNumber,
                DocumentReference = "DOC-1",
                FileName = "policy.pdf",
                ContentType = "application/pdf",
                Content = [1, 2, 3]
            });
        }

        public Task<IReadOnlyCollection<PolicySummaryResponse>> GetDashboardAsync(long? customerId, string? agentUserId, string? status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PolicySummaryResponse>>(Array.Empty<PolicySummaryResponse>());

        public Task<PolicyOperationResponse> EndorsePolicyAsync(string policyNumber, EndorsePolicyRequest request, CancellationToken cancellationToken = default)
        {
            if (EndorseExceptionToThrow is not null)
            {
                throw EndorseExceptionToThrow;
            }

            return Task.FromResult(new PolicyOperationResponse { PolicyNumber = policyNumber, Status = "Active", Message = "Policy endorsement applied." });
        }

        public Task<PolicyOperationResponse> CancelPolicyAsync(string policyNumber, CancelPolicyRequest request, CancellationToken cancellationToken = default)
        {
            if (CancelExceptionToThrow is not null)
            {
                throw CancelExceptionToThrow;
            }

            return Task.FromResult(new PolicyOperationResponse { PolicyNumber = policyNumber, Status = "Cancelled", Message = "Policy cancelled." });
        }

        public Task<RenewalReminderRunResponse> RunRenewalReminderCycleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new RenewalReminderRunResponse());
    }
}
