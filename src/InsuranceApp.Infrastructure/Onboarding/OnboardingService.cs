using System.Security.Cryptography;
using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Onboarding;

public class OnboardingService(InsuranceDbContext dbContext, IAuthService authService) : IOnboardingService
{
    public async Task<CustomerProfileResponse> CaptureProfileAsync(CaptureProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ConsentAccepted)
        {
            throw new InvalidOperationException("Consent must be accepted before profile can be saved.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await dbContext.Customers.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        var customer = existing ?? new Customer
        {
            CustomerNumber = $"CUS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}",
            Email = email
        };

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.PhoneNumber = request.PhoneNumber.Trim();
        customer.GhanaCardNumberHash = HashCardNumber(request.GhanaCardNumber);
        customer.ConsentAccepted = true;
        customer.ConsentAcceptedAtUtc = DateTime.UtcNow;

        if (existing is null)
        {
            dbContext.Customers.Add(customer);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapResponse(customer);
    }

    public async Task<CustomerProfileResponse> AgentOnboardAsync(AgentOnboardCustomerRequest request, AuthRequestContext context, string agentUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agentUserId))
        {
            throw new InvalidOperationException("Agent identity is required for assisted onboarding.");
        }

        if (!request.ConsentAccepted)
        {
            throw new InvalidOperationException("Consent must be accepted before assisted onboarding can be completed.");
        }

        request.Register.Role = "Customer";
        await authService.RegisterAsync(request.Register, context, cancellationToken);

        var profile = await CaptureProfileAsync(new CaptureProfileRequest
        {
            Email = request.Register.Email,
            FirstName = request.Register.FirstName,
            LastName = request.Register.LastName,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.Register.PhoneNumber,
            GhanaCardNumber = request.GhanaCardNumber,
            ConsentAccepted = request.ConsentAccepted
        }, cancellationToken);

        var customer = await dbContext.Customers.SingleAsync(x => x.Id == profile.CustomerId, cancellationToken);
        customer.RegisteredByAgent = true;
        customer.RegisteredByAgentId = agentUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapResponse(customer);
    }

    private static string HashCardNumber(string cardNumber)
    {
        var normalized = cardNumber.Trim().ToUpperInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    private static CustomerProfileResponse MapResponse(Customer customer)
    {
        return new CustomerProfileResponse
        {
            CustomerId = customer.Id,
            CustomerNumber = customer.CustomerNumber,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
            KycVerified = customer.KycVerified,
            ConsentAccepted = customer.ConsentAccepted,
            ConsentAcceptedAtUtc = customer.ConsentAcceptedAtUtc,
            RegisteredByAgent = customer.RegisteredByAgent,
            RegisteredByAgentId = customer.RegisteredByAgentId
        };
    }
}
