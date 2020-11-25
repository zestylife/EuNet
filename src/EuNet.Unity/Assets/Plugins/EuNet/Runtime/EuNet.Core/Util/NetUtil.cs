using System;
using System.Net;
using System.Net.Sockets;

namespace EuNet.Core
{
    public static class NetUtil
    {
        public static IPEndPoint GetEndPoint(string address, int port)
        {
            try
            {
                return new IPEndPoint(IPAddress.Parse(address), port);
            }
            catch
            {

            }

            if ("any".Equals(address, StringComparison.OrdinalIgnoreCase))
            {
                return new IPEndPoint(IPAddress.Any, port);
            }
            else if ("IpV6Any".Equals(address, StringComparison.OrdinalIgnoreCase))
            {
                return new IPEndPoint(IPAddress.IPv6Any, port);
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(address);
                if (hostEntry.AddressList.Length > 0)
                {
                    return new IPEndPoint(hostEntry.AddressList[0], port);
                }
            }

            return null;
        }

        public static string GetPublicIpAddress(int timeoutMilliseconds = 5000)
        {
            var client = new WebClient();
            var task = client.DownloadStringTaskAsync("http://icanhazip.com");
            if (task.Wait(timeoutMilliseconds) == false)
                return string.Empty;

            return task.Result.Trim();
        }

        public static string GetLocalIpAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return host.AddressList[i].ToString();
                }
            }
            return string.Empty;
        }

        internal static int RelativeSequenceNumber(int number, int expected)
        {
            return (number - expected + NetPacket.MaxSequence + NetPacket.HalfMaxSequence) % NetPacket.MaxSequence - NetPacket.HalfMaxSequence;
        }
    }
}
