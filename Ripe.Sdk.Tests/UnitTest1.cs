using Ripe.Sdk.Core;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ripe.Sdk.Tests
{
    public class Tests
    {
        [Test]
        public void VerifyExpiry()
        {
            var sdk = new RipeSdk<MockRipeConfig>(options =>
            {
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            }, new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig())));
            var config = sdk.Hydrate();
            Assert.That(sdk.Expiry, Is.GreaterThan(DateTime.Now.AddSeconds(290)));
            Assert.That(sdk.Expiry, Is.LessThan(DateTime.Now.AddSeconds(310)));
        }
        [Test]
        public async Task VerifyExpiryAsync()
        {
            var sdk = new RipeSdk<MockRipeConfig>(options =>
            {
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            }, new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig())));
            var config = await sdk.HydrateAsync();
            Assert.That(sdk.Expiry, Is.GreaterThan(DateTime.Now.AddSeconds(290)));
            Assert.That(sdk.Expiry, Is.LessThan(DateTime.Now.AddSeconds(310)));
        }
        [Test]
        public async Task VerifyRequestSchemaAsync()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig());
            var sdk = new RipeSdk<MockRipeConfig>(options =>
            {
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            }, new HttpClient(requestHandler));
            var config = await sdk.HydrateAsync();

            Assert.That(requestHandler.Request?.Content, Is.Not.Null);
            var requestContent = await requestHandler.Request.Content.ReadAsStringAsync();
            var requestObj = JsonSerializer.Deserialize<JsonObject>(requestContent);
            Assert.That(requestObj, Is.Not.Null);
            Assert.That(requestObj["Version"]?.ToString(), Is.EqualTo("1.0.0.0"));
            var schema = JsonSerializer.Deserialize<string[]>(requestObj["Schema"]);
            Assert.That(schema, Is.Not.Null);
            Assert.That(schema, Has.Length.EqualTo(4));
            Assert.That(schema, Does.Contain("TimeToLive"));
            Assert.That(schema, Does.Contain("ApiVersion"));
            Assert.That(schema, Does.Contain("Child.Child1"));
            Assert.That(schema, Does.Contain("Child.Child2"));
        }
        [Test]
        public void VerifyRequestSchema()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig());
            var sdk = new RipeSdk<MockRipeConfig>(options =>
            {
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            }, new HttpClient(requestHandler));
            var config = sdk.Hydrate();

            Assert.That(requestHandler.Request?.Content, Is.Not.Null);
            var requestContent = requestHandler.Request.Content.ReadAsStringAsync().Result;
            var requestObj = JsonSerializer.Deserialize<JsonObject>(requestContent);
            Assert.That(requestObj, Is.Not.Null);
            Assert.That(requestObj["Version"]?.ToString(), Is.EqualTo("1.0.0.0"));
            var schema = JsonSerializer.Deserialize<string[]>(requestObj["Schema"]);
            Assert.That(schema, Is.Not.Null);
            Assert.That(schema, Has.Length.EqualTo(4));
            Assert.That(schema, Does.Contain("TimeToLive"));
            Assert.That(schema, Does.Contain("ApiVersion"));
            Assert.That(schema, Does.Contain("Child.Child1"));
            Assert.That(schema, Does.Contain("Child.Child2"));
        }
    }

    internal class MockRipeConfig : IRipeConfiguration
    {
        public int TimeToLive { get; set; } = 300;
        public string ApiVersion { get; set; } = "";
        public MockRipeConfigChild Child { get; set; } = new();
    }
    internal class MockRipeConfigChild
    {
        public string Child1 { get; set; } = "";
        public string Child2 { get; set; } = "";
    }
}