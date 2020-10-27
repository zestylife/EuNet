namespace EuNet.Core
{
    public class P2pMemberBase
    {
        //! 세션
        private ISession _session;
        public ISession Session => _session;

        public ushort SessionId => _session != null ? _session.SessionId : ushort.MaxValue;
        public int Ping => _session != null && _session.UdpChannel != null ? _session.UdpChannel.Ping : int.MaxValue;
        public sbyte MasterPriority;

        public P2pMemberBase()
        {
            _session = null;
        }

        public virtual void Init()
        {
            lock (this)
            {
                _session = null;
                MasterPriority = 0;
            }
        }

        public virtual void Close()
        {
            lock (this)
            {
                _session?.Close();
                _session = null;
            }
        }

        public void SetSession(ISession session)
        {
            lock (this)
            {
                _session = session;
            }
        }

        public void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            _session?.SendAsync(data, offset, length, deliveryMethod);
        }

        public void SendAsync(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            _session?.SendAsync(writer, deliveryMethod);
        }
    }
}
