using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using TN.Azure.KeyVault.Services.Abstractions;

namespace TN.Azure.KeyVault.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;
        public KeyVaultService(IOptionsMonitor<KeyVaultOptions> options)
        {
            _secretClient = new SecretClient(options.CurrentValue.EndPoint, new DefaultAzureCredential());
        }
        public async Task DeleteSecretAsync(string secretName)
        {
            await _secretClient.StartDeleteSecretAsync(secretName);
        }

        public async Task<KeyVaultSecret> GetSecretAsync(string secretName)
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            await _secretClient.SetSecretAsync(secretName, secretValue);
        }
    }
}
