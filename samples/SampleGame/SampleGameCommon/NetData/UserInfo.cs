using EuNet.Core;
using System;

namespace SampleGameCommon
{
    public class UserInfo : INetSerializable
    {
        public string Name;

        public void Serialize(NetDataWriter writer)
        {
            writer.Write(Name);
        }

        public void Deserialize(NetDataReader reader)
        {
            Name = reader.ReadString();
        }
    }
}
