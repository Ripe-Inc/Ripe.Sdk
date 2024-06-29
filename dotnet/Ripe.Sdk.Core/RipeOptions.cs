namespace Ripe.Sdk.Core
{
    public interface IRipeOptions
    {
        string ApiKey { get; set; }
        string Uri { get; set; }
        string Version { get; set; }
    }
    internal class RipeOptions : IRipeOptions
    {
        public string ApiKey { get; set; } = "";
        public string Uri { get; set; } = "";
        public string Version { get; set; } = "";
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new System.NullReferenceException($"{nameof(ApiKey)} is required");
            }

            if (string.IsNullOrWhiteSpace(Uri))
            {
                throw new System.NullReferenceException($"{nameof(Uri)} is required");
            }

            return true;
        }
    }
}
