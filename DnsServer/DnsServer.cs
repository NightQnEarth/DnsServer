using System;
using System.Net;
using System.Net.Sockets;
using DNS.Client;
using DNS.Protocol;

namespace DnsServer
{
    public static class DnsServer
    {
        private const int DnsPortNumber = 53;
        private const int DatagramSize = 4096; // TODO: change on 512

        public static void StartServer(string originDnsServerName, DnsLocalCache dnsLocalCache)
        {
            Console.WriteLine("Starting server...");

            dnsLocalCache.LoadCache();

            var dnsServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            dnsServer.Bind(new IPEndPoint(IPAddress.Loopback, DnsPortNumber));

            while (true)
            {
                EndPoint remoteAddress = new IPEndPoint(IPAddress.Any, 0);
                var receivedMessage = new byte[DatagramSize];

                dnsServer.ReceiveFrom(receivedMessage, ref remoteAddress);

                dnsLocalCache.UpdateCache();

                Header receivedMessageHeader;

                try
                {
                    receivedMessageHeader = Header.FromArray(receivedMessage);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    continue;
                }

                if (receivedMessageHeader.Response)
                    dnsLocalCache.CacheResponse(Response.FromArray(receivedMessage));
                else
                {
                    var receivedRequest = Request.FromArray(receivedMessage);

                    var cachedResourceRecords = dnsLocalCache.FindResponse(Question.FromArray(receivedMessage, 0));

                    if (cachedResourceRecords is null)
                    {
                        var clientRequest = new ClientRequest(originDnsServerName, DnsPortNumber, receivedRequest);
                        var originServerResponse = clientRequest.Resolve().Result;

                        dnsLocalCache.CacheResponse(originServerResponse);

                        SendMessage(dnsServer, originServerResponse, remoteAddress);
                    }
                    else
                    {
                        var replyResponse = Response.FromRequest(receivedRequest);
                        foreach (var cachedResourceRecord in cachedResourceRecords)
                            replyResponse.AnswerRecords.Add(cachedResourceRecord);
                        SendMessage(dnsServer, replyResponse, remoteAddress);
                    }

                    dnsServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    dnsServer.Bind(new IPEndPoint(IPAddress.Loopback, DnsPortNumber));

                    if (!dnsLocalCache.Empty)
                        dnsLocalCache.SaveCache();
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private static void SendMessage(Socket socket, IMessage originSeverResponse, EndPoint remoteAddress)
        {
            socket.Connect(remoteAddress);
            socket.Send(originSeverResponse.ToArray());
            socket.Close();
        }
    }
}