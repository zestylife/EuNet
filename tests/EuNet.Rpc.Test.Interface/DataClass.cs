using EuNet.Core;

namespace Rpc.Test.Interface
{
    [NetDataObject]
    public class DataClass
    {
        public int Int;
        public string String;
        public int Property { get; set; }

        [IgnoreMember]
        public int IgnoreInt;

        [IgnoreMember]
        public int IgnoreProperty { get; set; }
    }
    
    [NetDataObject]
    public class GenericDataClass<T>
    {
        public T GenericValue;
    }

    public class InterfaceSerializeClass : INetSerializable
    {
        public int Int;

        public void Serialize(NetDataWriter writer)
        {
            writer.Write(Int);
        }

        public void Deserialize(NetDataReader reader)
        {
            Int = reader.ReadInt32();
        }
    }
}
