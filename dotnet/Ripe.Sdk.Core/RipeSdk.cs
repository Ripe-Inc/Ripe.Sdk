using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Ripe.Sdk.Core
{
    /// <summary>
    /// Connect to the Ripe API to get your centralized configuration. <code>Note:</code> This class should only be 
    /// initialized once per lifetime of the application, and either stored as a static variable, or injected as a singleton.
    /// </summary>
    /// <typeparam name="TConfig">Your implementation of <see cref="IRipeConfiguration"/></typeparam>
    public interface IRipeSdk<TConfig>
        where TConfig : class, IRipeConfiguration
    {
        /// <summary>
        /// The date time that the local cache will expire
        /// </summary>
        DateTime Expiry { get; }

        /// <summary>
        /// Refreshes your configuration object and returns the result
        /// </summary>
        /// <returns>Your configuration object</returns>
        Task<TConfig> HydrateAsync();

        /// <inheritdoc cref="HydrateAsync"/>
        TConfig Hydrate();
    }

    /// <summary>
    /// Connect to the Ripe API to get your centralized configuration. <code>Note:</code> This class should only be 
    /// initialized once per lifetime of the application, and either stored as a static variable, or injected as a singleton.
    /// </summary>
    /// <typeparam name="TConfig">Your implementation of <see cref="IRipeConfiguration"/></typeparam>
    public class RipeSdk<TConfig> : IRipeSdk<TConfig>
        where TConfig : class, IRipeConfiguration
    {
        /// <inheritdoc/>
        public DateTime Expiry { get; private set; }

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            {
                Modifiers = { RipePropertyNameContract.Contract }
            }
        };
        private readonly IRipeOptions _options;
        private readonly HttpClient _httpClient;
        private TConfig _cache;
        /// <summary>
        /// Initializes a new instance of the <see cref="RipeSdk{TConfig}"/> class.
        /// </summary>
        /// <param name="optionsBuilder">Builder to configure options</param>
        public RipeSdk(Action<IRipeOptions> optionsBuilder)
            : this((httpClient, options) => optionsBuilder.Invoke(options)) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RipeSdk{TConfig}"/> class.
        /// </summary>
        /// <param name="optionsBuilder">Builder to configure options with a custom <see cref="HttpClient"/></param>
        public RipeSdk(Action<HttpClient, IRipeOptions> optionsBuilder)
        {
            _httpClient = new HttpClient();
            var options = new RipeOptions();
            optionsBuilder(_httpClient, options);
            if (!options.Validate())
            {
                return;
            }

            _options = options;
            _httpClient.DefaultRequestHeaders.Add("x-ripe-key", options.ApiKey);
        }

        /// <inheritdoc/>
        public async Task<TConfig> HydrateAsync()
        {
            try
            {
                if (_cache != null && !CheckTtl())
                {
                    return _cache;
                }

                HydrationRequest request = new HydrationRequest()
                {
                    Schema = GetSchema(),
                    Version = _options.Version
                };

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _options.Uri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await _httpClient.SendAsync(msg).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var obj = JsonSerializer.Deserialize<HydrationResponse<TConfig>>(content, _serializerOptions)
                        ?? throw new JsonException("Failed to deserialize IRipeConfiguration");
                    if (obj.Data != null)
                    {
                        _cache = obj.Data;
                        Expiry = DateTime.Now.AddSeconds(_options.CacheExpiry);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(content);
                    throw new Exception($"Ripe hydration returned a bad status code: {response.ReasonPhrase}");
                }

                return _cache;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public TConfig Hydrate()
        {
            try
            {
                if (_cache != null && !CheckTtl())
                {
                    return _cache;
                }

                HydrationRequest request = new HydrationRequest()
                {
                    Schema = GetSchema(),
                    Version = _options.Version
                };

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _options.Uri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = _httpClient.SendAsync(msg).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    var obj = JsonSerializer.Deserialize<HydrationResponse<TConfig>>(content, _serializerOptions)
                        ?? throw new JsonException("Failed to deserialize IRipeConfiguration");
                    if (obj.Data != null)
                    {
                        _cache = obj.Data;
                        Expiry = DateTime.Now.AddSeconds(_options.CacheExpiry);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(content);
                    throw new Exception($"Ripe hydration returned a bad status code: {response.ReasonPhrase}");
                }

                return _cache;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Check the time against the <see cref="Expiry"/>
        /// </summary>
        /// <returns>true if expired</returns>
        private bool CheckTtl()
        {
            if (Expiry <= DateTime.Now)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the Ripe config schema based on the properties in the derrived type
        /// </summary>
        /// <returns></returns>
        private string[] GetSchema()
        {
            var props = typeof(TConfig).GetProperties();
            List<string> result = new List<string>();
            foreach (var prop in props)
            {
                ExtractType(result, "", prop);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Helper method for getting the property type
        /// </summary>
        /// <param name="result"></param>
        /// <param name="root"></param>
        /// <param name="prop"></param>
        private static void ExtractType(List<string> result, string root, PropertyInfo prop)
        {
            string name = prop.Name;
            var propNameAttr = prop.GetCustomAttribute<RipePropertyNameAttribute>();
            if(propNameAttr != null)
            {
                name = propNameAttr.PropertyName;
            }

            if (prop.PropertyType == typeof(string)
                || prop.PropertyType == typeof(int)
                || prop.PropertyType == typeof(decimal)
                || prop.PropertyType == typeof(bool)
                || typeof(IDictionary).IsAssignableFrom(prop.PropertyType))
            {
                result.Add((root + "." + name).TrimStart('.'));
            }
            else
            {
                root += "." + name;
                var props = prop.PropertyType.GetProperties();
                foreach (var propDeep in props)
                {
                    ExtractType(result, root, propDeep);
                }
            }
        }
    }
}
