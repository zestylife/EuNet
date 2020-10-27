using System;
using System.Threading.Tasks;

namespace EuNet.Client
{
    public interface IClient : IDisposable
    {
        Task<bool> ConnectAsync(TimeSpan? timeout);
        void Disconnect();
    }
}
