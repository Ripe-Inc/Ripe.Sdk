using System.Net.Http.Json;
using System.Net;

namespace Ripe.Sdk.Tests
{
    internal class MockHttpMessageHandler(HttpStatusCode statusCode, object? responseContent = null)
        : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode = statusCode;
        public object? ResponseContent { get; set; } = responseContent;
        public int Count { get; private set; }
        public HttpRequestMessage? Request { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Count++;
            return await Task.FromResult(new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = JsonContent.Create(new { Data = ResponseContent })
            });
        }
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsync(request, cancellationToken).Result;
        }
    }
}
