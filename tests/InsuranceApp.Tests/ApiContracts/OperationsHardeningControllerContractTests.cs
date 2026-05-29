using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Operations;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class OperationsHardeningControllerContractTests
{
    [Fact]
    public async Task Readiness_ShouldReturnOk()
    {
        var controller = new OperationsHardeningController(new StubOperationsHardeningService());
        var result = await controller.Readiness(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RunFailoverDrill_ShouldReturnOk()
    {
        var controller = new OperationsHardeningController(new StubOperationsHardeningService());
        var result = await controller.RunFailoverDrill(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class StubOperationsHardeningService : IOperationsHardeningService
    {
        public Task<OperationsReadinessSummaryResponse> GetReadinessSummaryAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new OperationsReadinessSummaryResponse());

        public Task<FailoverDrillRunResponse> RunFailoverDrillAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new FailoverDrillRunResponse());
    }
}