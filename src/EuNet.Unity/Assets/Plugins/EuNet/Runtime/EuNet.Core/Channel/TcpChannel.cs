using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace EuNet.Core
{
    public class TcpChannel : ChannelBase
    {
        private Socket _socket;
        private List<NetPacket> _sendedList;
        private SendingQueue _sendedBufferList;
        private List<NetPacket> _sendWaitQueue;

        private SocketAsyncEventArgs _asyncEventArgsSend;
        private SocketAsyncEventArgs _asyncEventArgsReceive;

        private byte[] _receivedBuffer;
        private int _receivedSize;

        public TcpChannel(IChannelOption channelOption, ILogger logger, NetStatistic statistic)
            : base(channelOption, logger, statistic)
        {
            _asyncEventArgsSend = new SocketAsyncEventArgs();
            _asyncEventArgsSend.Completed += OnIoCompleted;
            _asyncEventArgsSend.UserToken = this;

            _receivedBuffer = new byte[NetPacket.MaxTcpPacketSize];

            _asyncEventArgsReceive = new SocketAsyncEventArgs();
            _asyncEventArgsReceive.Completed += OnIoCompleted;
            _asyncEventArgsReceive.UserToken = this;
            _asyncEventArgsReceive.SetBuffer(_receivedBuffer, 0, _receivedBuffer.Length);

            _sendedList = new List<NetPacket>();
            _sendWaitQueue = new List<NetPacket>();
            _sendedBufferList = new SendingQueue();

            _receivedSize = 0;
        }

        public override void Init(CancellationTokenSource cts)
        {
            base.Init(cts);

            lock (_sendedList)
            {
                _sendedList.Clear();
                _sendWaitQueue.Clear();
                _sendedBufferList.Clear();
            }

            _receivedSize = 0;
        }

        public override void Close()
        {
            base.Close();

            var socket = _socket;

            if (socket == null)
                return;

            if (Interlocked.CompareExchange(ref _socket, null, socket) == socket)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {

                }

                try
                {
                    socket.Close();
                }
                catch
                {

                }
            }
        }

        internal override void OnClosed()
        {
            base.OnClosed();

            try
            {
                _socket?.Close();

            }
            finally
            {
                _socket = null;
            }

            lock (_sendedList)
            {
                foreach (NetPacket packet in _sendedList)
                    NetPool.PacketPool.Free(packet);

                _sendedList.Clear();

                foreach (NetPacket packet in _sendWaitQueue)
                    NetPool.PacketPool.Free(packet);

                _sendWaitQueue.Clear();
                _sendedBufferList.Clear();
            }
        }

        public void SetSocket(Socket socket)
        {
            if (_socket != null)
                throw new Exception("already exist TcpAsyncChannel socket");

            _socket = socket;
            ReceiveAsync();
        }

        public override bool Update(int elapsedTime)
        {
            return true;
        }

        public override void SendAsync(NetPacket poolingPacket)
        {
            IPacketFilter filter = _channelOption.PacketFilter;
            while (filter != null)
            {
                filter.Encode(poolingPacket);
                filter = filter.NextFilter;
            }

            Interlocked.Increment(ref _statistic.TcpPacketSentCount);

            lock (_sendedList)
            {
                if (_sendedList.Count > 0)
                {
                    // 전송중임
                    _sendWaitQueue.Add(poolingPacket);
                    return;
                }

                _sendedBufferList.Push(new ArraySegment<byte>(poolingPacket.RawData, 0, poolingPacket.Size));
                _sendedList.Add(poolingPacket);
            }

            SendAsync(_sendedBufferList);
        }

        private void SendAsync(SendingQueue queue)
        {
            try
            {
                if (queue.Count > 1)
                {
                    if (_asyncEventArgsSend.Buffer != null)
                        _asyncEventArgsSend.SetBuffer(null, 0, 0);

                    _asyncEventArgsSend.BufferList = queue;
                }
                else
                {
                    if (_asyncEventArgsSend.BufferList != null)
                        _asyncEventArgsSend.BufferList = null;

                    var item = queue[0];
                    _asyncEventArgsSend.SetBuffer(item.Array, item.Offset, item.Count);
                }

                if (_socket == null)
                    return;

                //Interlocked.Add(ref _statistic.TcpSentCount, queue.Count);
                //Interlocked.Add(ref SendedCount, queue.Count);

                if (_socket.SendAsync(_asyncEventArgsSend) == false)
                {
                    OnSendCompleted(_asyncEventArgsSend);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception happened in SendAsync");
                Close();
            }
        }

        private void OnSendCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.BytesTransferred > 0)
            {
                Interlocked.Increment(ref _statistic.TcpSentCount);
                Interlocked.Add(ref _statistic.TcpSentBytes, e.BytesTransferred);
            }
            else
            {
                Close();
                return;
            }

            var total = _sendedBufferList.TotalSegmentCount;
            if (total != e.BytesTransferred)
            {
                // 덜 전송되었다!
                _logger.LogWarning($"sended {e.BytesTransferred}/{total} bytes");
                _sendedBufferList.LeftTrim(e.BytesTransferred);
                SendAsync(_sendedBufferList);
                return;
            }

            bool remainSend = false;

            lock (_sendedList)
            {
                // 다 보냈으니 지운다
                foreach (NetPacket packet in _sendedList)
                    NetPool.PacketPool.Free(packet);

                _sendedList.Clear();
                _sendedBufferList.Clear();

                if (_sendWaitQueue.Count > 0)
                {
                    foreach (NetPacket packet in _sendWaitQueue)
                    {
                        _sendedBufferList.Push(new ArraySegment<byte>(packet.RawData, 0, packet.Size));
                    }

                    List<NetPacket> tempList = _sendWaitQueue;
                    _sendWaitQueue = _sendedList;
                    _sendedList = tempList;

                    remainSend = true;
                }
            }

            if (remainSend == true)
            {
                // 못보낸거 보내자
                SendAsync(_sendedBufferList);
            }
        }

        private void OnReceiveCompleted(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0 && e.SocketError == SocketError.ConnectionReset)
            {
                Close();
            }
            else if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                Interlocked.Increment(ref _statistic.TcpReceivedCount);
                Interlocked.Add(ref _statistic.TcpReceivedBytes, e.BytesTransferred);

                _receivedSize += e.BytesTransferred;

                int readOffset = ReadPacket(_receivedBuffer, _receivedSize, _channelOption.PacketFilter);
                if (readOffset < 0)
                    return;

                if (_receivedSize - readOffset > 0)
                    Buffer.BlockCopy(_receivedBuffer, readOffset, _receivedBuffer, 0, _receivedSize - readOffset);

                _receivedSize -= readOffset;

                ReceiveAsync();
            }
            else
            {
                Close();
            }
        }

        public bool ReceiveAsync()
        {
            try
            {
                if (_socket == null)
                    return false;

                _asyncEventArgsReceive.SetBuffer(_receivedSize, _receivedBuffer.Length - _receivedSize);

                if (_socket.ReceiveAsync(_asyncEventArgsReceive) == false)
                {
                    OnReceiveCompleted(_asyncEventArgsReceive);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception happened in ReceiveAsync");
                Close();
                return false;
            }

            return true;
        }

        public void OnIoCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        OnReceiveCompleted(e);
                        break;
                    case SocketAsyncOperation.Send:
                        OnSendCompleted(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception happened in OnIoCompleted");
                Close();
            }
        }
    }
}
