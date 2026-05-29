using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Storage;

public sealed class S3ObjectStorage(IOptions<ObjectStorageOptions> options, ILogger<S3ObjectStorage> logger) : IObjectStorage
{
    public string Provider => "S3";

    private IAmazonS3 CreateClient()
    {
        var cfg = options.Value.S3;
        var s3Config = new AmazonS3Config
        {
            ForcePathStyle = cfg.ForcePathStyle,
        };
        if (!string.IsNullOrWhiteSpace(cfg.ServiceUrl))
        {
            s3Config.ServiceURL = cfg.ServiceUrl;
        }
        else
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(cfg.Region);
        }

        var credentials = string.IsNullOrWhiteSpace(cfg.AccessKey)
            ? (AWSCredentials)new AnonymousAWSCredentials()
            : new BasicAWSCredentials(cfg.AccessKey, cfg.SecretKey);

        return new AmazonS3Client(credentials, s3Config);
    }

    public async Task<string> UploadAsync(ObjectStorageUploadRequest request, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var stream = new MemoryStream(request.Content);
        var put = new PutObjectRequest
        {
            BucketName = options.Value.S3.BucketName,
            Key = request.Key,
            InputStream = stream,
            ContentType = request.ContentType
        };
        if (request.Metadata is not null)
        {
            foreach (var kv in request.Metadata)
            {
                put.Metadata[kv.Key] = kv.Value;
            }
        }
        await client.PutObjectAsync(put, cancellationToken);
        logger.LogInformation("Uploaded {Key} to S3 bucket {Bucket}", request.Key, options.Value.S3.BucketName);
        return request.Key;
    }

    public async Task<ObjectStorageDownloadResult?> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            var get = await client.GetObjectAsync(options.Value.S3.BucketName, key, cancellationToken);
            using var ms = new MemoryStream();
            await get.ResponseStream.CopyToAsync(ms, cancellationToken);
            return new ObjectStorageDownloadResult(key, ms.ToArray(), get.Headers.ContentType ?? "application/octet-stream");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        await client.DeleteObjectAsync(options.Value.S3.BucketName, key, cancellationToken);
        return true;
    }
}
