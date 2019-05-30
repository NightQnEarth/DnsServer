using System;
using DNS.Protocol;
using Newtonsoft.Json;

namespace DnsServer
{
    public class CachedRecordKeyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var cachedRecordKey = (CachedRecordKey)value;
            writer.WriteValue($"{cachedRecordKey.ResourceRecordName}|{cachedRecordKey.ResourceRecordType}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            var splitArguments = reader.Value.ToString().Split('|');
            return new CachedRecordKey(new Domain(splitArguments[0]), (RecordType)int.Parse(splitArguments[1]));
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(CachedRecordKey);
    }
}