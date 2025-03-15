namespace TN.Azure.KeyVault.Services.Abstractions
{
    public interface ICryptographyService
    {
        Task<string> EncryptData(string keyVaultName, string data);
        Task<string> DecryptData(string keyVaultName, string data);
    }
}
