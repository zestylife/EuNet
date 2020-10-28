using EuNet.Core;
using StarterCommon;
using System;
using System.Threading.Tasks;

namespace StarterServer
{
    public partial class UserSession : ServerSession
    {
        private UserInfo _userInfo;

        public UserSession(ushort sessionId, TcpChannel tcpChannel, UdpChannel udpChannel)
            : base(sessionId, tcpChannel, udpChannel)
        {

        }

        protected override void OnConnected()
        {
            base.OnConnected();

            _userInfo = null;
        }

        protected override Task OnClosed()
        {
            return base.OnClosed();
        }

        public override void OnError(Exception ex)
        {
            base.OnError(ex);
            Console.Error.WriteLine(ex.ToString());
        }
    }
}
