using EuNet.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EuNet.Server
{
    public class DefaultSessionFactory : ISessionFactory
    {
        private ConcurrentQueue<ISession> _sessionQueue;
        private readonly int _maxSession;

        public DefaultSessionFactory(
            ServerOption serverOption,
            ILoggerFactory loggerFactory,
            NetStatistic statistic,
            Func<ushort, TcpChannel, UdpChannel, ServerSession> createSessionFunc = null)
        {
            createSessionFunc = createSessionFunc ?? CreateSession;

            _sessionQueue = new ConcurrentQueue<ISession>();
            _maxSession = serverOption.MaxSession;

            for (int i = 0; i < _maxSession; ++i)
            {
                TcpChannel tcpChannel = new TcpChannel(
                    serverOption,
                    loggerFactory.CreateLogger(nameof(TcpChannel)),
                    statistic);

                UdpChannel udpChannel = null;

                if (serverOption.IsServiceUdp)
                {
                    udpChannel = new UdpChannel(
                        serverOption,
                        loggerFactory.CreateLogger(nameof(UdpChannel)),
                        statistic,
                        0);
                }

                ServerSession session = createSessionFunc((ushort)(i + 1), tcpChannel, udpChannel);

                _sessionQueue.Enqueue(session);
            }
        }

        private ServerSession CreateSession(ushort sessionId, TcpChannel tcpChannel, UdpChannel udpChannel)
        {
            return new ServerSession(sessionId, tcpChannel, udpChannel);
        }

        public ISession Create()
        {
            ISession session = null;
            if (_sessionQueue.TryDequeue(out session) == false ||
                session == null)
                return null;

            return session;
        }

        public bool Release(ISession session)
        {
#if DEBUG
            if (_sessionQueue.Contains(session) == true)
            {
                // 중복되어 들어갔으므로 로그를 남기자
                return false;
            }
#endif
            _sessionQueue.Enqueue(session);
            return true;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }
    }
}
