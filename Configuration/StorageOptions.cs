namespace CommerceHub.API.Configuration;

public class StorageOptions
{
    public string Provider { get; set; } = "local";
    public AwsOptions Aws { get; set; } = new();
    public AzureOptions Azure { get; set; } = new();
}

public class AwsOptions
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string CloudFrontUrl { get; set; } = string.Empty;
}

public class AzureOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "commercehub-assets";
}