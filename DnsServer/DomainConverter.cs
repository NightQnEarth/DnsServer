using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class DomainConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer) =>
            new Domain(reader.Value.ToString());

        public override bool CanConvert(Type objectType) => objectType == typeof(Domain);
    }
}