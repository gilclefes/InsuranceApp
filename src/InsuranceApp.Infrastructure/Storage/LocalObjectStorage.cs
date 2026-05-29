using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Storage;

public sealed class LocalObjectStorage(IOptions<ObjectStorageOptions> options, ILogger<LocalObjectStorage> logger) : IObjectStorage
{
    public string Provider => "LOCAL";

    private string Root => options.Value.Local.RootPath;

    public async Task<string> UploadAsync(ObjectStorageUploadRequest request, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(request.Key);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, request.Content, cancellationToken);
        var metaPath = fullPath + ".meta";
        await File.WriteAllTextAsync(metaPath, request.ContentType, cancellationToken);
        logger.LogInformation("Stored object {Key} ({Bytes} bytes) at {Path}", request.Key, request.Content.Length, fullPath);
        return request.Key;
    }

    public async Task<ObjectStorageDownloadResult?> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(key);
        if (!File.Exists(fullPath)) return null;
        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        var metaPath = fullPath + ".meta";
        var contentType = File.Exists(metaPath) ? await File.ReadAllTextAsync(metaPath, cancellationToken) : "application/octet-stream";
        return new ObjectStorageDownloadResult(key, bytes, contentType);
    }

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(key);
        if (!File.Exists(fullPath)) return Task.FromResult(false);
        File.Delete(fullPath);
        var metaPath = fullPath + ".meta";
        if (File.Exists(metaPath)) File.Delete(metaPath);
        return Task.FromResult(true);
    }

    private string ResolvePath(string key)
    {
        var safe = key.Replace("..", string.Empty).Trim('/');
        return Path.Combine(Root, safe);
    }
}
