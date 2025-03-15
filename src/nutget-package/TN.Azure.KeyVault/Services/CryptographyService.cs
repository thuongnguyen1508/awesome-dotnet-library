using Azure.Security.KeyVault.Secrets;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using TN.Azure.KeyVault.Services.Abstractions;

namespace TN.Azure.KeyVault.Services
{
    public class CryptographyService : ICryptographyService
    {
        private readonly IKeyVaultService _keyVaultService;
        private Dictionary<string, KeyVaultSecret> _cache = new Dictionary<string, KeyVaultSecret>();

        public CryptographyService(IKeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
        }

        public async Task<string> DecryptData(string keyVaultName, string data)
        {
            KeyVaultSecret secret = null!;

            if (keyVaultName != null)
            {
                if (_cache.ContainsKey(keyVaultName))
                {
                    secret = _cache[keyVaultName];
                }
                else
                {
                    secret = await _keyVaultService.GetSecretAsync(keyVaultName);
                    _cache.TryAdd(keyVaultName, secret);
                }
            }

            if (secret != null)
            {
                var rsa = RSA.Create();
                var privateKey = Regex.Unescape(secret.Value);
                rsa.ImportFromPem(privateKey.ToCharArray());
                var dataDecrypt = rsa.Decrypt(Convert.FromBase64String(data), RSAEncryptionPadding.OaepSHA256);
                var decodedText = Encoding.UTF8.GetString(dataDecrypt);
                return decodedText;
            }
            else
            {
                throw new Exception($"KeyVault secret {keyVaultName} not found");
            }
        }

        public async Task<string> EncryptData(string keyVaultName, string data)
        {
            KeyVaultSecret secret = null!;

            if (keyVaultName != null)
            {
                if (_cache.ContainsKey(keyVaultName))
                {
                    secret = _cache[keyVaultName];
                }
                else
                {
                    secret = await _keyVaultService.GetSecretAsync(keyVaultName);
                    _cache.TryAdd(keyVaultName, secret);
                }
            }

            if (secret != null)
            {
                var rsa = RSA.Create();
                var privateKey = Regex.Unescape(secret.Value);
                rsa.ImportFromPem(privateKey.ToCharArray());
                var dataEncrypt = rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.OaepSHA256);
                var encodedText = Convert.ToBase64String(dataEncrypt);
                return encodedText;
            }
            else
            {
                throw new Exception($"KeyVault secret {keyVaultName} not found");
            }
        }
    }
}
