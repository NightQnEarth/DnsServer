using System;
using Newtonsoft.Json;

namespace DnsServer
{
    public class Converter<T> : JsonConverter<T>
    {
        private readonly Func<JsonReader, T> objectParser;

        public Converter(Func<JsonReader, T> objectParser) => this.objectParser = objectParser;

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
                                   JsonSerializer serializer) => objectParser(reader);
    }
}