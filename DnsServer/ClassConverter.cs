using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class ClassConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            Enum.TryParse<RecordClass>(reader.Value.ToString(), out var result);
            return result;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(RecordClass);
    }
}