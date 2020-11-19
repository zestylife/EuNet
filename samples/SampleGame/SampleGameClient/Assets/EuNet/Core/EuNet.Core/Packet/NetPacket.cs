using System;

namespace EuNet.Core
{
    /// <summary>
    /// 패킷 클래스
    /// </summary>
    public sealed class NetPacket
    {
        /// <summary>
        /// 버퍼
        /// </summary>
        public byte[] RawData;

        /// <summary>
        /// TCP 패킷 최대 크기. 채널에 할당되는 버퍼의 크기에 영향을 주므로 크면 메모리 사용이 커짐.
        /// </summary>
        public const int MaxTcpPacketSize = 1024 * 4;

        /// <summary>
        /// 기본 헤더 사이즈
        /// 0~1 : 패킷 사이즈
        /// 2 : PacketProperty , DeliveryMethod , IsFragmented
        /// </summary>
        public const int HeaderSize = 3;

        /// <summary>
        /// 유저 데이터 헤더 사이즈
        /// </summary>
        public const int UserDataHeaderSize = 7;

        /// <summary>
        /// 큰 패킷을 분할하여 보낼때 헤더 사이즈
        /// </summary>
        public const int FragmentHeaderSize = 6;
        public const int FragmentedHeaderTotalSize = UserDataHeaderSize + FragmentHeaderSize;

        public const ushort MaxSequence = 32768;
        public const ushort HalfMaxSequence = MaxSequence / 2;

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

        /// <summary>
        /// 패킷 크기 (버퍼의 크기는 아님. 사용된 크기만 해당)
        /// </summary>
        public ushort Size
        {
            get { return BitConverter.ToUInt16(RawData, 0); }
            set { FastBitConverter.GetBytes(RawData, 0, value); }
        }

        /// <summary>
        /// 패킷 특성
        /// </summary>
        public PacketProperty Property
        {
            get { return (PacketProperty)(RawData[2] & 0xF); }
            set { RawData[2] = (byte)((RawData[2] & 0xF0) | (byte)value); }
        }

        /// <summary>
        /// 전송 방법
        /// </summary>
        public DeliveryMethod DeliveryMethod
        {
            get { return (DeliveryMethod)((RawData[2] & 0x70) >> 4); }
            set { RawData[2] = (byte)((RawData[2] & 0x8F) | ((byte)value << 4)); }
        }

        /// <summary>
        /// 분할된 패킷여부
        /// </summary>
        public bool IsFragmented
        {
            get { return (RawData[2] & 0x80) != 0; }
        }

        public void MarkFragmented()
        {
            RawData[2] |= 0x80;
        }

        /// <summary>
        /// RUDP 에서 순서를 보장하기 위해서 사용하는 시퀀스
        /// </summary>
        public ushort Sequence
        {
            get { return BitConverter.ToUInt16(RawData, 3); }
            set { FastBitConverter.GetBytes(RawData, 3, value); }
        }

        /// <summary>
        /// P2P 또는 Relay 사용시 세션 아이디 (아닐 경우 0)
        /// </summary>
        public ushort P2pSessionId
        {
            get { return BitConverter.ToUInt16(RawData, 5); }
            set { FastBitConverter.GetBytes(RawData, 5, value); }
        }

        public static void SetP2pSessionId(byte[] data, ushort relaySessionId)
        {
            FastBitConverter.GetBytes(data, 5, relaySessionId);
        }

        /// <summary>
        /// UDP 분할패킷 아이디
        /// </summary>
        public ushort FragmentId
        {
            get { return BitConverter.ToUInt16(RawData, 7); }
            set { FastBitConverter.GetBytes(RawData, 7, value); }
        }

        /// <summary>
        /// UDP 분할패킷 파트
        /// </summary>
        public ushort FragmentPart
        {
            get { return BitConverter.ToUInt16(RawData, 9); }
            set { FastBitConverter.GetBytes(RawData, 9, value); }
        }

        /// <summary>
        /// UDP 분할패킷의 분할된 총 개수
        /// </summary>
        public ushort FragmentsTotal
        {
            get { return BitConverter.ToUInt16(RawData, 11); }
            set { FastBitConverter.GetBytes(RawData, 11, value); }
        }

        /// <summary>
        /// TCP, UDP 연결 요청, 응답때 사용될 세션아이디
        /// </summary>
        public ushort SessionIdForConnection
        {
            get { return BitConverter.ToUInt16(RawData, 3); }
            set { FastBitConverter.GetBytes(RawData, 3, value); }
        }

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

        /// <summary>
        /// 헤더 사이즈를 구함
        /// </summary>
        /// <returns>패킷의 헤더 사이즈</returns>
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
                case PacketProperty.AliveCheck:
                    return HeaderSize + 1;
                default:
                    return HeaderSize;
            }
        }
    }
}
