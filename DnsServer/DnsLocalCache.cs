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
        private Dictionary<CachedRecordKey, DateTime> cachedRecords;

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
                cachedRecords = JsonConvert.DeserializeObject<Dictionary<CachedRecordKey, DateTime>>(
                    File.ReadAllText(cacheFileName));
            }
            catch (Exception exception)
            {
                Console.WriteLine("LoadCache():");
                Console.WriteLine(exception);
                cachedRecords = new Dictionary<CachedRecordKey, DateTime>();
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
    }
}