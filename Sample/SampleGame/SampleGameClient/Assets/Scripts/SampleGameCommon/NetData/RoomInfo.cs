using EuNet.Core;

namespace SampleGameCommon
{
    [NetDataObject]
    public class RoomInfo
    {
        public string Name { get; set; }
        public int Id;
        public RoomSlotInfo[] Slots;
        private int PrivateField;

        [IgnoreMember]
        public int Ignore;
    }

    [NetDataObject]
    public class RoomSlotInfo
    {
        public int SlotId;

        [IgnoreMember]
        public int Ignore;
    }

    [NetDataObject]
    public class GenericClass<T>
    {
        public T SlotId;

        [IgnoreMember]
        public int Ignore;
    }
}
