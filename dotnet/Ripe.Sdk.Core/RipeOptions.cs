namespace Ripe.Sdk.Core
{
    /// <summary>
    /// Options container to facilitate options builders
    /// </summary>
    public interface IRipeOptions
    {
        /// <summary>
        /// The Ripe API key. This can be found in your Ripe Environment settings
        /// </summary>
        string ApiKey { get; set; }
        /// <summary>
        /// The Ripe URI endpoint. This can be found in your Ripe Environment settings
        /// </summary>
        string Uri { get; set; }
        /// <summary>
        /// [Optional] The version of your application.
        /// </summary>
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
