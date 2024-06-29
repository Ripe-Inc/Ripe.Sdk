using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var sdk = new RipeSdk<MockRipeConfig>((httpClient, options) =>
            {
                httpClient.AssignHttpClientMessageHandler(new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig()));
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            });
            var config = sdk.Hydrate();
            Assert.That(sdk.Expiry, Is.GreaterThan(DateTime.Now.AddSeconds(290)));
            Assert.That(sdk.Expiry, Is.LessThan(DateTime.Now.AddSeconds(310)));
        }
        [Test]
        public async Task VerifyExpiryAsync()
        {
            var sdk = new RipeSdk<MockRipeConfig>((httpClient, options) =>
            {
                httpClient.AssignHttpClientMessageHandler(new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig()));
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            });
            var config = await sdk.HydrateAsync();
            Assert.That(sdk.Expiry, Is.GreaterThan(DateTime.Now.AddSeconds(290)));
            Assert.That(sdk.Expiry, Is.LessThan(DateTime.Now.AddSeconds(310)));
        }
        [Test]
        public async Task VerifyRequestSchemaAsync()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig());
            var sdk = new RipeSdk<MockRipeConfig>((httpClient, options) =>
            {
                httpClient.AssignHttpClientMessageHandler(requestHandler);
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            });
            var config = await sdk.HydrateAsync();

            Assert.That(requestHandler.Request?.Content, Is.Not.Null);
            var requestContent = await requestHandler.Request.Content.ReadAsStringAsync();
            var requestObj = JsonSerializer.Deserialize<JsonObject>(requestContent);
            Assert.That(requestObj, Is.Not.Null);
            Assert.That(requestObj["Version"]?.ToString(), Is.EqualTo("1.0.0.0"));
            var schema = JsonSerializer.Deserialize<string[]>(requestObj["Schema"]);
            Assert.That(schema, Is.Not.Null);
            Assert.That(schema, Has.Length.EqualTo(8));
            Assert.That(schema, Does.Contain("TimeToLive"));
            Assert.That(schema, Does.Contain("ApiVersion"));
            Assert.That(schema, Does.Contain("Child.Value1"));
            Assert.That(schema, Does.Contain("Child.Value2"));
            Assert.That(schema, Does.Contain("Child1.Value1"));
            Assert.That(schema, Does.Contain("Child1.Value2"));
            Assert.That(schema, Does.Contain("Child.Child2.Value1"));
            Assert.That(schema, Does.Contain("Child.Child2.Value2"));
        }
        [Test]
        public void VerifyRequestSchema()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig());
            var sdk = new RipeSdk<MockRipeConfig>((httpClient, options) =>
            {
                httpClient.AssignHttpClientMessageHandler(requestHandler);
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            });
            var config = sdk.Hydrate();

            Assert.That(requestHandler.Request?.Content, Is.Not.Null);
            var requestContent = requestHandler.Request.Content.ReadAsStringAsync().Result;
            var requestObj = JsonSerializer.Deserialize<JsonObject>(requestContent);
            Assert.That(requestObj, Is.Not.Null);
            Assert.That(requestObj["Version"]?.ToString(), Is.EqualTo("1.0.0.0"));
            var schema = JsonSerializer.Deserialize<string[]>(requestObj["Schema"]);
            Assert.That(schema, Is.Not.Null);
            Assert.That(schema, Has.Length.EqualTo(8));
            Assert.That(schema, Does.Contain("TimeToLive"));
            Assert.That(schema, Does.Contain("ApiVersion"));
            Assert.That(schema, Does.Contain("Child.Value1"));
            Assert.That(schema, Does.Contain("Child.Value2"));
            Assert.That(schema, Does.Contain("Child1.Value1"));
            Assert.That(schema, Does.Contain("Child1.Value2"));
            Assert.That(schema, Does.Contain("Child.Child2.Value1"));
            Assert.That(schema, Does.Contain("Child.Child2.Value2"));
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
                    Value1 = "Hello",
                    Value2 = "World",
                    Child2 = new()
                    {
                        Value1 = "I",
                        Value2 = "Am"
                    }
                },
                Child1 = new()
                {
                    Value1 = "Ripe",
                    Value2 = "Config"
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
            bool tryChildValue1 = provider.TryGet("Child:Value1", out string? childValue1);
            Assert.Multiple(() =>
            {
                Assert.That(tryChildValue1, Is.True);
                Assert.That(childValue1, Is.EqualTo("Hello"));
            });
            bool tryChildValue2 = provider.TryGet("Child:Value2", out string? childValue2);
            Assert.Multiple(() =>
            {
                Assert.That(tryChildValue2, Is.True);
                Assert.That(childValue2, Is.EqualTo("World"));
            });
            bool tryChild1Value1 = provider.TryGet("Child1:Value1", out string? child1Value1);
            Assert.Multiple(() =>
            {
                Assert.That(tryChild1Value1, Is.True);
                Assert.That(child1Value1, Is.EqualTo("Ripe"));
            });
            bool tryChild1Value2 = provider.TryGet("Child1:Value2", out string? child1Value2);
            Assert.Multiple(() =>
            {
                Assert.That(tryChild1Value2, Is.True);
                Assert.That(child1Value2, Is.EqualTo("Config"));
            });
            bool tryChildChild2Value1 = provider.TryGet("Child:Child2:Value1", out string? childChild2Value1);
            Assert.Multiple(() =>
            {
                Assert.That(tryChildChild2Value1, Is.True);
                Assert.That(childChild2Value1, Is.EqualTo("I"));
            });
            bool tryChildChild2Value2 = provider.TryGet("Child:Child2:Value2", out string? childChild2Value2);
            Assert.Multiple(() =>
            {
                Assert.That(tryChildChild2Value2, Is.True);
                Assert.That(childChild2Value2, Is.EqualTo("Am"));
            });
        }

        [Test]
        public void TestExtensionSetup()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig() { TimeToLive = 0 });
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddRipe<MockRipeConfig>(services, (httpClient, options) =>
                {
                    httpClient.AssignHttpClientMessageHandler(requestHandler);
                    options.Uri = "https://test.com";
                    options.ApiKey = "rpri_testkey";
                    options.Version = "1.0.0.0";
                })
                .Build();
            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<IRipeSdk<MockRipeConfig>>();
            configService.Hydrate();
            configService.Hydrate();
            configService.Hydrate();
            Assert.That(requestHandler.Count, Is.EqualTo(5));
        }
        [Test]
        public async Task TestExtensionSetupAsync()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig() { TimeToLive = 0 });
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddRipe<MockRipeConfig>(services, (httpClient, options) =>
                {
                    httpClient.AssignHttpClientMessageHandler(requestHandler);
                    options.Uri = "https://test.com";
                    options.ApiKey = "rpri_testkey";
                    options.Version = "1.0.0.0";
                })
                .Build();
            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<IRipeSdk<MockRipeConfig>>();
            await configService.HydrateAsync();
            await configService.HydrateAsync();
            await configService.HydrateAsync();
            Assert.That(requestHandler.Count, Is.EqualTo(5));
        }
        [Test]
        public async Task TestExtensionSetupAsync_NewDataIncoming()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig() { TimeToLive = 0 });
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddRipe<MockRipeConfig>(services, (httpClient, options) =>
                {
                    httpClient.AssignHttpClientMessageHandler(requestHandler);
                    options.Uri = "https://test.com";
                    options.ApiKey = "rpri_testkey";
                    options.Version = "1.0.0.0";
                })
                .Build();

            var provider = services.BuildServiceProvider();
            var configService = provider.GetRequiredService<IRipeSdk<MockRipeConfig>>();
            var configObj = await configService.HydrateAsync();
            Assert.That(configObj.TimeToLive, Is.EqualTo(0));
            
            // Assert that changes are reflected in the hydrated obj immediately
            requestHandler.ResponseContent = new MockRipeConfig() { TimeToLive = 300 };
            configObj = await configService.HydrateAsync();
            Assert.Multiple(() =>
            {
                Assert.That(configObj.TimeToLive, Is.EqualTo(300));
                Assert.That(configService.Expiry, Is.GreaterThan(DateTime.Now));
            });
        }
        [Test]
        public async Task TestPropertyNameAttr_Outgoing()
        {
            var requestHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new MockRipeConfig2());
            var sdk = new RipeSdk<MockRipeConfig2>((httpClient, options) =>
            {
                httpClient.AssignHttpClientMessageHandler(requestHandler);
                options.Uri = "https://test.com";
                options.ApiKey = "rpri_testkey";
                options.Version = "1.0.0.0";
            });
            var config = await sdk.HydrateAsync();

            Assert.That(requestHandler.Request?.Content, Is.Not.Null);
            var requestContent = await requestHandler.Request.Content.ReadAsStringAsync();
            var requestObj = JsonSerializer.Deserialize<JsonObject>(requestContent);
            Assert.That(requestObj, Is.Not.Null);
            Assert.That(requestObj["Version"]?.ToString(), Is.EqualTo("1.0.0.0"));
            var schema = JsonSerializer.Deserialize<string[]>(requestObj["Schema"]);
            Assert.That(schema, Is.Not.Null);
            Assert.That(schema, Has.Length.EqualTo(7));
            Assert.That(schema, Does.Contain("TimeToLive"));
            Assert.That(schema, Does.Contain("ApiVersion"));
            Assert.That(schema, Does.Contain("Test"));
            Assert.That(schema, Does.Contain("Child.TestAttribute"));
            Assert.That(schema, Does.Contain("Child.Dict"));
            Assert.That(schema, Does.Contain("TestChild.TestAttribute"));
            Assert.That(schema, Does.Contain("TestChild.Dict"));
        }
    }

    public class MockRipeConfig : IRipeConfiguration
    {
        public int TimeToLive { get; set; } = 300;
        public string ApiVersion { get; set; } = "";
        public MockRipeConfigChild Child { get; set; } = new();
        public MockRipeConfigChild1 Child1 { get; set; } = new();
    }
    public class MockRipeConfigChild
    {
        public string Value1 { get; set; } = "";
        public string Value2 { get; set; } = "";
        public MockRipeConfigChild2 Child2 { get; set; } = new();
    }
    public class MockRipeConfigChild1
    {
        public string Value1 { get; set; } = "";
        public string Value2 { get; set; } = "";
    }
    public class MockRipeConfigChild2
    {
        public string Value1 { get; set; } = "";
        public string Value2 { get; set; } = "";
    }
    public class MockRipeConfig2 : IRipeConfiguration
    {
        public int TimeToLive { get; set; } = 300;
        public string ApiVersion { get; set; } = "";
        [RipePropertyName("Test")]
        public string TestAttribute { get; set; } = "";
        public MockRipeConfig2Child Child { get; set; } = new();
        [RipePropertyName("TestChild")]
        public MockRipeConfig2Child Child2 { get; set; } = new();
    }
    public class MockRipeConfig2Child
    {
        [RipePropertyName("TestAttribute")]
        public string Test { get; set; } = "";
        public Dictionary<string, object> Dict { get; set; } = [];
    }
}