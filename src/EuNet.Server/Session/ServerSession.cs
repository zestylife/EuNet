using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

[assembly: InternalsVisibleTo("EuNet.Rpc")]
namespace EuNet.Core
{
    /// <summary>
    /// 서버 세션. 이 클래스를 상속받아서 유저 클래스를 사용하면 편리함.
    /// </summary>
    public class ServerSession : ISession
    {
        /// <summary>
        /// 세션 고유 아이디
        /// </summary>
        public ushort SessionId { get; }

        private TcpChannel _tcpChannel;
        public TcpChannel TcpChannel => _tcpChannel;

        private UdpChannel _udpChannel;
        public UdpChannel UdpChannel => _udpChannel;

        /// <summary>
        /// 현재 채널 상태
        /// </summary>
        public SessionState State { get; private set; } = SessionState.Initialized;

        /// <summary>
        /// 패킷을 처리하기 전에 전처리하는 콜백
        /// </summary>
        internal Func<ISession, NetPacket, bool> OnPreProcessPacket { get; set; }

        /// <summary>
        /// 데이터를 받아서 처리하는 콜백
        /// </summary>
        internal Func<ISession, NetDataReader, Task> OnReceived { get; set; }

        /// <summary>
        /// 요청을 받은 콜백
        /// </summary>
        internal Func<ISession, NetDataReader, NetDataWriter, Task> OnRequestReceived { get; set; }

        /// <summary>
        /// 에러가 발생되는 콜백
        /// </summary>
        internal Action<ISession, Exception> OnErrored { get; set; }

        private BufferBlock<NetPacket> _receivedPacketQueue;
        private CancellationTokenSource _cts;

        private long _connectId;
        /// <summary>
        /// 연결 인증키. 새롭게 연결될때마다 랜덤한 값이 생성됨. 이를 통해 세션인증가능
        /// </summary>
        public long ConnectId => _connectId;

        /// <summary>
        /// 현재 업데이트가 가능한지 여부. 세션이 닫히면 false가 되어 업데이트를 막음
        /// </summary>
        private volatile bool _isPossibleUpdate;

        private SessionRequest _request;
        public SessionRequest SessionRequest => _request;

        public ServerSession(SessionCreateInfo createInfo)
        {
            SessionId = createInfo.SessionId;
            _tcpChannel = createInfo.TcpChannel;
            if (_tcpChannel != null)
                _tcpChannel.PacketReceived = OnReceiveFromChannel;

            _udpChannel = createInfo.UdpChannel;
            if (_udpChannel != null)
                _udpChannel.PacketReceived = OnReceiveFromChannel;

            _request = new SessionRequest(this, createInfo.Statistic);
        }

        /// <summary>
        /// 세션을 초기화 한다
        /// 세션은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
        /// <param name="info">초기화시 정보</param>
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

        /// <summary>
        /// 세션을 닫는다
        /// 세션은 재활용될 수 있으므로 재활용을 고려하여야 한다
        /// </summary>
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

        /// <summary>
        /// 세션이 연결되었음
        /// </summary>
        protected virtual void OnConnected()
        {

        }

        /// <summary>
        /// 세션이 닫혔음
        /// </summary>
        protected virtual Task OnClosed()
        {
            NetPacket poolingPacket;
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

        /// <summary>
        /// 주기적인 업데이트 호출
        /// 외부에서 주기적으로 (ex.30ms) 호출하여 내부로직을 처리해야 함
        /// </summary>
        /// <param name="elapsedTime">기존 업데이트로부터 지난 시간. 밀리세컨드(ms)</param>
        public void Update(int elapsedTime)
        {
            if (_isPossibleUpdate == false)
                return;

            try
            {
                if(_tcpChannel?.Update(elapsedTime) == false)
                    throw new Exception("Disconnected due to TCP timeout");

                if (_udpChannel?.Update(elapsedTime) == false)
                    throw new Exception("Disconnected due to RUDP timeout");
            }
            catch (Exception ex)
            {
                _isPossibleUpdate = false;
                OnError(ex);
                Close();
            }
        }

        /// <summary>
        /// 전송방법에 따른 채널을 가져온다
        /// </summary>
        /// <param name="deliveryMethod">전송 방법</param>
        /// <returns>채널. 없다면 null</returns>
        private IChannel GetChannel(DeliveryMethod deliveryMethod)
        {
            if (deliveryMethod == DeliveryMethod.Tcp)
                return _tcpChannel;

            return _udpChannel;
        }

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
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

        /// <summary>
        /// 데이터를 전송함
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        public void SendAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            SendAsync(dataWriter.Data, 0, dataWriter.Length, deliveryMethod);
        }

        /// <summary>
        /// 요청을 보내고 답을 기다립니다.
        /// </summary>
        /// <param name="data">보낼 데이터 버퍼</param>
        /// <param name="offset">보낼 데이터 버퍼 오프셋</param>
        /// <param name="length">보낼 데이터 길이</param>
        /// <param name="deliveryMethod">전송 방법</param>
        /// <param name="timeout">답을 기다리는 시간</param>
        /// <returns>요청에 대한 답 (데이터)</returns>
        public Task<NetDataBufferReader> RequestAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            return _request.RequestAsync(data, offset, length, deliveryMethod, timeout);
        }

        /// <summary>
        /// 요청을 보내고 답을 기다립니다.
        /// </summary>
        /// <param name="dataWriter">보낼 데이터를 가지고 있는 NetDataWriter</param>
        /// <param name="deliveryMethod">전송 방법</param>
        /// <param name="timeout">답을 기다리는 시간</param>
        /// <returns>요청에 대한 답 (데이터)</returns>
        public Task<NetDataBufferReader> RequestAsync(NetDataWriter dataWriter, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            return RequestAsync(dataWriter.Data, 0, dataWriter.Length, deliveryMethod, timeout);
        }

        /// <summary>
        /// 패킷을 저수준에서 그대로 전송. 내부에서만 사용.
        /// </summary>
        /// <param name="poolingPacket">보낼패킷. NetPool.PacketPool.Alloc 으로 할당하여 사용하세요</param>
        /// <param name="deliveryMethod">전송 방법</param>
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

        /// <summary>
        /// 데이터를 받음. 데이터 처리가 끝날때까지 기다릴 수 있는 비동기 메서드
        /// </summary>
        /// <param name="dataReader">받은 데이터</param>
        public virtual async Task OnReceive(NetDataReader dataReader)
        {
            await OnReceived?.Invoke(this, dataReader);
        }

        /// <summary>
        /// 에러가 발생되었음
        /// </summary>
        /// <param name="exception">예외</param>
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
                                    await _request.OnReceive(packet.Property, packet.DeliveryMethod, reader, OnRequestReceived);
                                }
                                break;
                            case PacketProperty.ViewRequest:
                                {
                                    throw new Exception("Not support ViewRequest");
                                }
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
