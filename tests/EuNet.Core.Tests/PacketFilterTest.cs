using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace EuNet.Core.Tests
{
    public class PacketFilterTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void XorPacketFilterTest()
        {
            NetDataWriter writer = new NetDataWriter();
            List<byte> data = new List<byte>()
            {
                1,2,3,4,5,6,7,8,9,10,100,101,102,103,104,105
            };

            foreach (var item in data)
                writer.Write(item);

            NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.UserData, writer);

            XorPacketFilter filter = new XorPacketFilter();

            int encodeSize = filter.Encode(packet);
            filter.Decode(packet);

            NetDataReader reader = new NetDataReader(packet.RawData, packet.GetHeaderSize(), packet.Size);

            for (int i = 0; i < data.Count; i++)
            {
                Assert.AreEqual(data[i], reader.ReadByte());
            }
        }
    }
}