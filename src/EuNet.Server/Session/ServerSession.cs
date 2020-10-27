using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

[assembly: InternalsVisibleTo("EuNet.Rpc")]
namespace EuNet.Core
{
    public class ServerSession : ISession
    {
        public ushort SessionId { get; }

        private TcpChannel _tcpChannel;
        public TcpChannel TcpChannel => _tcpChannel;

        private UdpChannel _udpChannel;
        public UdpChannel UdpChannel => _udpChannel;

        public SessionState State { get; private set; } = SessionState.Initialized;

        // 패킷을 넘기기전에 미리 처리해야 할 함수
        internal Func<ISession, NetPacket, bool> OnPreProcessPacket { get; set; }

        // 데이터를 받았음
        internal Func<ISession, NetDataReader, Task> OnReceived { get; set; }
        internal Func<ISession, NetDataReader, NetDataWriter, Task> OnRequestReceived { get; set; }
        internal Action<ISession, Exception> OnErrored { get; set; }

        private BufferBlock<NetPacket> _receivedPacketQueue;
        private CancellationTokenSource _cts;

        // 연결인증키
        private long _connectId;
        public long ConnectId => _connectId;

        // 업데이트가 가능한 시점인가?
        private volatile bool _isPossibleUpdate;

        private SessionRequest _request;
        public SessionRequest SessionRequest => _request;

        public ServerSession(ushort sessionId, TcpChannel tcpChannel, UdpChannel udpChannel)
        {
            SessionId = sessionId;
            _tcpChannel = tcpChannel;
            if (_tcpChannel != null)
                _tcpChannel.PacketReceived = OnReceiveFromChannel;

            _udpChannel = udpChannel;
            if (_udpChannel != null)
                _udpChannel.PacketReceived = OnReceiveFromChannel;

            _request = new SessionRequest(this);
        }

        public void Init(SessionInitializeInfo info)
        {
            _receivedPacketQueue = new BufferBlock<NetPacket>();
            _cts = new CancellationTokenSource();

            int low = Guid.NewGuid().GetHashCode();
            long high = (long)Guid.NewGuid().GetHashCode() << 32;
            _connectId = (long)low | high;

            State = SessionState.Initialized;

            if (_tcpChannel != null)
            {
                _tcpChannel.Init(_cts);
                _tcpChannel.SetSocket(info.AcceptedTcpSocket);
            }

            if (_udpChannel != null)
            {
                _udpChannel.Init(_cts);
                _udpChannel.SetSocket(info.UdpServiceSocket);
            }
        }

        public virtual void Close()
        {
            _receivedPacketQueue.Complete();
            _cts.Cancel();

            try
            {
                _tcpChannel?.Close();
                _udpChannel?.Close();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        protected virtual void OnConnected()
        {

        }

        protected virtual Task OnClosed()
        {
            NetPacket poolingPacket = null;
            while (_receivedPacketQueue.TryReceive(out poolingPacket) == true &&
                poolingPacket != null)
            {
                NetPool.PacketPool.Free(poolingPacket);
            }

            _cts?.Dispose();
            _cts = null;

            try
            {
                _tcpChannel?.OnClosed();
                _udpChannel?.OnClosed();
                _request.Close();
            }
            catch
            {

            }

            return Task.CompletedTask;
        }

        internal void OnSessionConnected()
        {
            State = SessionState.Connected;
            OnConnected();
        }

        internal async Task OnSessionClosed()
        {
            await OnClosed();
            State = SessionState.Closed;
        }

        public void Update(int elapsedTime)
        {
            if (_isPossibleUpdate == false)
                return;

            try
            {
                _tcpChannel?.Update(elapsedTime);

                if (_udpChannel?.Update(elapsedTime) == false)
                    throw new Exception("rudp disconnect timeout");
            }
            catch (Exception ex)
            {
                OnError(ex);
                Close();
            }
        }

        private IChannel GetChannel(DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                return _tcpChannel;

            return _udpChannel;
        }

        public void SendAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            if (State != SessionState.Connected)
                return;

            IChannel channel = GetChannel(deliveryMethod);
            if (channel == null)
                throw new Exception($"can not found channel : {deliveryMethod}");

            PacketProperty property = PacketProperty.UserData;
            int headerSize = NetPacket.GetHeaderSize(property);

            NetPacket packet = NetPool.PacketPool.Alloc(headerSize + length);
            packet.Property = property;
            packet.DeliveryMethod = deliveryMethod;

            Buffer.BlockCopy(data, offset, packet.RawData, headerSize, length);

            channel.SendAsync(packet);
        }

        public void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            SendAsync(dataWriter.Data, 0, dataWriter.Length, deliveryMethod);
        }

        public Task<NetDataBufferReader> RequestAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            return _request.RequestAsync(data, offset, length, deliveryMethod, timeout);
        }

        public Task<NetDataBufferReader> RequestAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            return RequestAsync(dataWriter.Data, 0, dataWriter.Length, deliveryMethod, timeout);
        }

        public void SendRawAsync(NetPacket poolingPacket, DeliveryMethod deliveryMethod)
        {
            IChannel channel = GetChannel(deliveryMethod);
            if (channel == null)
                throw new Exception($"can not found channel : {deliveryMethod}");

            channel.SendAsync(poolingPacket);
        }

        private void OnReceiveFromChannel(IChannel ch, NetPacket poolingPacket)
        {
            if (State == SessionState.Closed)
            {
                NetPool.PacketPool.Free(poolingPacket);
                return;
            }

            if (OnPreProcessPacket(this, poolingPacket) == true)
                return;

            _receivedPacketQueue.Post(poolingPacket);
        }

        public virtual async Task OnReceive(NetDataReader dataReader)
        {
            await OnReceived?.Invoke(this, dataReader);
        }

        public virtual void OnError(Exception exception)
        {
            OnErrored?.Invoke(this, exception);
        }

        private async Task ProcessPacketAsync()
        {
            NetDataReader reader = new NetDataReader();

            while (_cts.IsCancellationRequested == false)
            {
                NetPacket packet = null;
                try
                {
                    packet = await _receivedPacketQueue.ReceiveAsync(_cts.Token);
                }
                catch
                {
                    break;
                }

                if (packet == null)
                    continue;

                do
                {
                    try
                    {
                        int headerSize = NetPacket.GetHeaderSize(packet.Property);
                        reader.SetSource(packet.RawData, headerSize, packet.Size);

                        switch(packet.Property)
                        {
                            case PacketProperty.Request:
                                {
                                    await _request.OnReceive(packet.Property, reader, OnRequestReceived);
                                }
                                break;
                            case PacketProperty.ViewRequest:
                                {
                                    throw new Exception("Not support ViewRequest");
                                }
                                break;
                            default:
                                {
                                    await OnReceive(reader);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                        break;
                    }
                    finally
                    {
                        NetPool.PacketPool.Free(packet);
                    }
                }
                while (_receivedPacketQueue.TryReceive(out packet) == true && packet != null);
            }
        }

        internal async Task RunAsync()
        {
            // 첫 접속 패킷을 보내자
            NetPacket sendPacket = NetPool.PacketPool.Alloc(PacketProperty.ResponseConnection, 8);
            sendPacket.SessionIdForConnection = SessionId;
            FastBitConverter.GetBytes(sendPacket.RawData, 5, _connectId);

            SendRawAsync(sendPacket, DeliveryMethod.Tcp);

            _isPossibleUpdate = true;

            try
            {
                await ProcessPacketAsync();
            }
            finally
            {
                _isPossibleUpdate = false;
                Close();
            }
        }
    }
}
