using System;
using System.Threading;

namespace EuNet.Core
{
    /// <summary>
    /// 채널 인터페이스
    /// 통신을 위한 채널을 만드는데 쓰인다
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// 초기화를 한다. 
        /// 채널은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
        /// <param name="cts">취소 토큰소스. IChannel.Close 되었을때 CancellationTokenSource.Cancel을 호출함</param>
        void Init(CancellationTokenSource cts);

        /// <summary>
        /// 리소스를 해제한다. 
        /// 채널은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
        void Close();

        /// <summary>
        /// 주기적인 업데이트 호출
        /// 외부에서 주기적으로 (ex.30ms) 호출하여 내부로직을 처리해야 함
        /// </summary>
        /// <param name="elapsedTime">기존 업데이트로부터 지난 시간. 밀리세컨드(ms)</param>
        /// <returns></returns>
        bool Update(int elapsedTime);

        /// <summary>
        /// 패킷을 전송. 내부적으로만 사용
        /// </summary>
        /// <param name="poolingPacket">보낼패킷. NetPool.PacketPool.Alloc 으로 할당하여 사용하세요</param>
        void SendAsync(NetPacket poolingPacket);

        /// <summary>
        /// 받은 패킷을 처리할 액션
        /// </summary>
        Action<IChannel, NetPacket> PacketReceived { get; set; }
    }
}
