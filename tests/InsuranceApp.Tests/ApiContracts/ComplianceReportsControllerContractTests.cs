using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Compliance;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class ComplianceReportsControllerContractTests
{
    [Fact]
    public async Task GetSummary_ShouldReturnOk()
    {
        var controller = new ComplianceReportsController(new StubComplianceReportingService());
        var result = await controller.GetSummary(null, null, CancellationToken.None);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ExportSummaryCsv_ShouldReturnFile()
    {
        var controller = new ComplianceReportsController(new StubComplianceReportingService());
        var result = await controller.ExportSummaryCsv(null, null, CancellationToken.None);
        Assert.IsType<FileContentResult>(result);
    }

    private sealed class StubComplianceReportingService : IComplianceReportingService
    {
        public Task<ComplianceSummaryResponse> GetSummaryAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
            => Task.FromResult(new ComplianceSummaryResponse());

        public Task<string> ExportSummaryCsvAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
            => Task.FromResult("Section,Metric,Value\n");
    }
}