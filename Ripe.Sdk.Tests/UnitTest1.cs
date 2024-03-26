using NSubstitute;
using Ripe.Sdk.Core;
using Ripe.Sdk.DependencyInjection;
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
        [Test]
        public void TestConfigurationProvider()
        {
            var mockSdk = Substitute.For<IRipeSdk<MockRipeConfig>>();
            mockSdk.Hydrate().Returns(new MockRipeConfig()
            {
                ApiVersion = "123",
                Child = new()
                {
                    Child1 = "Hello",
                    Child2 = "World"
                }
            });

            var provider = new RipeConfigurationProvider<MockRipeConfig>(mockSdk);
            provider.Load();
            bool tryTtl = provider.TryGet("TimeToLive", out string? ttl);
            Assert.Multiple(() =>
            {
                Assert.That(tryTtl, Is.True);
                Assert.That(ttl, Is.EqualTo("300"));
            });
            bool tryApiVersion = provider.TryGet("ApiVersion", out string? apiVersion);
            Assert.Multiple(() =>
            {
                Assert.That(tryApiVersion, Is.True);
                Assert.That(apiVersion, Is.EqualTo("123"));
            });
            bool tryChild1 = provider.TryGet("Child:Child1", out string? child1);
            Assert.Multiple(() =>
            {
                Assert.That(tryChild1, Is.True);
                Assert.That(child1, Is.EqualTo("Hello"));
            });
            bool tryChild2 = provider.TryGet("Child:Child2", out string? child2);
            Assert.Multiple(() =>
            {
                Assert.That(tryChild2, Is.True);
                Assert.That(child2, Is.EqualTo("World"));
            });
        }
    }

    public class MockRipeConfig : IRipeConfiguration
    {
        public int TimeToLive { get; set; } = 300;
        public string ApiVersion { get; set; } = "";
        public MockRipeConfigChild Child { get; set; } = new();
    }
    public class MockRipeConfigChild
    {
        public string Child1 { get; set; } = "";
        public string Child2 { get; set; } = "";
    }
}