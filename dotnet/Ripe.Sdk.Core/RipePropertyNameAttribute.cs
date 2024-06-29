using System;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace Ripe.Sdk.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RipePropertyNameAttribute : Attribute
    {
        public string PropertyName { get; set; }
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
