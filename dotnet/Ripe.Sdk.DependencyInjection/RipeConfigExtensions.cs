using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripe.Sdk.Core;
using System;
using System.Net.Http;

namespace Ripe.Sdk.DependencyInjection
{
    /// <summary>
    /// A collection of static helper methods for initializing Ripe Dependency Injection
    /// </summary>
    public static class RipeConfigExtensions
    {
        /// <summary>
        /// Add the specified Ripe configuration to your <see cref="IConfigurationBuilder"/>
        /// </summary>
        /// <typeparam name="TConfig">The <see cref="IRipeConfiguration"/> that defines your Ripe configuration</typeparam>
        /// <param name="builder"></param>
        /// <param name="optionsBuilder">The options you use to connect to Ripe</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, Action<HttpClient, IRipeOptions> optionsBuilder)
            where TConfig : class, IRipeConfiguration
        {
            IRipeSdk<TConfig> sdk = new RipeSdk<TConfig>(optionsBuilder);
            builder.Add(new RipeConfigurationSource<TConfig>(sdk));
            return builder;
        }

        /// <summary>
        /// Add the specified Ripe configuration to your <see cref="IConfigurationBuilder"/> and inject the built configuration as a Scoped Service. 
        /// The configuration will automatically refresh values based on Ripe environment caching details
        /// </summary>
        /// <typeparam name="TConfig">The <see cref="IRipeConfiguration"/> that defines your Ripe configuration</typeparam>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="optionsBuilder">The options you use to connect to Ripe</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, IServiceCollection services, Action<HttpClient, IRipeOptions> optionsBuilder)
            where TConfig : class, IRipeConfiguration
        {
            return builder.AddRipe<TConfig>(services, optionsBuilder, out _);
        }

        /// <summary>
        /// Add the specified Ripe configuration to your <see cref="IConfigurationBuilder"/> and inject the built configuration as a Scoped Service. 
        /// The configuration will automatically refresh values based on Ripe environment caching details.
        /// </summary>
        /// <typeparam name="TConfig">The <see cref="IRipeConfiguration"/> that defines your Ripe configuration</typeparam>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <param name="optionsBuilder">The options you use to connect to Ripe</param>
        /// <param name="bindingObj">An out parameter returning the configuration object to be reused in your app setup</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, IServiceCollection services, Action<HttpClient, IRipeOptions> optionsBuilder, out TConfig bindingObj)
            where TConfig : class, IRipeConfiguration
        {
            IRipeSdk<TConfig> sdk = new RipeSdk<TConfig>(optionsBuilder);
            builder.Add(new RipeConfigurationSource<TConfig>(sdk));
            services.AddScoped(provider =>
            {
                var hydrate = provider.GetRequiredService<IRipeSdk<TConfig>>();
                return hydrate.Hydrate();
            });
            bindingObj = sdk.Hydrate();
            services.AddSingleton(sdk);
            return builder;
        }

        /// <summary>
        /// Add the Ripe configuration to your <see cref="IConfiguration"/>
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="builder"></param>
        /// <param name="sdk"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, RipeSdk<TConfig> sdk)
            where TConfig : class, IRipeConfiguration
        {
            var ripeBuilder = new ConfigurationBuilder();
            ripeBuilder.Add(new RipeConfigurationSource<TConfig>(sdk));
            builder.AddConfiguration(ripeBuilder.Build());
            return builder;
        }
    }
}
