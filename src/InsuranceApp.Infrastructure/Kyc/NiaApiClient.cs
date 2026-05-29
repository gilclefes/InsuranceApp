using InsuranceApp.Application.Common;
using InsuranceApp.Contracts.Kyc;
using InsuranceApp.Infrastructure.Kyc.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InsuranceApp.Infrastructure.Kyc;

public class NiaApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<NiaApiClient> logger,
    IOptions<NiaOptions> options) : INiaApiClient
{
    private static readonly object CircuitLock = new();
    private static int _failureCount;
    private static DateTime? _circuitOpenUntilUtc;
    private static readonly Regex GhanaCardRegex = new("^GHA-\\d{9}-\\d$", RegexOptions.Compiled);
    private readonly NiaOptions _niaOptions = options.Value;

    public async Task<GhanaCardVerificationResponse> VerifyAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (_niaOptions.UseMock)
        {
            var valid = GhanaCardRegex.IsMatch(request.GhanaCardNumber.Trim());
            return new GhanaCardVerificationResponse
            {
                IsVerified = valid,
                VerificationStatus = valid ? "Verified" : "Failed",
                Message = valid
                    ? "Verification successful in mock mode."
                    : "Invalid Ghana Card format. Expected format: GHA-123456789-1."
            };
        }

        if (IsCircuitOpen())
        {
            return PendingResponse("NIA service temporarily unavailable. Verification moved to pending.");
        }

        Exception? lastException = null;
        for (var attempt = 1; attempt <= Math.Max(1, _niaOptions.RetryCount); attempt++)
        {
            try
            {
                var client = httpClientFactory.CreateClient(nameof(NiaApiClient));
                var providerRequest = new NiaVerifyRequest
                {
                    GhanaCardNumber = request.GhanaCardNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth.ToString("yyyy-MM-dd")
                };

                using var response = await client.PostAsJsonAsync("/verify", providerRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"NIA returned status code {(int)response.StatusCode}");
                }

                var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
                ValidateSignature(response, rawBody);

                var payload = ParseProviderResponse(rawBody);
                if (payload is null)
                {
                    throw new InvalidOperationException("NIA response payload was empty.");
                }

                ResetCircuit();
                return MapProviderResponse(payload);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                lastException = ex;
                logger.LogWarning(ex, "NIA verification attempt {Attempt} failed.", attempt);
            }
        }

        RegisterFailure();
        logger.LogError(lastException, "NIA verification failed after retries. Falling back to pending status.");
        return PendingResponse("NIA verification service unavailable. Request queued for retry.");
    }

    private void ValidateSignature(HttpResponseMessage response, string body)
    {
        if (!_niaOptions.EnforceSignatureValidation)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_niaOptions.SignatureSecret))
        {
            throw new InvalidOperationException("NIA signature validation is enabled but SignatureSecret is not configured.");
        }

        if (!response.Headers.TryGetValues(_niaOptions.SignatureHeaderName, out var values))
        {
            throw new InvalidOperationException("NIA response signature header missing.");
        }

        var incomingSignature = values.FirstOrDefault() ?? string.Empty;
        var expected = ComputeSignature(body, _niaOptions.SignatureSecret);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(incomingSignature.Trim()),
                Encoding.UTF8.GetBytes(expected)))
        {
            throw new InvalidOperationException("NIA response signature validation failed.");
        }
    }

    private static string ComputeSignature(string body, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static NiaVerifyResponse? ParseProviderResponse(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<NiaVerifyResponse>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static GhanaCardVerificationResponse MapProviderResponse(NiaVerifyResponse providerResponse)
    {
        var status = providerResponse.VerificationStatus
            ?? providerResponse.Status
            ?? (providerResponse.IsVerified == true ? "Verified" : "Pending");

        var normalizedStatus = status.Trim();
        var isVerified = providerResponse.IsVerified
            ?? normalizedStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase)
            || normalizedStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)
            || normalizedStatus.Equals("Matched", StringComparison.OrdinalIgnoreCase);

        var finalStatus = isVerified
            ? "Verified"
            : normalizedStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase)
                ? "Failed"
                : "Pending";

        return new GhanaCardVerificationResponse
        {
            IsVerified = isVerified,
            VerificationStatus = finalStatus,
            Message = string.IsNullOrWhiteSpace(providerResponse.Message)
                ? "NIA verification processed."
                : providerResponse.Message
        };
    }

    private bool IsCircuitOpen()
    {
        lock (CircuitLock)
        {
            if (_circuitOpenUntilUtc is null)
            {
                return false;
            }

            if (_circuitOpenUntilUtc <= DateTime.UtcNow)
            {
                _circuitOpenUntilUtc = null;
                _failureCount = 0;
                return false;
            }

            return true;
        }
    }

    private void RegisterFailure()
    {
        lock (CircuitLock)
        {
            _failureCount++;
            if (_failureCount >= Math.Max(1, _niaOptions.CircuitBreakerFailureThreshold))
            {
                _circuitOpenUntilUtc = DateTime.UtcNow.AddSeconds(Math.Max(10, _niaOptions.CircuitBreakerDurationSeconds));
                _failureCount = 0;
            }
        }
    }

    private static void ResetCircuit()
    {
        lock (CircuitLock)
        {
            _failureCount = 0;
            _circuitOpenUntilUtc = null;
        }
    }

    private static GhanaCardVerificationResponse PendingResponse(string message)
    {
        return new GhanaCardVerificationResponse
        {
            IsVerified = false,
            VerificationStatus = "Pending",
            Message = message
        };
    }
}
