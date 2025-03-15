using TN.Azure.Storage.Models;

namespace TN.Azure.Storage.Services.Abtractions
{
    public interface IAzureStorageService
    {
        Task UploadBlobAsync(string containerName, string blobName, Stream content);
        Task<Stream> DownloadBlobAsync(string containerName, string blobName);
        Task DeleteBlobAsync(string containerName, string blobName);
        Task<SASToken> GenToken(string containerName);
    }
}
