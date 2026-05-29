using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceApp.Api.Middleware;

/// In-memory idempotency middleware. Persists request-key → response snapshot for a TTL window so retries return the original outcome.
/// For production replace the in-memory store with a distributed cache (Redis) — see Phase 4.
public sealed class IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
{
    public const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);
    private static readonly ConcurrentDictionary<string, CachedResponse> Cache = new();

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) && !HttpMethods.IsPut(context.Request.Method))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.ToString();
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var key = keyValues.ToString().Trim();
        var bodyHash = await ComputeBodyHashAsync(context);
        var cacheKey = $"{path}::{key}::{bodyHash}";

        PurgeExpired();

        if (Cache.TryGetValue(cacheKey, out var cached))
        {
            logger.LogInformation("Idempotent replay for {Path} key={Key}", path, key);
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType;
            context.Response.Headers["Idempotent-Replay"] = "true";
            await context.Response.Body.WriteAsync(cached.Body);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        var bytes = buffer.ToArray();
        if (context.Response.StatusCode is >= 200 and < 300)
        {
            Cache[cacheKey] = new CachedResponse(context.Response.StatusCode, context.Response.ContentType ?? "application/json", bytes, DateTimeOffset.UtcNow.Add(Ttl));
        }
        await originalBody.WriteAsync(bytes);
    }

    private static async Task<string> ComputeBodyHashAsync(HttpContext context)
    {
        if (context.Request.ContentLength is null or 0)
        {
            return "no-body";
        }

        context.Request.EnableBuffering();
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(context.Request.Body);
        context.Request.Body.Position = 0;
        return Convert.ToHexString(hash);
    }

    private static void PurgeExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var pair in Cache)
        {
            if (pair.Value.ExpiresAt < now)
            {
                Cache.TryRemove(pair.Key, out _);
            }
        }
    }

    private sealed record CachedResponse(int StatusCode, string ContentType, byte[] Body, DateTimeOffset ExpiresAt);
}
