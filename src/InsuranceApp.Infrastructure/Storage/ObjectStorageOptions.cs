namespace InsuranceApp.Infrastructure.Storage;

public sealed class ObjectStorageOptions
{
    public string Provider { get; set; } = "LOCAL";
    public LocalObjectStorageOptions Local { get; set; } = new();
    public S3ObjectStorageOptions S3 { get; set; } = new();
}

public sealed class LocalObjectStorageOptions
{
    public string RootPath { get; set; } = "App_Data/objects";
}

public sealed class S3ObjectStorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty; // MinIO endpoint
    public bool ForcePathStyle { get; set; } = true;
}
