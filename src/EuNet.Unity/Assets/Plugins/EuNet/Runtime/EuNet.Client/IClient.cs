using System;
using System.Threading.Tasks;

namespace EuNet.Client
{
    /// <summary>
    /// 클라이언트 인터페이스
    /// </summary>
    public interface IClient : IDisposable
    {
        /// <summary>
        /// 접속을 비동기적으로 한다
        /// </summary>
        /// <param name="timeout">타임아웃</param>
        /// <returns>성공여부</returns>
        Task<bool> ConnectAsync(TimeSpan? timeout);

        /// <summary>
        /// 접속을 해제한다
        /// </summary>
        void Disconnect();
    }
}
