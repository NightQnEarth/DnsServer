using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class RecordTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            Enum.TryParse<RecordType>(reader.Value.ToString(), out var result);
            return result;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(RecordType);
    }
}