using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ripe.Sdk.Core;
using System;

namespace Ripe.Sdk.DependencyInjection
{
    public static class RipeConfigExtensions
    {
        /// <summary>
        /// Add the specified Ripe configuration to your <see cref="IConfigurationBuilder"/>
        /// </summary>
        /// <typeparam name="TConfig">The <see cref="IRipeConfiguration"/> that defines your Ripe configuration</typeparam>
        /// <param name="builder"></param>
        /// <param name="optionsBuilder">The options you use to connect to Ripe</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, Action<IRipeOptions> optionsBuilder)
            where TConfig : class, IRipeConfiguration
        {
            RipeSdk<TConfig> sdk = new RipeSdk<TConfig>(optionsBuilder);
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
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, IServiceCollection services, Action<IRipeOptions> optionsBuilder)
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
        public static IConfigurationBuilder AddRipe<TConfig>(this IConfigurationBuilder builder, IServiceCollection services, Action<IRipeOptions> optionsBuilder, out TConfig bindingObj)
            where TConfig : class, IRipeConfiguration
        {
            RipeSdk<TConfig> sdk = new RipeSdk<TConfig>(optionsBuilder);
            builder.Add(new RipeConfigurationSource<TConfig>(sdk));
            services.AddScoped(async provider =>
            {
                var hydrate = provider.GetRequiredService<RipeSdk<TConfig>>();
                return await hydrate.HydrateAsync();
            });
            bindingObj = sdk.Hydrate();
            services.AddSingleton(sdk);
            return builder;
        }
    }
}
