namespace Ripe.Sdk.Core
{
    internal class HydrationRequest
    {
        public string Version { get; set; } = "";
        public string[] Schema { get; set; }
    }

    internal class HydrationResponse<TConfig>
    {
        public TConfig Data { get; set; }
    }
}
