using InsuranceApp.Contracts.Kyc;
using InsuranceApp.Infrastructure.Kyc;

namespace InsuranceApp.Tests.Kyc;

public class MockNiaKycServiceTests
{
    [Fact]
    public async Task VerifyGhanaCard_ShouldSucceed_ForValidFormat()
    {
        var service = new MockNiaKycService();
        var response = await service.VerifyGhanaCardAsync(new GhanaCardVerificationRequest
        {
            GhanaCardNumber = "GHA-123456789-1",
            FirstName = "Kojo",
            LastName = "Owusu",
            DateOfBirth = new DateTime(1990, 1, 1)
        });

        Assert.True(response.IsVerified);
        Assert.Equal("Verified", response.VerificationStatus);
    }

    [Fact]
    public async Task VerifyGhanaCard_ShouldFail_ForInvalidFormat()
    {
        var service = new MockNiaKycService();
        var response = await service.VerifyGhanaCardAsync(new GhanaCardVerificationRequest
        {
            GhanaCardNumber = "BAD-123",
            FirstName = "Kojo",
            LastName = "Owusu",
            DateOfBirth = new DateTime(1990, 1, 1)
        });

        Assert.False(response.IsVerified);
        Assert.Equal("Failed", response.VerificationStatus);
    }
}
