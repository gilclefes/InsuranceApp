using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Kyc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Tests.ApiContracts;

public class KycControllerContractTests
{
    [Fact]
    public async Task VerifyGhanaCard_ShouldReturnOk_WithContractResponse()
    {
        var expected = new GhanaCardVerificationResponse
        {
            IsVerified = false,
            VerificationStatus = "Pending",
            Message = "Provider timeout fallback"
        };

        var controller = new KycController(new StubKycService(expected), new NoopAuditLogger())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.VerifyGhanaCard(new GhanaCardVerificationRequest
        {
            GhanaCardNumber = "GHA-123456789-0",
            FirstName = "Kojo",
            LastName = "Mensah",
            DateOfBirth = new DateTime(1994, 1, 31)
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<GhanaCardVerificationResponse>(ok.Value);
        Assert.False(payload.IsVerified);
        Assert.Equal("Pending", payload.VerificationStatus);
        Assert.Equal("Provider timeout fallback", payload.Message);
    }

    private sealed class StubKycService(GhanaCardVerificationResponse response) : IKycService
    {
        public Task<GhanaCardVerificationResponse> VerifyGhanaCardAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class NoopAuditLogger : IIdentityAuditLogger
    {
        public Task LogAsync(string action, string outcome, string subjectId, string description, string correlationId, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
