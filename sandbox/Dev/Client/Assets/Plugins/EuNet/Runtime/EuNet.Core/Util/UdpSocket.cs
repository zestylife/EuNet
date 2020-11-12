using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace EuNet.Core
{
    public class UdpSocket
    {
        public const int ReceivePollingTime = 500000; //0.5 second
        private const int SioUdpConnreset = -1744830452; //SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12
        private Socket _socket;
        private Thread _udpThread;
        private ILogger _logger;

        public int LocalPort { get; private set; }
        public volatile bool IsRunning;

        public short Ttl
        {
            get
            {
                return _socket.Ttl;
            }
            set
            {
                _socket.Ttl = value;
            }
        }

        public EndPoint RemoteEndPoint => _socket.RemoteEndPoint;
        public Socket Socket => _socket;
        public Action<byte[], int, SocketError, IPEndPoint> OnReceived;

        public UdpSocket(ILogger logger)
        {
            _logger = logger;
        }

        private bool IsActive()
        {
            return IsRunning;
        }

        public bool CreateClient()
        {
            if (IsActive())
                return false;

            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            if (!SetSocketOption(_socket, false))
                return false;

            // 가능한 포트를 찾아서 바인딩해보자
            IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Any, 10000);
            bool succeed = false;
            Random rand = new Random();
            int currentPort = rand.Next(10000, 65535);
            for (int i = 0; i < 50000; i++)
            {
                try
                {
                    bindEndPoint.Port = ((currentPort++) % 50000) + 10000;

                    _socket.Bind(bindEndPoint);
                    _logger.LogInformation($"Successfully udp binded to port : {((IPEndPoint)_socket.LocalEndPoint).Port}");
                    succeed = true;
                    break;
                }
                catch
                {

                }
            }

            if (succeed == false)
            {
                _logger.LogError($"Bind failed");
                return false;
            }

            IsRunning = true;

            _udpThread = new Thread(ReceiveLogic)
            {
                Name = "UdpSocketThread(" + LocalPort + ")",
                IsBackground = true
            };
            _udpThread.Start(_socket);

            return true;
        }

        public bool CreateServer(IPEndPoint bindEndPoint, bool reuseAddress)
        {
            if (IsActive())
                return false;

            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            if (!SetSocketOption(_socket, reuseAddress))
                return false;

            try
            {
                _socket.Bind(bindEndPoint);
                _logger.LogInformation($"Successfully udp binded to port : {((IPEndPoint)_socket.LocalEndPoint).Port}");
            }
            catch (SocketException bindException)
            {
                if(bindException.SocketErrorCode != SocketError.AddressFamilyNotSupported)
                {
                    _logger.LogError(bindException, $"Bind exception error code : {bindException.SocketErrorCode}");
                    return false;
                }
            }

            LocalPort = ((IPEndPoint)_socket.LocalEndPoint).Port;
            IsRunning = true;

            _udpThread = new Thread(ReceiveLogic)
            {
                Name = "UdpSocketThread(" + LocalPort + ")",
                IsBackground = true
            };
            _udpThread.Start(_socket);

            return true;
        }

        private bool SetSocketOption(Socket socket, bool reuseAddress)
        {
            socket.ReceiveTimeout = 500;
            socket.SendTimeout = 500;
            socket.ReceiveBufferSize = 1024 * 1024;
            socket.SendBufferSize = 1024 * 1024;

#if !UNITY || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#if NETSTANDARD || NETCOREAPP
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#endif
                try
                {
                    socket.IOControl(SioUdpConnreset, new byte[] { 0 }, null);
                }
                catch
                {

                }
#endif

            try
            {
                socket.ExclusiveAddressUse = !reuseAddress;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, reuseAddress);
            }
            catch
            {

            }

            Ttl = 255;

            try
            {
                socket.DontFragment = true;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, $"DontFragment error");
            }

            try
            {
                socket.EnableBroadcast = true;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, $"EnableBroadcast error");
            }

            return true;
        }

        public IPEndPoint GetLocalEndPoint()
        {
            IPEndPoint p = (IPEndPoint)_socket.LocalEndPoint;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(host.AddressList[i], p.Port);
                }
            }

            return null;
        }

        private void ReceiveLogic(object state)
        {
            Socket socket = (Socket)state;
            EndPoint bufferEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBuffer = new byte[NetPacket.MaxUdpPacketSize];

            while (IsActive())
            {
                int result;

                try
                {
                    if (socket.Available == 0 && !socket.Poll(ReceivePollingTime, SelectMode.SelectRead))
                        continue;

                    result = socket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref bufferEndPoint);

                    OnReceivedData(receiveBuffer, result, 0, (IPEndPoint)bufferEndPoint);
                    //OnReceived?.Invoke(receiveBuffer, result, 0, (IPEndPoint)bufferEndPoint);
                }
                catch (SocketException ex)
                {
                    switch (ex.SocketErrorCode)
                    {
#if UNITY_IOS && !UNITY_EDITOR
                        case SocketError.NotConnected:
#endif
                        case SocketError.Interrupted:
                        case SocketError.NotSocket:
                            return;
                        case SocketError.ConnectionReset:
                        case SocketError.MessageSize:
                        case SocketError.TimedOut:
                            _logger.LogInformation($"Ignored error : {ex.SocketErrorCode} - {ex.ToString()}");
                            break;
                        default:
                            _logger.LogError(ex, "Socket error");
                            break;
                    }
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ReceiveFrom error");
                    return;
                }
            }
        }

        public bool SendBroadcast(byte[] data, int offset, int size, int port)
        {
            if (!IsActive())
                return false;

            bool broadcastSuccess = false;
            bool multicastSuccess = false;

            try
            {
                broadcastSuccess = _socket.SendTo(
                    data,
                    offset,
                    size,
                    SocketFlags.None,
                    new IPEndPoint(IPAddress.Broadcast, port)) > 0;
            }
            catch (Exception)
            {
                //NetDebug.WriteError("[S][MCAST]" + ex);
                return broadcastSuccess;
            }

            return broadcastSuccess || multicastSuccess;
        }

        public int SendTo(byte[] data, int offset, int size, IPEndPoint remoteEndPoint, ref SocketError errorCode)
        {
            if (!IsActive())
                return 0;
            try
            {
                int result = _socket.SendTo(data, offset, size, SocketFlags.None, remoteEndPoint);
                return result;
            }
            catch (SocketException ex)
            {
                switch (ex.SocketErrorCode)
                {
                    case SocketError.NoBufferSpaceAvailable:
                    case SocketError.Interrupted:
                        return 0;
                    case SocketError.MessageSize: //do nothing              
                        break;
                    default:
                        //NetDebug.WriteError("[S]" + ex);
                        break;
                }
                errorCode = ex.SocketErrorCode;
                _logger.LogError(ex, "Socket error");
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendTo error");
                return -1;
            }
        }

        protected virtual void OnReceivedData(byte[] data, int size, SocketError error, IPEndPoint endPoint)
        {

        }

        public void Close(bool suspend)
        {
            if (!suspend)
            {
                IsRunning = false;
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }

                try
                {
                    _socket.Close();
                }
                catch
                {

                }

                _socket = null;
            }

            if (_udpThread != null && _udpThread != Thread.CurrentThread)
                _udpThread.Join();

            _udpThread = null;
        }
    }
}
