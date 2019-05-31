namespace DnsServer
{
    static class Program
    {
        private const string CacheFileName = "DnsLocalCache.json";

        public static void Main(string[] args) =>
            DnsServer.StartServer(UIDataParser.GetInputData(args).OriginDnsServerName,
                                  new DnsLocalCache(CacheFileName));
    }
}