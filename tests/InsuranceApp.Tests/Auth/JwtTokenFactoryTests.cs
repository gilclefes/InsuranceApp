using InsuranceApp.Application.Common;
using InsuranceApp.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InsuranceApp.Tests.Auth;

public class JwtTokenFactoryTests
{
    [Fact]
    public void CreateToken_ShouldEmbedUserAndRoleClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "InsuranceApp",
            Audience = "InsuranceApp.MobileAndWeb",
            Key = "unit_test_secret_key_that_is_long_enough_for_hmac",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 30
        });

        var factory = new JwtTokenFactory(options);
        var (tokenValue, _) = factory.CreateToken("user-123", "ama@example.com", "Customer");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenValue);
        var sub = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        Assert.Equal("user-123", sub);
        Assert.Equal("Customer", role);
    }
}
