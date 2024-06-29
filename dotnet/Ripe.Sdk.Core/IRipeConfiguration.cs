namespace Ripe.Sdk.Core
{
    /// <summary>
    /// Interface to implement on your configuration objects
    /// </summary>
    public interface IRipeConfiguration
    {
        /// <summary>
        /// The interval at which the Ripe Service will refresh the cache with changes to your Ripe configuration. Used for configuring 
        /// when to automatically attempt to refresh configuration data
        /// </summary>
        int TimeToLive { get; set; }
        /// <summary>
        /// The version of the Ripe API you have connected to
        /// </summary>
        string ApiVersion { get; set; }
    }
}
