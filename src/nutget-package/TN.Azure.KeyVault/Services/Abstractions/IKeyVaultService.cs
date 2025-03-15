using Azure.Security.KeyVault.Secrets;

namespace TN.Azure.KeyVault.Services.Abstractions
{
    public interface IKeyVaultService
    {
        Task<KeyVaultSecret> GetSecretAsync(string secretName);
        Task SetSecretAsync(string secretName, string secretValue);
        Task DeleteSecretAsync(string secretName);
    }
}
