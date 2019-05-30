using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class RecordTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(((RecordType)value).ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer) =>
            (RecordType)int.Parse(reader.Value.ToString());

        public override bool CanConvert(Type objectType) => objectType == typeof(RecordType);
    }
}