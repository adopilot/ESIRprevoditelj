using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EsirDriver.JsonConverteri
{
    public class CyrillicStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                // Convert the string to Serbian Cyrillic
                value = SerbianCyrillicConverter.ConvertToCyrillic(value);
            }

            writer.WriteStringValue(value);
        }
    }
}
