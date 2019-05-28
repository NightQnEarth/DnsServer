using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DnsServer
{
    public class DnsLocalCache
    {
        private readonly string cacheFileName;
        private readonly DNS.Server.DnsServer originDnsServer;
        private Dictionary<CachedRecordKey, List<CachedRecordValue>> cachedRecords;

        public DnsLocalCache(string originDnsServerName, string cacheFileName)
        {
            originDnsServer = new DNS.Server.DnsServer(originDnsServerName);
            this.cacheFileName = cacheFileName;
        }

        public bool Empty => cachedRecords.Count > 0;

        public void LoadCache()
        {
            try
            {
                cachedRecords = JsonConvert.DeserializeObject<Dictionary<CachedRecordKey, List<CachedRecordValue>>>(
                    File.ReadAllText(cacheFileName));
            }
            catch (Exception exception)
            {
                Console.WriteLine("LoadCache():");
                Console.WriteLine(exception);
                cachedRecords = new Dictionary<CachedRecordKey, List<CachedRecordValue>>();
            }
        }

        public void SaveCache()
        {
            try
            {
                using (var fileStream = new FileStream(cacheFileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream.Name))
                        streamWriter.WriteLine(JsonConvert.SerializeObject(cachedRecords));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private static bool VerifyCachedRecord(CachedRecordValue recordValue) =>
            DateTime.Now - recordValue.CachedTime >= recordValue.ResourceRecord.TimeToLive;

        public void UpdateCache() { }
    }
}