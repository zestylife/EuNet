using EuNet.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EuNet.Server
{
    public class SessionManager
    {
        public int SessionCount => _sessionMap.Count;

        private ConcurrentDictionary<int, ISession> _sessionMap;
        private ISession[] _sessionList;

        public SessionManager(int maxSession)
        {
            _sessionMap = new ConcurrentDictionary<int, ISession>();
            _sessionList = new ISession[maxSession + 1];
        }

        public bool InsertSession(ISession session)
        {
            if (_sessionMap.TryAdd(session.SessionId, session) == false)
                return false;

            _sessionList[session.SessionId] = session;

            return true;
        }

        public bool RemoveSession(ISession session)
        {
            if (_sessionMap.TryRemove(session.SessionId, out var removedSession) == false)
                return false;

            _sessionList[session.SessionId] = null;

            //if (session != removedSession)
            //    throw new Exception("");

            return true;
        }

        public ISession FindSession(int sessionId)
        {
            if (_sessionMap.TryGetValue(sessionId, out var session) == false)
                return null;

            return session;
        }

        public void InvokeAllSession(Action<ISession> action)
        {
            foreach (var session in _sessionList)
            {
                var s = session;
                if (s == null)
                    continue;

                action(s);
            }
        }

        public async Task InvokeAllSessionAsync(Func<ISession, Task> action)
        {
            foreach (var session in _sessionList)
            {
                var s = session;
                if (s == null)
                    continue;

                await action(s);
            }
        }
    }
}