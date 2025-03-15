namespace TN.Azure.Storage
{
    public class AzureStorageOptions
    {
        public string EndPoint { get; set; }
        public bool UseAzureIdentity { get; set; } = true;
        public string AccountKey { get; set; }
        public string AccountName { get; set; }
        public int TimeStart { get; set; }
        public int ExpiredTime { get; set; }
        public string CdnEndpoint { get; set; }
    }
}
