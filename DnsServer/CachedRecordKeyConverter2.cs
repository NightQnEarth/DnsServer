using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class CachedRecordKeyConverter2 : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            var values = reader.Value.ToString().Split();
            return (values[0], (RecordClass)int.Parse(values[1]));
        }

        public override bool CanConvert(Type objectType) => objectType == (string.Empty, RecordClass.IN).GetType();
    }
}