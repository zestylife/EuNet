using System;
using System.Threading.Tasks;

namespace EuNet.Server
{
    /// <summary>
    /// 서버 인터페이스
    /// </summary>
    public interface IServer : IDisposable
    {
        /// <summary>
        /// 서버를 비동기적으로 시작
        /// </summary>
        /// <returns>성공여부</returns>
        Task<bool> StartAsync();

        /// <summary>
        /// 서버를 비동기적으로 정지
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 세션을 관리하는 세션 매니저
        /// </summary>
        SessionManager SessionManager { get; }

        /// <summary>
        /// P2P 그룹을 관리하는 P2P 매니저
        /// </summary>
        P2pManager P2pManager { get; }

        /// <summary>
        /// 현재 접속중인 세션수
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// 현재 서버 상태
        /// </summary>
        ServerState State { get; }
    }
}
