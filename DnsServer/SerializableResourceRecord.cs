using System;
using System.Linq;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;

namespace DnsServer
{
    public struct SerializableResourceRecord
    {
        [JsonProperty] public readonly Domain Name;
        [JsonProperty] public readonly RecordType Type;
        [JsonProperty] public readonly RecordClass Class;
        [JsonProperty] public readonly TimeSpan TimeToLive;
        [JsonProperty] public readonly byte[] Data;

        [JsonProperty] public readonly DateTime CachedTime;

        public SerializableResourceRecord(IResourceRecord resourceRecord)
        {
            Name = resourceRecord.Name;
            Type = resourceRecord.Type;
            Class = resourceRecord.Class;
            TimeToLive = resourceRecord.TimeToLive;
            Data = resourceRecord.Data;

            CachedTime = DateTime.Now;
        }

        public override bool Equals(object obj) =>
            obj is SerializableResourceRecord resourceRecord && resourceRecord.Equals(this);

        private bool Equals(SerializableResourceRecord other) =>
            Equals(Name, other.Name) && Type == other.Type && Class == other.Class &&
            (Data == other.Data || !(Data is null) && !(other.Data is null) && Data.SequenceEqual(other.Data));

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (int)Class;
                hashCode = (hashCode * 397) ^ (Data != null ? Data.Length : 0);

                return hashCode;
            }
        }
    }
}