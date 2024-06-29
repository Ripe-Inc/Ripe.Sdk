namespace Ripe.Sdk.Tests
{
    internal static class TestExtensions
    {
        /// <summary>
        /// Use reflection to set the <see cref="HttpMessageHandler"/> of the <see cref="HttpClient"/>
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="handler"></param>
        /// <exception cref="Exception"></exception>
        public static void AssignHttpClientMessageHandler(this HttpClient httpClient, HttpMessageHandler handler)
        {
            var prop = typeof(HttpMessageInvoker).GetField("_handler", System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance) 
                ?? throw new Exception();
            prop.SetValue(httpClient, handler);
        }
    }
}
