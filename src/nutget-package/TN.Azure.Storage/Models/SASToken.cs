namespace TN.Azure.Storage.Models
{
    public class SASToken
    {
        public string SasToken { get; set; }
        public string CdnFormatEndpoint { get; set; }
        public string BlobEndpoint { get; set; }
    }
}
