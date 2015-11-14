using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Orchestrate.Tests.Utility
{
    public class CustomSerializer
    {
        public static JsonSerializer Create()
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new StringEnumConverter());
            return JsonSerializer.Create(jsonSettings);
        }
    }
}
