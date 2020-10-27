using EuNet.Core;

public enum Protocol : ushort
{
    Move,
    Attack,
}

public static class ProtocolExtension
{
    public static void Write(this NetDataWriter writer, Protocol value)
    {
        writer.Write((ushort)value);
    }

    public static Protocol GetProtocol(this NetDataReader reader)
    {
        return (Protocol)reader.ReadUInt16();
    }
}