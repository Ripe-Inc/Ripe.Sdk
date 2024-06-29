using System;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace Ripe.Sdk.Core
{
    /// <summary>
    /// Specify a name to use other than the property name when making the Ripe request
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RipePropertyNameAttribute : Attribute
    {
        /// <summary>
        /// The property name to use
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Specify a name to use other than the property name when making the Ripe request
        /// </summary>
        /// <param name="propertyName">The property name to use</param>
        public RipePropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
    internal sealed class RipePropertyNameContract
    {
        public static void Contract(JsonTypeInfo typeInfo)
        {
            foreach(JsonPropertyInfo prop in typeInfo.Properties)
            {
                var attr = prop.AttributeProvider.GetCustomAttributes(true).FirstOrDefault(f => f is RipePropertyNameAttribute);
                if(attr is RipePropertyNameAttribute attribute)
                {
                    prop.Name = attribute.PropertyName;
                }
            }
        }
    }
}
