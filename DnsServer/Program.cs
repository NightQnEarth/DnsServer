using System;

namespace DnsServer
{
    static class Program
    {
        private const string OriginDnsServerName = "8.8.8.8";
        private const string CacheFileName = "DnsLocalCache.json";

        public static void Main()
        {
            var dnsLocalCache = new DnsLocalCache(OriginDnsServerName, CacheFileName);

            try
            {
                DnsServer.StartServer(OriginDnsServerName, dnsLocalCache);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                if (!dnsLocalCache.Empty)
                    dnsLocalCache.SaveCache();
            }
        }
    }
}