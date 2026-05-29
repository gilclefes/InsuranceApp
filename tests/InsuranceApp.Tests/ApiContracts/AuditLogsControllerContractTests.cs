using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Audit;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class AuditLogsControllerContractTests
{
    [Fact]
    public async Task List_ShouldReturnOk_WithPagedResponse()
    {
        var service = new StubAuditReadService
        {
            QueryResult = new IdentityAuditLogQueryResponse
            {
                Page = 1,
                PageSize = 50,
                TotalCount = 1,
                Items = new[]
                {
                    new IdentityAuditLogDto
                    {
                        Id = 1,
                        Action = "Auth.Login",
                        Outcome = "Success",
                        SubjectId = "user-1",
                        Description = "ok",
                        CorrelationId = "cid",
                        IpAddress = "127.0.0.1",
                        UserAgent = "test",
                        CreatedAtUtc = DateTime.UtcNow
                    }
                }
            }
        };

        var controller = new AuditLogsController(service);
        var result = await controller.List("Auth.Login", "Success", "user-1", null, null, 1, 50, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<IdentityAuditLogQueryResponse>(ok.Value);
        Assert.Equal(1, payload.TotalCount);
        Assert.Single(payload.Items);
    }

    [Fact]
    public async Task ExportCsv_ShouldReturnCsvFile()
    {
        var service = new StubAuditReadService
        {
            CsvResult = "Id,CreatedAtUtc,Action,Outcome,SubjectId,Description,CorrelationId,IpAddress,UserAgent\n"
        };

        var controller = new AuditLogsController(service);
        var result = await controller.ExportCsv(string.Empty, string.Empty, string.Empty, null, null, CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", file.ContentType);
        Assert.StartsWith("identity-audit-", file.FileDownloadName, StringComparison.Ordinal);
    }

    private sealed class StubAuditReadService : IIdentityAuditReadService
    {
        public IdentityAuditLogQueryResponse QueryResult { get; set; } = new();
        public string CsvResult { get; set; } = string.Empty;

        public Task<IdentityAuditLogQueryResponse> QueryAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(QueryResult);

        public Task<string> ExportCsvAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(CsvResult);
    }
}
