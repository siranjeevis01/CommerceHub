namespace CommerceHub.API.Configuration;

public class AzureKeyVaultOptions
{
    public string VaultUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}