using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DNS.Protocol;

namespace DnsServer
{
    public static class DnsServer
    {
        private const int DnsPortNumber = 53;
        private const int UdpDatagramSize = 512;

        [Conditional("DEBUG")]
        public static void DebugDataPrint(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public static void StartServer(string originDnsServerName, DnsLocalCache dnsLocalCache)
        {
            DebugDataPrint("Starting server...");

            using (var dnsServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                dnsServer.Bind(new IPEndPoint(IPAddress.Loopback, DnsPortNumber));
                EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                var receivedMessage = new byte[UdpDatagramSize];

                while (true)
                    try
                    {
                        DebugDataPrint("");

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

                            var question = receivedRequest.Questions.First();

                            DebugDataPrint(
                                $"Got request with question: [{question.Name} {question.Type} {question.Class}]");

                            var cachedResourceRecords = dnsLocalCache.FindResponse(question);

                            if (cachedResourceRecords is null)
                            {
                                DebugDataPrint($"Make request to origin server '{originDnsServerName}'.");

                                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                                               ProtocolType.Udp))
                                {
                                    socket.SendTo(receivedRequest.ToArray(),
                                                  new IPEndPoint(ParseStringToIpAddress(originDnsServerName),
                                                                 DnsPortNumber));
                                    Array.Clear(receivedMessage, 0, receivedBytesCount);
                                    socket.Receive(receivedMessage);
                                }

                                Response originServerResponse = TryParseDnsMessage(receivedMessage, receivedBytesCount,
                                                                                   Response.FromArray);
                                if (originServerResponse is null)
                                {
                                    DebugDataPrint($"Couldn't get response from '{originDnsServerName}'.");
                                    continue;
                                }

                                DebugDataPrint(string.Concat(
                                                   $"Got response from '{originDnsServerName}' ",
                                                   $"['{originServerResponse.AnswerRecords.Count}' answer-RR; ",
                                                   $"'{originServerResponse.AuthorityRecords.Count}' authority-RR; ",
                                                   $"'{originServerResponse.AdditionalRecords.Count}' additional-RR]"));

                                dnsServer.SendTo(originServerResponse.ToArray(), remoteAddress);

                                if (dnsLocalCache.ToCacheResponse(originServerResponse))
                                    dnsLocalCache.SaveCache();
                            }
                            else
                            {
                                DebugDataPrint("Answer resource records will take from cache.");

                                var replyResponse = Response.FromRequest(receivedRequest);

                                foreach (var cachedResourceRecord in cachedResourceRecords)
                                    replyResponse.AnswerRecords.Add(cachedResourceRecord);

                                dnsServer.SendTo(replyResponse.ToArray(), remoteAddress);
                            }
                        }

                        Array.Clear(receivedMessage, 0, receivedBytesCount);
                    }
                    catch (SocketException)
                    {
                        DebugDataPrint($"Couldn't get response from '{originDnsServerName}'.");
                    }
            }
        }

        private static IPAddress ParseStringToIpAddress(string hostNameOrAddress) =>
            IPAddress.TryParse(hostNameOrAddress, out var ipAddress)
                ? ipAddress
                : Dns.GetHostAddresses(hostNameOrAddress).First();

        private static T TryParseDnsMessage<T>(byte[] message, int messageLength, Func<byte[], T> fromArray)
        {
            T parsedDnsMessage = default;
            try
            {
                parsedDnsMessage = fromArray(message);
            }
            catch (ArgumentException)
            {
                Array.Clear(message, 0, messageLength);
            }

            return parsedDnsMessage;
        }
    }
}