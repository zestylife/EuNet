using System;
using System.Net;

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

        internal static int RelativeSequenceNumber(int number, int expected)
        {
            return (number - expected + NetPacket.MaxSequence + NetPacket.HalfMaxSequence) % NetPacket.MaxSequence - NetPacket.HalfMaxSequence;
        }
    }
}
