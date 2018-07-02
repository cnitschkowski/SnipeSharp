using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SnipeSharp.Common;
using SnipeSharp.Endpoints.Models;
using SnipeSharp.Exceptions;

namespace SnipeSharp.Endpoints
{
    public class QueryParameterBuilder : IQueryParameterBuilder
    {
        public Dictionary<string, string> GetParameters(object item)
        {
            var values = new Dictionary<string, string>();
            

            if (item == null)
            {
                return values;
            }

            foreach (var property in item.GetType().GetProperties())
            {
                var nameAttribute = property.GetCustomAttribute<NameAttribute>();

                if (nameAttribute == null)
                {
                    continue;
                }

                var propValue = property.GetValue(item)?.ToString();

                var requestAttribute = nameAttribute as RequestHeaderAttribute;

                // Abort in case of missing required headers.
                if (string.IsNullOrEmpty(propValue) && requestAttribute?.IsRequired == true)
                {
                    throw new RequiredValueIsNullException($"Property {property.Name} cannot be null.");
                }

                // Skip empty parameters.
                if (string.IsNullOrEmpty(propValue))
                {
                    continue;
                }

                string headerName = nameAttribute.Name.Replace("\"", "");

                values.Add(headerName, propValue);
            }


            // Add foreach loop to run with .Replace("\"", ""); 
            if (item.GetType() == typeof(SnipeSharp.Endpoints.Models.Asset))
            {
                if (item.GetType().GetProperty("CustomFields") != null) {
                    Dictionary<string, string> customFields = item.GetType().GetProperty("CustomFields").GetValue(item) as Dictionary<string, string>;
                    values = values.Concat(customFields).ToDictionary(e => e.Key, e => e.Value);
                }
            }

            return values;
        }
    }
}