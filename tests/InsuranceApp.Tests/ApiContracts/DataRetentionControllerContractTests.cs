using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Privacy;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class DataRetentionControllerContractTests
{
    [Fact]
    public async Task Summary_ShouldReturnOk()
    {
        var controller = new DataRetentionController(new StubDataRetentionService());
        var result = await controller.Summary(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Run_ShouldReturnOk()
    {
        var controller = new DataRetentionController(new StubDataRetentionService());
        var result = await controller.Run(CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    private sealed class StubDataRetentionService : IDataRetentionService
    {
        public Task<DataRetentionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DataRetentionSummaryResponse());

        public Task<DataRetentionRunResponse> RunRetentionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DataRetentionRunResponse());
    }
}