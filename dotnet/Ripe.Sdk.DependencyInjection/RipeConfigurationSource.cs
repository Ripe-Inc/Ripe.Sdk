using Microsoft.Extensions.Configuration;
using Ripe.Sdk.Core;
using System.Collections;
using System.Collections.Generic;

namespace Ripe.Sdk.DependencyInjection
{
    internal class RipeConfigurationSource<T> : IConfigurationSource
        where T : class, IRipeConfiguration
    {
        private readonly IRipeSdk<T> _sdk;

        public RipeConfigurationSource(IRipeSdk<T> sdk)
        {
            _sdk = sdk;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RipeConfigurationProvider<T>(_sdk);
        }
    }

    public class RipeConfigurationProvider<T> : ConfigurationProvider
        where T : class, IRipeConfiguration
    {
        private readonly IRipeSdk<T> _sdk;

        public RipeConfigurationProvider(IRipeSdk<T> sdk)
        {
            _sdk = sdk;
        }
        public override void Load()
        {
            T config = _sdk.Hydrate();
            Data = ToDictionary(config);
        }

        private Dictionary<string, string> ToDictionary(T config)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            ToDictionary(config, result, string.Empty);
            return result;
        }

        private void ToDictionary(object obj, Dictionary<string, string> result, string prefix)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string)
                    || prop.PropertyType == typeof(int)
                    || prop.PropertyType == typeof(decimal)
                    || prop.PropertyType == typeof(bool)
                    || typeof(IDictionary).IsAssignableFrom(prop.PropertyType))
                {
                    result.Add(GetPrefix(prefix, prop.Name), prop.GetValue(obj).ToString());
                }
                else
                {
                    ToDictionary(prop.GetValue(obj), result, GetPrefix(prefix, prop.Name));
                }
            }
        }

        private string GetPrefix(string prefix, string name)
        {
            if(string.IsNullOrWhiteSpace(prefix))
            {
                return name;
            }

            return $"{prefix}:{name}";
        }
    }
}
