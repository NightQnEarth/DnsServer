using System;
using DNS.Protocol.ResourceRecords;

namespace DnsServer
{
    public struct CachedRecordValue
    {
        public readonly ResourceRecord ResourceRecord;
        public readonly DateTime CachedTime;

        public CachedRecordValue(ResourceRecord resourceRecord)
        {
            ResourceRecord = resourceRecord;
            CachedTime = DateTime.Now;
        }
    }
}