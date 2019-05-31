using CommandLine;

namespace DnsServer
{
    public class Options
    {
        [Option('s', "origin-server",
            Default = "ns1.e1.ru",
            HelpText = "Enter ip address or domain name of origin DNS-server.")]
        public string OriginDnsServerName { get; set; }
    }
}