using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using System.Web;
using TN.Azure.Storage.Models;
using TN.Azure.Storage.Services.Abtractions;

namespace TN.Azure.Storage.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureStorageOptions _options;
        public AzureStorageService(IOptionsMonitor<AzureStorageOptions> storageOptions)
        {
            _options = storageOptions.CurrentValue;
            if (_options.UseAzureIdentity == true)
            {
                _blobServiceClient = new BlobServiceClient(new Uri(_options.EndPoint), new DefaultAzureCredential());
            }
            else
            {
                _blobServiceClient = new BlobServiceClient($"DefaultEndpointsProtocol=https;AccountName={_options.AccountName};AccountKey={_options.AccountKey};EndpointSuffix=core.windows.net");
            }
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = GetBlobContainerByName(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
        {
            var containerClient = GetBlobContainerByName(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
        }

        public async Task UploadBlobAsync(string containerName, string blobName, Stream content)
        {
            var containerClient = GetBlobContainerByName(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, overwrite: true);
        }

        public async Task<SASToken> GenToken(string containerName)
        {
            if (_options.UseAzureIdentity == true)
            {
                return await GenSasToken(containerName);
            }
            else
            {
                return GenAccountToken(containerName);
            }
        }

        private BlobContainerClient GetBlobContainerByName(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }

        private SASToken GenAccountToken(string containerName)
        {
            string accountKey = _options.AccountKey;
            string accountName = _options.AccountName;

            var storageSharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);

            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.Blobs,
                ResourceTypes = AccountSasResourceTypes.Object,
                StartsOn = DateTime.UtcNow.AddMinutes(_options.TimeStart),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiredTime),
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(AccountSasPermissions.Add | AccountSasPermissions.Write | AccountSasPermissions.Create | AccountSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();
            string sasTokenDecode = HttpUtility.UrlDecode(sasToken);
            return new SASToken()
            {
                SasToken = sasTokenDecode,
                BlobEndpoint = $"{_options.EndPoint}/{containerName}",
                CdnFormatEndpoint = $"{_options.EndPoint}/{containerName}"
            };
        }


        private async Task<SASToken> GenSasToken(string containerName)
        {
            // Create a SAS token that's valid for ExpireMinutes.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                Resource = "c",
                StartsOn = DateTime.UtcNow.AddMinutes(_options.TimeStart),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiredTime),
            };

            var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.AddMinutes(_options.TimeStart),
                                                                    DateTimeOffset.UtcNow.AddMinutes(_options.ExpiredTime));

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Create | BlobAccountSasPermissions.Add | BlobAccountSasPermissions.Write | BlobAccountSasPermissions.Read);

            return new SASToken()
            {
                SasToken = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName).ToString(),
                BlobEndpoint = $"{_options.EndPoint}/{containerName}",
                CdnFormatEndpoint = $"{_options.EndPoint}/{containerName}"
            };
        }
    }
}
