using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EuNet.Core.Tests
{
    public class PacketFilterTest
    {
        [SetUp]
        public void Setup()
        {

        }

        private byte[] GetData(int size)
        {
            byte[] data = new byte[size];

            Random rand = new Random();
            rand.NextBytes(data);

            return data;
        }

        public void TestBase(
            IPacketFilter filter,
            int dataSize)
        {
            NetDataWriter writer = new NetDataWriter();
            byte[] data = GetData(dataSize);

            foreach (var item in data)
                writer.Write(item);

            NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.UserData, writer);

            var encodedPacket = filter.Encode(packet);
            var decodedPacket = filter.Decode(encodedPacket);

            NetDataReader reader = new NetDataReader(decodedPacket);

            Assert.GreaterOrEqual(decodedPacket.Size, packet.Size);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], reader.ReadByte());
            }
        }

        [Test]
        public void XorPacketFilterTest(
            [Values(54, 55, 56, 57, 58, 59, 60)] int dataSize)
        {
            TestBase(new XorPacketFilter(), dataSize);
        }

        [Test]
        public void TeaPacketFilterTest(
         [Values(54, 55, 56, 57, 58, 59, 60)] int dataSize)
        {
            TestBase(new TeaPacketFilter(), dataSize);
        }

    }
}