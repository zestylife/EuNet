using EuNet.Core;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EuNet.Server
{
    public class TcpListener : IListener
    {
        public ServerOption Options { get; }

        private Socket _listenSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskCompletionSource<bool> _stopTaskCompletionSource;
        private ILogger _logger;

        public TcpListener(ServerOption options, ILogger logger)
        {
            Options = options;
            _logger = logger;
        }

        public bool IsRunning { get; private set; }

        public bool Start()
        {
            var options = Options;

            try
            {
                var listenEndpoint = NetUtil.GetEndPoint(options.TcpServerAddress, options.TcpServerPort);
                var listenSocket = _listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, !options.TcpNoDelay);

                //TcpKeepAliveTime
                listenSocket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)3, 60);

                //TcpKeepAliveInterval
                listenSocket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)17, 3);

                //TcpKeepAliveRetryCount
                listenSocket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)16, 20);

                listenSocket.LingerState = new LingerOption(false, 0);

                listenSocket.Bind(listenEndpoint);
                listenSocket.Listen(options.TcpBackLog);

                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();

                KeepAccept(listenSocket).DoNotAwait();

                _logger.LogInformation($"Successfully tcp binded : {listenEndpoint}");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The listener failed to start.");
                return false;
            }
        }

        private async Task KeepAccept(Socket listenSocket)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var client = await listenSocket.AcceptAsync();
                    OnNewClientAccept(client);
                }
                catch (Exception)
                {
                    break;
                }
            }

            _stopTaskCompletionSource.TrySetResult(true);
        }

        public event NewClientAcceptHandler NewClientAccepted;

        private void OnNewClientAccept(Socket socket)
        {
            NewClientAccepted?.Invoke(this, socket);
        }

        public Task StopAsync()
        {
            var listenSocket = _listenSocket;

            if (listenSocket == null)
                return Task.Delay(0);

            _stopTaskCompletionSource = new TaskCompletionSource<bool>();

            _cancellationTokenSource.Cancel();
            listenSocket.Close();

            return _stopTaskCompletionSource.Task;
        }

        public override string ToString()
        {
            return Options?.ToString();
        }
    }
}
