# Ripe.Sdk.Core

[![NuGet Build](https://github.com/Ripe-Inc/ripe-sdks/actions/workflows/nuget-publish-core.yml/badge.svg)](https://github.com/Ripe-Inc/ripe-sdks/actions/workflows/nuget-publish-core.yml)
[![Unit Tests](https://github.com/Ripe-Inc/ripe-sdks/actions/workflows/unit-tests.yml/badge.svg)](https://github.com/Ripe-Inc/ripe-sdks/actions/workflows/unit-tests.yml)

The Ripe Core package includes the bare minimum to get started with Ripe, used mostly for Console and Desktop applications. If you're looking for 
a package that supports modern dependency injection via the `IConfigurationBuilder`, then navigate [here](https://github.com/matt-andrews/Ripe.Sdk/tree/main/Ripe.Sdk.DependencyInjection)

## Getting Started
Install Ripe.Sdk
```
dotnet add package Ripe.Sdk.Core
```

Create a class that will hold your configuration, inheriting from `IRipeConfiguration`:
```csharp
public class Config : IRipeConfiguration
{
	public string TestConfig { get; set; }
	public string ApiVersion { get; set; }
}
```

You can also include nested objects if your Ripe configuration has nested configurations. For instance the Ripe key `TestObj.Value` can be represented like so:
```csharp
public class Config : IRipeConfiguration
{
	public TestObjConfig TestObj { get; set; }
	public string TestConfig { get; set; }
	public int TimeToLive { get; set; }
	public string ApiVersion { get; set; }
}
public class TestObjConfig
{
	public string Value { get; set; }
}
```

Once you have your config class setup, you can initialize it like so:
```csharp
RipeSdk<Config> sdk = new RipeConfig<Config>(options => 
{
	options.Uri = "https://config.ripe.sh/<your-environment-details>";
	options.ApiKey = "rpri_your-ripe-api-key";
	options.Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? ""
});

Config config = sdk.Hydrate(); /* there is also an async api available with HydrateAsync() */
```

From here you now have the `config` object that represents your configuration. You can call `sdk.Hydrate()` at regular intervals to refresh the configuration with the latest data.