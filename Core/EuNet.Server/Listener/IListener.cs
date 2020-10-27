using System.Net.Sockets;
using System.Threading.Tasks;

namespace EuNet.Server
{
    public delegate void NewClientAcceptHandler(IListener listener, Socket socket);

    public interface IListener
    {
        ServerOption Options { get; }
        bool Start();
        event NewClientAcceptHandler NewClientAccepted;

        Task StopAsync();
        bool IsRunning { get; }
    }
}
