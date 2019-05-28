using System.Net;
using System.Net.Sockets;

namespace DnsServer
{
    public static class DnsServer
    {
        private const int DnsPortNumber = 53;
        private const int DatagramSize = 4096; // TODO: change on 512

        public static void StartServer(string originDnsServer, DnsLocalCache dnsLocalCache)
        {
            dnsLocalCache.LoadCache();

            using (var dnsServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                dnsServer.Bind(new IPEndPoint(IPAddress.Loopback, DnsPortNumber));

                while (true)
                {
                    EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                    var buffer = new byte[DatagramSize];

                    dnsServer.ReceiveFrom(buffer, ref remoteAddress);

                    // TODO
                }
            }
        }
    }
}