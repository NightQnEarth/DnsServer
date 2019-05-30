using System;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace DnsServer
{
    public struct CachedRecordValue
    {
        public readonly string Name;
        public readonly byte[] Data;
        public readonly RecordType Type;
        public readonly RecordClass Class;
        public readonly int TimeToLive;

        public readonly DateTime CachedTime;

        public CachedRecordValue(IResourceRecord resourceRecord)
        {
            Name = resourceRecord.Name.ToString();
            Data = resourceRecord.Data;
            Type = resourceRecord.Type;
            Class = resourceRecord.Class;
            TimeToLive = resourceRecord.TimeToLive.Seconds;

            CachedTime = DateTime.Now;
        }
    }
}