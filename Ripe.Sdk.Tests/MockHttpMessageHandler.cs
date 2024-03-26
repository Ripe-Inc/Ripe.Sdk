using System.Net.Http.Json;
using System.Net;

namespace Ripe.Sdk.Tests
{
    internal class MockHttpMessageHandler(HttpStatusCode statusCode, object? responseContent = null)
        : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode = statusCode;
        private readonly object? _responseContent = responseContent;
        public HttpRequestMessage? Request { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return await Task.FromResult(new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = JsonContent.Create(new { Data = _responseContent })
            });
        }
    }
}
