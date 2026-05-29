using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class ClaimsControllerContractTests
{
    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPolicyMissing()
    {
        var controller = new ClaimsController(new StubClaimService
        {
            CreateException = new InvalidOperationException("Policy not found.")
        });

        var result = await controller.Create(new CreateClaimRequest { PolicyNumber = "POL-404", ClaimedAmount = 100 }, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetByNumber_ShouldReturnNotFound_WhenMissing()
    {
        var controller = new ClaimsController(new StubClaimService
        {
            GetException = new InvalidOperationException("Claim not found.")
        });

        var result = await controller.GetByNumber("CLM-NA", CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task List_ShouldReturnOk()
    {
        var controller = new ClaimsController(new StubClaimService());
        var result = await controller.List(null, null, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Timeline_ShouldReturnNotFound_WhenMissing()
    {
        var controller = new ClaimsController(new StubClaimService
        {
            TimelineException = new InvalidOperationException("Claim not found.")
        });

        var result = await controller.Timeline("CLM-NO", CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SlaDashboard_ShouldReturnOk()
    {
        var controller = new ClaimsController(new StubClaimService());
        var result = await controller.SlaDashboard(null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class StubClaimService : IClaimService
    {
        public Exception? CreateException { get; set; }
        public Exception? GetException { get; set; }
        public Exception? TimelineException { get; set; }

        public Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default)
        {
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return Task.FromResult(new ClaimResponse());
        }

        public Task<ClaimResponse> GetClaimAsync(string claimNumber, CancellationToken cancellationToken = default)
        {
            if (GetException is not null)
            {
                throw GetException;
            }

            return Task.FromResult(new ClaimResponse());
        }

        public Task<IReadOnlyCollection<ClaimResponse>> ListClaimsAsync(string? status, string? assignedAdjusterId, string? policyNumber, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<ClaimResponse>>(Array.Empty<ClaimResponse>());

        public Task<ClaimResponse> AssignClaimAsync(string claimNumber, AssignClaimRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new ClaimResponse());

        public Task<ClaimResponse> ReviewClaimAsync(string claimNumber, ReviewClaimRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new ClaimResponse());

        public Task<IReadOnlyCollection<ClaimTimelineEventResponse>> GetTimelineAsync(string claimNumber, CancellationToken cancellationToken = default)
        {
            if (TimelineException is not null)
            {
                throw TimelineException;
            }

            return Task.FromResult<IReadOnlyCollection<ClaimTimelineEventResponse>>(Array.Empty<ClaimTimelineEventResponse>());
        }

        public Task<ClaimSlaDashboardResponse> GetSlaDashboardAsync(string? assignedAdjusterId, CancellationToken cancellationToken = default)
            => Task.FromResult(new ClaimSlaDashboardResponse());
    }
}