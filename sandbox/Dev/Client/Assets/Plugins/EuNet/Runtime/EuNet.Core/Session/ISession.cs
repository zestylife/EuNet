using System;
using System.Threading.Tasks;

namespace EuNet.Core
{
    /// <summary>
    /// 세션 인터페이스
    /// 세션은 접속을 단위로 하나씩 생성되며, 채널을 가집니다
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 세션 고유 아이디
        /// </summary>
        ushort SessionId { get; }

        /// <summary>
        /// Tcp Channel
        /// </summary>
        TcpChannel TcpChannel { get; }

        /// <summary>
        /// Udp Channel
        /// </summary>
        UdpChannel UdpChannel { get; }

        /// <summary>
        /// 현재 채널 상태
        /// </summary>
        SessionState State { get; }

        /// <summary>
        /// 세션 요청에 대한 처리기
        /// </summary>
        SessionRequest SessionRequest { get; }

        /// <summary>
        /// 세션을 초기화 한다
        /// 세션은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
        /// <param name="info">초기화시 정보</param>
        void Init(SessionInitializeInfo info);

        /// <summary>
        /// 세션을 닫는다
        /// 세션은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
        void Close();

        /// <summary>
        /// 주기적인 업데이트 호출
        /// 외부에서 주기적으로 (ex.30ms) 호출하여 내부로직을 처리해야 함
        /// </summary>
        /// <param name="elapsedTime">기존 업데이트로부터 지난 시간. 밀리세컨드(ms)</param>
        void Update(int elapsedTime);

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
        void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod);

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod);

        /// <summary>
        /// 패킷을 저수준에서 그대로 전송. 내부에서만 사용.
        /// </summary>
        /// <param name="poolingPacket">보낼패킷. NetPool.PacketPool.Alloc 으로 할당하여 사용하세요</param>
        /// <param name="deliveryMethod">전송 방법</param>
        void SendRawAsync(NetPacket poolingPacket, DeliveryMethod deliveryMethod);

        /// <summary>
        /// 요청을 보내고 답을 기다립니다.
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
        /// <param name="timeout">답을 기다리는 시간</param>
        /// <returns>요청에 대한 답 (데이터)</returns>
        Task<NetDataBufferReader> RequestAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod, TimeSpan? timeout);

        /// <summary>
        /// 요청을 보내고 답을 기다립니다.
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        /// <param name="timeout">답을 기다리는 시간</param>
        /// <returns>요청에 대한 답 (데이터)</returns>
        Task<NetDataBufferReader> RequestAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod, TimeSpan? timeout);

        /// <summary>
        /// 데이터를 받음. 데이터 처리가 끝날때까지 기다릴 수 있는 비동기 메서드
        /// </summary>
        /// <param name="dataReader">받은 데이터</param>
        Task OnReceive(NetDataReader dataReader);

        /// <summary>
        /// 에러가 발생되었음
        /// </summary>
        /// <param name="exception">예외</param>
        void OnError(Exception exception);
    }
}
