namespace InsuranceApp.Application.Interfaces;

public sealed record ObjectStorageUploadRequest(string Key, byte[] Content, string ContentType, IDictionary<string, string>? Metadata = null);
public sealed record ObjectStorageDownloadResult(string Key, byte[] Content, string ContentType);

public interface IObjectStorage
{
    string Provider { get; }
    Task<string> UploadAsync(ObjectStorageUploadRequest request, CancellationToken cancellationToken = default);
    Task<ObjectStorageDownloadResult?> DownloadAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
}
