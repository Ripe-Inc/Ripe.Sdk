namespace Ripe.Sdk.Core
{
    public interface IRipeConfiguration
    {
        int TimeToLive { get; set; }
        string ApiVersion { get; set; }
    }
}
