using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EuNet.Core
{
    public class SessionRequest
    {
        public enum RequestType : byte
        {
            Notification = 1,
            Request = 2,
            ReplyWithException = 3,
            ReplyWithResult = 4,
            CustomMessage = 5,
        }

        private ISession _session;
        private int _lastRequestId;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<NetDataBufferReader>> _responseWaitingItems =
            new ConcurrentDictionary<int, TaskCompletionSource<NetDataBufferReader>>();

        public SessionRequest(ISession session)
        {
            _session = session;
        }

        public void Close()
        {
            foreach (var kvp in _responseWaitingItems)
                kvp.Value.TrySetCanceled();

            _responseWaitingItems.Clear();
            _lastRequestId = 0;
        }

        public Task<NetDataBufferReader> RequestAsync(byte[] data, int offset, int length, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            var tcs = new TaskCompletionSource<NetDataBufferReader>();
            int requestId;

            while (true)
            {
                requestId = ++_lastRequestId;
                if (requestId <= 0)
                    requestId = _lastRequestId = 1;

                var added = _responseWaitingItems.TryAdd(requestId, tcs);

                if (added)
                    break;
            }

            // 타임아웃 설정
            if (timeout != null && timeout.Value != Timeout.InfiniteTimeSpan && timeout.Value > default(TimeSpan))
            {
                var cancellationSource = new CancellationTokenSource();
                cancellationSource.Token.Register(() =>
                {
                    TaskCompletionSource<NetDataBufferReader> getTcs;
                    if (_responseWaitingItems.TryRemove(requestId, out getTcs))
                    {
                        getTcs.TrySetCanceled();
                    }
                });
                cancellationSource.CancelAfter(timeout.Value);
            }

            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write((byte)RequestType.Request);
                writer.Write(requestId);
                writer.WriteOnlyData(data, offset, length);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.Request, writer);
                packet.DeliveryMethod = deliveryMethod;

                _session.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
            
            return tcs.Task;
        }

        public void Notification(byte[] data, int offset, int length, DeliveryMethod deliveryMethod)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write((byte)RequestType.Notification);
                writer.Write(0);
                writer.WriteOnlyData(data, offset, length);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.Request, writer);
                packet.DeliveryMethod = deliveryMethod;

                _session.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        public Task<NetDataBufferReader> ViewRequestAsync(byte[] data, int offset, int length, int viewId, DeliveryMethod deliveryMethod, TimeSpan? timeout)
        {
            var tcs = new TaskCompletionSource<NetDataBufferReader>();
            int requestId;

            while (true)
            {
                requestId = ++_lastRequestId;
                if (requestId <= 0)
                    requestId = _lastRequestId = 1;

                var added = _responseWaitingItems.TryAdd(requestId, tcs);

                if (added)
                    break;
            }

            // 타임아웃 설정
            if (timeout != null && timeout.Value != Timeout.InfiniteTimeSpan && timeout.Value > default(TimeSpan))
            {
                var cancellationSource = new CancellationTokenSource();
                cancellationSource.Token.Register(() =>
                {
                    TaskCompletionSource<NetDataBufferReader> getTcs;
                    if (_responseWaitingItems.TryRemove(requestId, out getTcs))
                    {
                        getTcs.TrySetCanceled();
                    }
                });
                cancellationSource.CancelAfter(timeout.Value);
            }

            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write((byte)RequestType.Request);
                writer.Write(requestId);
                writer.Write(viewId);
                writer.WriteOnlyData(data, offset, length);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.ViewRequest, writer);
                packet.DeliveryMethod = deliveryMethod;

                _session.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }

            return tcs.Task;
        }

        public void ViewNotification(byte[] data, int offset, int length, int viewId, DeliveryMethod deliveryMethod)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write((byte)RequestType.Notification);
                writer.Write(0);
                writer.Write(viewId);
                writer.WriteOnlyData(data, offset, length);

                NetPacket packet = NetPool.PacketPool.Alloc(PacketProperty.ViewRequest, writer);
                packet.DeliveryMethod = deliveryMethod;

                _session.SendRawAsync(packet, packet.DeliveryMethod);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }

        public async Task OnReceive(PacketProperty packetProperty, NetDataReader reader, Func<ISession, NetDataReader, NetDataWriter, Task> onRequestReceive)
        {
            var requestType = (RequestType)reader.ReadByte();
            int requestId = reader.ReadInt32();

            switch(requestType)
            {
                case RequestType.Notification:
                    {
                        var writer = NetPool.DataWriterPool.Alloc();
                        try
                        {
                            await onRequestReceive(_session, reader, writer);
                        }
                        finally
                        {
                            NetPool.DataWriterPool.Free(writer);
                        }
                    }
                    break;
                case RequestType.Request:
                    {
                        var writer = NetPool.DataWriterPool.Alloc();
                        try
                        {
                            var prePos = writer.Length;

                            writer.Write((byte)RequestType.ReplyWithResult);
                            writer.Write(requestId);

                            try
                            {
                                await onRequestReceive(_session, reader, writer);
                            }
                            catch (Exception ex)
                            {
                                writer.Length = prePos;
                                writer.Write((byte)RequestType.ReplyWithException);
                                writer.Write(requestId);
                                writer.Write(ex.ToString());
                            }

                            NetPacket packet = NetPool.PacketPool.Alloc(packetProperty, writer);
                            packet.DeliveryMethod = DeliveryMethod.Tcp;

                            _session.SendRawAsync(packet, packet.DeliveryMethod);
                        }
                        finally
                        {
                            NetPool.DataWriterPool.Free(writer);
                        }
                    }
                    break;
                case RequestType.ReplyWithResult:
                    {
                        TaskCompletionSource<NetDataBufferReader> getTcs;
                        if (_responseWaitingItems.TryRemove(requestId, out getTcs) == false)
                            return;

                        NetDataBufferReader remainReader = new NetDataBufferReader(reader);
                        getTcs.TrySetResult(remainReader);
                    }
                    break;
                case RequestType.ReplyWithException:
                    {
                        TaskCompletionSource<NetDataBufferReader> getTcs;
                        if (_responseWaitingItems.TryRemove(requestId, out getTcs) == false)
                            return;

                        getTcs.TrySetException(new Exception(reader.ReadString()));
                    }
                    break;
            }
        }
    }
}
