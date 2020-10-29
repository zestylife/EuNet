using System;

namespace EuNet.Core
{
    public sealed class NetPacket
    {
        public byte[] RawData;

        //! 패킷 최대 크기
        public const int MaxTcpPacketSize = 1024 * 4;

        //! 헤더 사이즈
        public const int HeaderSize = 3;
        public const int UserDataHeaderSize = 7;
        public const int FragmentHeaderSize = 6;
        public const int FragmentedHeaderTotalSize = UserDataHeaderSize + FragmentHeaderSize;

        public const ushort MaxSequence = 32768;
        public const ushort HalfMaxSequence = MaxSequence / 2;

        public const int PacketPoolSize = 1000;

        internal const int MaxUdpHeaderSize = 68;

        internal static readonly int[] PossibleMtu =
        {
            576  - MaxUdpHeaderSize, //minimal
            1232 - MaxUdpHeaderSize,
            1460 - MaxUdpHeaderSize, //google cloud
            1472 - MaxUdpHeaderSize, //VPN
            1492 - MaxUdpHeaderSize, //Ethernet with LLC and SNAP, PPPoE (RFC 1042)
            1500 - MaxUdpHeaderSize  //Ethernet II (RFC 1191)
        };

        internal static readonly int MaxUdpPacketSize = PossibleMtu[PossibleMtu.Length - 1];

        public NetPacket()
        {

        }

        public NetPacket(int size)
        {
            if (size > 0)
            {
                RawData = new byte[size];
                Size = (ushort)size;
            }
            else
            {
                RawData = null;
            }
        }

        public NetPacket(PacketProperty property, int size)
        {
            size += GetHeaderSize(property);
            RawData = new byte[size];
            Property = property;
            Size = (ushort)size;
        }

        public ushort Size
        {
            get { return BitConverter.ToUInt16(RawData, 0); }
            set { FastBitConverter.GetBytes(RawData, 0, value); }
        }

        public PacketProperty Property
        {
            get { return (PacketProperty)(RawData[2] & 0xF); }
            set { RawData[2] = (byte)((RawData[2] & 0xF0) | (byte)value); }
        }

        public DeliveryMethod DeliveryMethod
        {
            get { return (DeliveryMethod)((RawData[2] & 0x70) >> 4); }
            set { RawData[2] = (byte)((RawData[2] & 0x8F) | ((byte)value << 4)); }
        }

        public bool IsFragmented
        {
            get { return (RawData[2] & 0x80) != 0; }
        }

        public void MarkFragmented()
        {
            RawData[2] |= 0x80;
        }

        public ushort Sequence
        {
            get { return BitConverter.ToUInt16(RawData, 3); }
            set { FastBitConverter.GetBytes(RawData, 3, value); }
        }

        // P2p 또는 릴레이일 때 세션 아이디 (아닐 경우 0)
        public ushort P2pSessionId
        {
            get { return BitConverter.ToUInt16(RawData, 5); }
            set { FastBitConverter.GetBytes(RawData, 5, value); }
        }

        public static void SetP2pSessionId(byte[] data, ushort relaySessionId)
        {
            FastBitConverter.GetBytes(data, 5, relaySessionId);
        }

        public ushort FragmentId
        {
            get { return BitConverter.ToUInt16(RawData, 7); }
            set { FastBitConverter.GetBytes(RawData, 7, value); }
        }

        public ushort FragmentPart
        {
            get { return BitConverter.ToUInt16(RawData, 9); }
            set { FastBitConverter.GetBytes(RawData, 9, value); }
        }

        public ushort FragmentsTotal
        {
            get { return BitConverter.ToUInt16(RawData, 11); }
            set { FastBitConverter.GetBytes(RawData, 11, value); }
        }

        //////////////////////////////////////////
        /// 연결 요청, 응답용 헤더 (TCP, UDP)

        public ushort SessionIdForConnection
        {
            get { return BitConverter.ToUInt16(RawData, 3); }
            set { FastBitConverter.GetBytes(RawData, 3, value); }
        }

        ////////////////////////////


        public NetPacket Clone()
        {
            NetPacket newPacket = new NetPacket(Size);
            Buffer.BlockCopy(RawData, 0, newPacket.RawData, 0, Size);

            return newPacket;
        }

        public NetPacket CloneFromPool()
        {
            NetPacket newPacket = NetPool.PacketPool.Alloc(Size);
            Buffer.BlockCopy(RawData, 0, newPacket.RawData, 0, Size);

            return newPacket;
        }

        public void SetBuffer(byte[] buffer, int offset, int packetSize)
        {
            Buffer.BlockCopy(buffer, offset, RawData, 0, packetSize);
            Size = (ushort)packetSize;
        }

        public int GetHeaderSize()
        {
            if (IsFragmented)
                return FragmentedHeaderTotalSize;

            return GetHeaderSize(Property);
        }

        public static int GetHeaderSize(PacketProperty property)
        {
            switch (property)
            {
                case PacketProperty.UserData:
                case PacketProperty.Request:
                case PacketProperty.ViewRequest:
                case PacketProperty.Ack:
                    return UserDataHeaderSize;
                case PacketProperty.RequestConnection:
                case PacketProperty.ResponseConnection:
                    return HeaderSize + 2;
                case PacketProperty.MtuOk:
                    return HeaderSize + 1;
                case PacketProperty.Ping:
                    return HeaderSize + 2;
                case PacketProperty.Pong:
                    return HeaderSize + 10;
                case PacketProperty.JoinP2p:
                case PacketProperty.LeaveP2p:
                case PacketProperty.HolePunchingStart:
                case PacketProperty.HolePunchingEnd:
                    return UserDataHeaderSize;
                default:
                    return HeaderSize;
            }
        }
    }
}
