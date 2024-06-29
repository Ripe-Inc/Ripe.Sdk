# Ripe.Sdk.DependencyInjection

[![NuGet Build](https://github.com/matt-andrews/Ripe.Sdk/actions/workflows/nuget-publish-dependencyinjection.yml/badge.svg?branch=main)](https://github.com/matt-andrews/Ripe.Sdk/actions/workflows/nuget-publish-dependencyinjection.yml)
[![Unit Tests](https://github.com/matt-andrews/Ripe.Sdk/actions/workflows/unit-tests.yml/badge.svg)](https://github.com/matt-andrews/Ripe.Sdk/actions/workflows/unit-tests.yml)

The Ripe Dependency Injection package includes extensions for integrating with the `IConfigurationBuilder` during setup to pull your configuration 
into `IConfiguration`, as well as inject your config as a scoped service which is automatically refreshed in regular intervals.

## Getting Started
Follow the guide [here](https://github.com/matt-andrews/Ripe.Sdk/tree/main/Ripe.Sdk.Core) to learn how to create an `IRipeConfiguration` object to base your configuration on. 

Install Ripe.Sdk.DependencyInjection
```
dotnet add package Ripe.Sdk.DependencyInjection
```

Once you have your configuration object created, you can inject your configuration like so:
```csharp
var configuration = new ConfigurationBuilder()
	.AddRipe<Config>(builder.Services, options =>
	{
		options.Uri = "https://config.ripe.sh/<your-environment-details>";
		options.ApiKey = "rpri_your-ripe-api-key";
		options.Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? ""
	})
	.Build();
builder.Configuration.AddConfiguration(configuration);
```

With the above code, you can access your `Config` object normally through dependency injection, and the data refreshed at periodic intervals.

There are several overrides for `AddRipe` which help you through different scenarios.
```csharp
//This extension is used for pulling the Ripe configuration and implanting the key/value pairs into `IConfiguration` only
public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, Action<IRipeOptions> optionsBuilder)
```

```csharp
//This extension is used for implanting the key/value pairs into `IConfiguration` as well as inject the object as a Scoped service, and has the `out` 
//parameter so that you can access the config object immediately in your setup code
public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, IServiceCollection services, Action<IRipeOptions> optionsBuilder, out TConfig bindingObj)
```