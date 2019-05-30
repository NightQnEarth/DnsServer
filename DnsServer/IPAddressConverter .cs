using System;
using System.Net;
using Newtonsoft.Json;

namespace DnsServer
{
    public class IPAddressConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer) =>
            IPAddress.Parse((string)reader.Value);

        public override bool CanConvert(Type objectType) => objectType == typeof(IPAddress);
    }
}