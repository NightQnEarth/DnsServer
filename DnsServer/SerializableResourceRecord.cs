using System;
using System.Linq;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DnsServer
{
    public struct SerializableResourceRecord
    {
        public readonly Domain Name;
        public readonly RecordType Type;
        public readonly RecordClass Class;
        public readonly TimeSpan TimeToLive;
        public readonly byte[] Data;

        public readonly DateTime CachedTime;

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
            Equals(Name, other.Name) && Type == other.Type && Class == other.Class && !(other.Data is null) &&
            Data.SequenceEqual(other.Data);

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