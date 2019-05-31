using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DNS.Protocol;

namespace DnsServer
{
    public static class DnsServer
    {
        private const int DnsPortNumber = 53;
        private const int UdpDatagramSize = 10000;

        public static void StartServer(string originDnsServerName, DnsLocalCache dnsLocalCache)
        {
            Console.WriteLine("Starting server...");

            using (var dnsServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                dnsServer.Bind(new IPEndPoint(IPAddress.Any, DnsPortNumber));
                EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                var receivedMessage = new byte[UdpDatagramSize];

                while (true)
                {
                    var receivedBytesCount = dnsServer.ReceiveFrom(receivedMessage, ref remoteAddress);
                    var receivedMessageHeader =
                        TryParseDnsMessage(receivedMessage, receivedBytesCount, Header.FromArray);
                    if (receivedMessageHeader.Id == default(Header).Id &&
                        receivedMessageHeader.Size == default(Header).Size) continue;

                    if (!receivedMessageHeader.Response)
                    {
                        dnsLocalCache.UpdateCache();

                        var receivedRequest =
                            TryParseDnsMessage(receivedMessage, receivedBytesCount, Request.FromArray);
                        if (receivedRequest is null) continue;

                        var cachedResourceRecords = dnsLocalCache.FindResponse(receivedRequest.Questions.First());

                        if (cachedResourceRecords is null)
                        {
                            dnsServer.SendTo(receivedRequest.ToArray(),
                                             new IPEndPoint(ParseStringToIpAddress(originDnsServerName),
                                                            DnsPortNumber));

                            Array.Clear(receivedMessage, 0, receivedBytesCount);

                            receivedBytesCount = dnsServer.Receive(receivedMessage);

                            Response originServerResponse =
                                TryParseDnsMessage(receivedMessage, receivedBytesCount, Response.FromArray);
                            if (originServerResponse is null) continue;

                            dnsServer.SendTo(originServerResponse.ToArray(), remoteAddress);

                            if (dnsLocalCache.ToCacheResponse(originServerResponse))
                                dnsLocalCache.SaveCache();
                        }
                        else
                        {
                            var replyResponse = Response.FromRequest(receivedRequest);

                            foreach (var cachedResourceRecord in cachedResourceRecords)
                                replyResponse.AnswerRecords.Add(cachedResourceRecord);

                            dnsServer.SendTo(replyResponse.ToArray(), remoteAddress);
                        }
                    }

                    Array.Clear(receivedMessage, 0, receivedBytesCount);
                }
            } // ReSharper disable once FunctionNeverReturns
        }

        private static IPAddress ParseStringToIpAddress(string hostNameOrAddress) =>
            IPAddress.TryParse(hostNameOrAddress, out var ipAddress)
                ? ipAddress
                : Dns.GetHostAddresses(hostNameOrAddress).First();

        private static T TryParseDnsMessage<T>(byte[] message, int messageLength, Func<byte[], T> fromArray)
        {
            T parsedDnsMessage = default(T);
            try
            {
                parsedDnsMessage = fromArray(message);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception);
                Array.Clear(message, 0, messageLength);
            }

            return parsedDnsMessage;
        }
    }
}