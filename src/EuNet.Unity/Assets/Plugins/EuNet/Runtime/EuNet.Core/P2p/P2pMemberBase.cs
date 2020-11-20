namespace EuNet.Core
{
    /// <summary>
    /// P2P 멤버의 기본 클래스
    /// </summary>
    public class P2pMemberBase
    {
        private ISession _session;

        /// <summary>
        /// 세션. 데이터 전송을 위해서 사용됨
        /// </summary>
        public ISession Session => _session;

        /// <summary>
        /// 세션아이디. 없다면 ushort.MaxValue
        /// </summary>
        public ushort SessionId => _session != null ? _session.SessionId : ushort.MaxValue;

        /// <summary>
        /// 핑. 없다면 int.MaxValue
        /// </summary>
        public int Ping => _session != null && _session.UdpChannel != null ? _session.UdpChannel.Ping : int.MaxValue;

        /// <summary>
        /// 마스터 우선순위
        /// 마스터 자동선정 때 사용함.
        /// 다른 P2P 멤버들과 우선순위를 비교함. (우선순위가 가장 중요하며, 같다면 Ping을 비교함)
        /// </summary>
        public sbyte MasterPriority;

        public P2pMemberBase()
        {
            _session = null;
        }

        /// <summary>
        /// 재사용을 위한 초기화
        /// </summary>
        public virtual void Init()
        {
            lock (this)
            {
                _session = null;
                MasterPriority = 0;
            }
        }

        /// <summary>
        /// 리소스를 제거함
        /// </summary>
        public virtual void Close()
        {
            lock (this)
            {
                _session?.Close();
                _session = null;
            }
        }

        /// <summary>
        /// 데이터 전송을 위한 세션을 지정함
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(ISession session)
        {
            lock (this)
            {
                _session = session;
            }
        }

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
        public void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            _session?.SendAsync(data, offset, length, deliveryMethod);
        }

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        public void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            _session?.SendAsync(dataWriter, deliveryMethod);
        }
    }
}
