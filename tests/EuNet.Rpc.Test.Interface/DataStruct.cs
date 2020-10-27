using EuNet.Core;

namespace Rpc.Test.Interface
{
    [NetDataObject]
    public struct DataStruct
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
    public struct GenericDataStruct<T>
    {
        public T GenericValue;
    }
}
