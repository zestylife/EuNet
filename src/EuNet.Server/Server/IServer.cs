using System;
using System.Threading.Tasks;

namespace EuNet.Server
{
    public interface IServer : IDisposable
    {
        Task<bool> StartAsync();
        Task StopAsync();

        SessionManager SessionManager { get; }
        P2pManager P2pManager { get; }
        int SessionCount { get; }
        ServerState State { get; }
    }
}
