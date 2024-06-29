using System;

namespace Ripe.Sdk.Core
{
    /// <summary>
    /// Interface to implement on your configuration objects
    /// </summary>
    public interface IRipeConfiguration
    {
        /// <summary>
        /// The version of the Ripe API you have connected to
        /// </summary>
        string ApiVersion { get; set; }
    }
}
