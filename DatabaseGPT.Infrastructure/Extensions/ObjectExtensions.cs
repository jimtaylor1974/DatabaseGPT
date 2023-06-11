using System.Reflection;
using Newtonsoft.Json.Linq;

namespace DatabaseGPT.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static Dictionary<string, object> AsDictionary(this object data, StringComparer comparer = null)
        {
            if (data == null)
            {
                return null;
            }

            if (data is IDictionary<string, object> objects)
            {
                return new Dictionary<string, object>(objects);
            }

            if (data is JObject jObject)
            {
                return jObject.ToObject<Dictionary<string, object>>();
            }

            const BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;

            return data
                .GetType()
                .GetProperties(publicAttributes)
                .Where(property => property.CanRead)
                .ToDictionary(property => property.Name, property => property.GetValue(data, null),
                    comparer ?? StringComparer.OrdinalIgnoreCase);
        }
    }
}