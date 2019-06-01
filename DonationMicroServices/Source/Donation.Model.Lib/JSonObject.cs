using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.JSON
{
    public class JsonObject
    {
        public virtual string Serialize()
        {
            return JsonObject.Serialize(this);
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            T t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return t;
        }

        public static string Serialize<T>(T o) where T : new()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
            return json;
        }
    }
}