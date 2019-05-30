using System;
using DNS.Protocol.ResourceRecords;

namespace DnsServer
{
    public struct CachedRecordValue
    {
        public readonly IResourceRecord ResourceRecord;
        public readonly DateTime CachedTime;

        public CachedRecordValue(IResourceRecord resourceRecord)
        {
            ResourceRecord = resourceRecord;
            CachedTime = DateTime.Now;
        }
    }
}